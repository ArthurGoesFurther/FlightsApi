namespace Domain.Entities;

public sealed class Role
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;

    // Navigation property for clarity
    public ICollection<User>? Users { get; set; }
}