using EmailSubscriber.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    // ─── JWT Authentication ──────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
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
    var frontendUrl = builder.Configuration["App:BaseUrl"] ?? "http://localhost:5173";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
            policy.WithOrigins(frontendUrl)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    // ─── Rate Limiting (Faz 1'de genişletilecek) ─────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("Api", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 100;
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0; // Kuyrukta bekleme yok, direkt reddet.
        });
        
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    builder.Services.AddSingleton<EmailSubscriber.API.Queue.IEmailQueueService, EmailSubscriber.API.Queue.EmailQueueService>();
    builder.Services.AddHostedService<EmailSubscriber.API.Queue.EmailWorker>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.IEmailService, EmailSubscriber.API.Services.MailKitEmailService>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.IEmailTemplateService, EmailSubscriber.API.Services.EmailTemplateService>();

    builder.Services.AddScoped<EmailSubscriber.API.Services.ISubscriberService, EmailSubscriber.API.Services.SubscriberService>();
    builder.Services.AddScoped<EmailSubscriber.API.Services.IAdminService, EmailSubscriber.API.Services.AdminService>();

    builder.Services.AddControllers();

    // ─── Build App ───────────────────────────────────────────────────────────
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();  // req. 4.3 — HTTPS zorunluluğu
    app.UseCors("FrontendPolicy");
    app.UseRateLimiter(); // Apply general rate limiter if needed, but we apply to endpoints
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers().RequireRateLimiting("Api");

    // ─── Health Check ─────────────────────────────────────────────────────────
    app.MapGet("/health", () => Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow }));

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