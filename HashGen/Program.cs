using System;

class Program
{
    static void Main(string[] args)
    {
        string password = args.Length > 0 ? args[0] : "AdminPass2026!SecureKey#";
        string hash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"BCrypt Hash: {hash}");
    }
}
