using EmailSubscriber.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

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
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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
    builder.Services.AddRateLimiter(_ => { });

    builder.Services.AddControllers();

    // ─── Build App ───────────────────────────────────────────────────────────
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();  // req. 4.3 — HTTPS zorunluluğu
    app.UseCors("FrontendPolicy");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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
