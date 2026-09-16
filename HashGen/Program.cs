using System.Text;

Console.Write("Enter a new admin password (input hidden): ");
var password = new StringBuilder();

while (true)
{
    var key = Console.ReadKey(intercept: true);
    if (key.Key == ConsoleKey.Enter)
        break;
    if (key.Key == ConsoleKey.Backspace)
    {
        if (password.Length > 0)
            password.Length--;
    }
    else if (!char.IsControl(key.KeyChar))
    {
        password.Append(key.KeyChar);
    }
}

Console.WriteLine();
if (password.Length < 12)
{
    Console.Error.WriteLine("Use a password with at least 12 characters.");
    return 1;
}

// Only the hash is printed. Never log or embed the plaintext password.
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(password.ToString()));
password.Clear();
return 0;
