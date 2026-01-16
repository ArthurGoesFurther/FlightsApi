namespace Domain.Entities;

public sealed class Role
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;

    public ICollection<User>? Users { get; set; }
}