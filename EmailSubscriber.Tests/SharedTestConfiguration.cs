using Xunit;

// Serilog Static Logger (Log.Logger) paralel test koşumlarında "The logger is already frozen"
// hatası fırlattığı için testlerin birbirini ezmemesi adına xUnit paralelleştirmesi kapatılıyor.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace EmailSubscriber.Tests;

internal static class TestConfiguration
{
    // Generated for this test process; independent of local or deployed secrets.
    internal static readonly string JwtSecret = Convert.ToHexString(
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

    internal static readonly string AdminPassword = Convert.ToHexString(
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));

    internal static readonly string AdminPasswordHash =
        BCrypt.Net.BCrypt.HashPassword(AdminPassword);
}
