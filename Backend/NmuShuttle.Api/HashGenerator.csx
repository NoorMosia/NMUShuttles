// Temporary script to generate BCrypt hashes
using System;

// We'll use a different approach - create a small program
class Program
{
    static void Main()
    {
        var hash1 = BCrypt.Net.BCrypt.HashPassword("Admin@2025!", 12);
        var hash2 = BCrypt.Net.BCrypt.HashPassword("Transport@2025!", 12);
        Console.WriteLine($"Admin@2025! => {hash1}");
        Console.WriteLine($"Transport@2025! => {hash2}");
    }
}
