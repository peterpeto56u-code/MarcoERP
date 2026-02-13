
using BCrypt.Net;
var storedHash = "$2a$12$K.yeXoucE83pQps.IA88l.22Y3OjSubSJn7rg7LizUenn0.YnDbjW";
var password = "Admin@123456";
Console.WriteLine(BCrypt.Verify(password, storedHash));
