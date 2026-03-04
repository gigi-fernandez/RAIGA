using System.ComponentModel.DataAnnotations;

namespace PinkNightmares.Models;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }
    public int CreditCount { get; set; }
}

public enum UserRole
{
    Free,
    Member,
    Admin
}