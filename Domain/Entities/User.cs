namespace Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // hashed password
    public int RoleId { get; set; }

    // Navigation property kept optional and as POCO reference
    public Role? Role { get; set; }
}