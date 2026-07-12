using System;

var hash1 = BCrypt.Net.BCrypt.HashPassword("Admin@2025!", 12);
var hash2 = BCrypt.Net.BCrypt.HashPassword("Transport@2025!", 12);
Console.WriteLine(hash1);
Console.WriteLine(hash2);
