using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmailSubscriber.API.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public Task<string?> LoginAsync(string username, string password)
    {
        var adminUsername = _configuration["Admin:Username"];
        var adminHash = _configuration["Admin:PasswordHash"];

        if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminHash))
        {
            return Task.FromResult<string?>(null);
        }

        if (username != adminUsername)
        {
            return Task.FromResult<string?>(null);
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, adminHash);

        if (!isPasswordValid)
        {
            return Task.FromResult<string?>(null);
        }

        // Generate JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult<string?>(tokenHandler.WriteToken(token));
    }

    public async Task<IEnumerable<Subscriber>> GetSubscribersAsync(string? search, bool? isActive, bool? isConfirmed)
    {
        var query = _context.Subscribers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => s.Email.Contains(search) || (s.Name != null && s.Name.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (isConfirmed.HasValue)
        {
            query = query.Where(s => s.IsConfirmed == isConfirmed.Value);
        }

        // Güvenlik için tokenları boşaltıyoruz (DTO kullanmadığımız için entity üzerinde null yapıp gönderiyoruz ama save etmiyoruz).
        // Daha iyi yaklaşım DTO dönmek.
        var list = await query.OrderByDescending(s => s.SubscribedAt).ToListAsync();
        
        list.ForEach(s => 
        {
            s.ConfirmationToken = null;
            s.ConfirmationTokenExpiresAt = null;
        });

        return list;
    }

    public async Task<bool> DeactivateSubscriberAsync(int id)
    {
        var subscriber = await _context.Subscribers.FindAsync(id);
        if (subscriber == null) return false;

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSubscriberAsync(int id)
    {
        var subscriber = await _context.Subscribers.FindAsync(id);
        if (subscriber == null) return false;

        _context.Subscribers.Remove(subscriber);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetStatsAsync()
    {
        var total = await _context.Subscribers.CountAsync();
        var active = await _context.Subscribers.CountAsync(s => s.IsActive);
        var unconfirmed = await _context.Subscribers.CountAsync(s => !s.IsConfirmed);
        
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        var today = await _context.Subscribers.CountAsync(s => s.SubscribedAt >= todayStart && s.SubscribedAt < todayEnd);

        return new { total, active, unconfirmed, today };
    }

    public async Task<object> GetGrowthChartAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);

        var data = await _context.Subscribers
            .Where(s => s.SubscribedAt >= thirtyDaysAgo)
            .GroupBy(s => s.SubscribedAt.Date)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .OrderBy(x => x.date)
            .ToListAsync();

        return data;
    }
}
