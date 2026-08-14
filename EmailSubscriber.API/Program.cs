using EmailSubscriber.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

// ─── Serilog bootstrap logger ───────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog ─────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext());

    // ─── Database (MySQL + EF Core) ──────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("Default");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 32))));

    // ─── Reverse Proxy / Forwarded Headers Configuration ─────────────────────
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        // Yalnızca açıkça yapılandırılmış proxy ve ağlara güvenilir.
        // Doğrudan dış istemcilerin X-Forwarded-For başlığıyla IP spoofing yapması ve rate limiter'ı atlatması engellenir.
        var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();
        if (knownProxies != null && knownProxies.Length > 0)
        {
            options.KnownProxies.Clear();
            foreach (var proxy in knownProxies)
            {
                if (System.Net.IPAddress.TryParse(proxy, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
            }
        }

        var knownNetworks = builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>();
        if (knownNetworks != null && knownNetworks.Length > 0)
        {
            options.KnownNetworks.Clear();
            foreach (var network in knownNetworks)
            {
                var parts = network.Split('/');
                if (parts.Length == 2 && System.Net.IPAddress.TryParse(parts[0], out var ip) && int.TryParse(parts[1], out var prefix))
                {
                    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(ip, prefix));
                }
            }
        }
    });

    // ─── JWT Authentication ──────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["Jwt:Secret"];
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    {
        throw new InvalidOperationException("Jwt:Secret is not configured or is shorter than 32 characters (256-bit).");
    }
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = builder.Configuration["Jwt:Issuer"],
                ValidAudience            = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });
    builder.Services.AddAuthorization();

    // ─── CORS ────────────────────────────────────────────────────────────────
    var frontendUrls = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:5173", "http://localhost:5174" };
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
            policy.WithOrigins(frontendUrls)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    // ─── Rate Limiting (IP Bazlı Partitioning & Brute-Force Koruması) ──────────
    builder.Services.AddRateLimiter(options =>
    {
        // 1. Genel API limiter (İstemci IP'si başına 100 req/dk)
        options.AddPolicy("Api", context =>
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        // 2. Admin Login limiter (İstemci IP'si başına 5 req/dk - Kaba kuvvet engelleme)
        options.AddPolicy("Login", context =>
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        // 3. Abone Kayıt (Subscribe) limiter (İstemci IP'si başına 10 req/dk - Spam ve E-posta Bombardımanı Koruması)
        options.AddPolicy("Subscribe", context =>
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync("{\"message\": \"Çok fazla istek gönderildi. Lütfen bir süre sonra tekrar deneyin.\"}", token);
        };
    });

    builder.Services.AddSingleton<EmailSubscriber.API.Queue.IEmailQueueService, EmailSubscriber.API.Queue.EmailQueueService>();
    builder.Services.AddHostedService<EmailSubscriber.API.Queue.EmailWorker>();
    builder.Services.AddHostedService<EmailSubscriber.API.Services.Jobs.AINewsletterJob>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.IEmailService, EmailSubscriber.API.Services.MailKitEmailService>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.IEmailTemplateService, EmailSubscriber.API.Services.EmailTemplateService>();
    builder.Services.AddHttpClient<EmailSubscriber.API.Services.AI.IAIService, EmailSubscriber.API.Services.AI.GeminiService>()
        .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(90));

    builder.Services.AddScoped<EmailSubscriber.API.Services.ISubscriberService, EmailSubscriber.API.Services.SubscriberService>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.IAdminService, EmailSubscriber.API.Services.AdminService>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.ITrackingService, EmailSubscriber.API.Services.TrackingService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EmailSubscriber API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Örnek: 'Bearer {token}'",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ─── Build App ───────────────────────────────────────────────────────────
    var app = builder.Build();

    app.UseForwardedHeaders();
    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // Security Headers (XSS, Clickjacking, Sniffing & CSP)
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data: https:; script-src 'self'; style-src 'self' 'unsafe-inline'; font-src 'self' data: https:;";
        await next();
    });

    app.UseHttpsRedirection();  // req. 4.3 — HTTPS zorunluluğu
    
    // Swagger sadece Development ortamında aktif edilir (Production ortamında kapatılır)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmailSubscriber API v1"));
    }
    
    // Enable serving static files from wwwroot
    app.UseStaticFiles();

    app.UseRouting();
    app.UseCors("FrontendPolicy");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ─── Health Check ─────────────────────────────────────────────────────────
    app.MapGet("/health", () => Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow }));

    // ─── Test Verisi Temizliği (Geliştirme Ortamı) ───────────────────────────
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var testCampaigns = await db.Campaigns
                .Where(c => c.Subject.StartsWith("Test") || c.Subject.StartsWith("[TEST]") || c.Subject.StartsWith("Geri Bildirim Testi"))
                .ToListAsync();

            if (testCampaigns.Any())
            {
                var testCampaignIds = testCampaigns.Select(c => c.Id).ToList();
                var links = await db.TrackedLinks.Where(l => testCampaignIds.Contains(l.CampaignId)).ToListAsync();
                var recipients = await db.CampaignRecipients.Where(r => testCampaignIds.Contains(r.CampaignId)).ToListAsync();
                var feedbacks = await db.CampaignFeedbacks.Where(f => testCampaignIds.Contains(f.CampaignId)).ToListAsync();

                db.TrackedLinks.RemoveRange(links);
                db.CampaignRecipients.RemoveRange(recipients);
                db.CampaignFeedbacks.RemoveRange(feedbacks);
                db.Campaigns.RemoveRange(testCampaigns);
            }

            var testSubs = await db.Subscribers.Where(s => s.Email.EndsWith("@test.com")).ToListAsync();
            if (testSubs.Any())
            {
                db.Subscribers.RemoveRange(testSubs);
            }

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Test verisi temizliği sırasında uyarı oluştu.");
        }
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Uygulama başlatılamadı.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }