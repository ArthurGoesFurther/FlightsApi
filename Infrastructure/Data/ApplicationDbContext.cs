using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext, IDbContext
{
    private readonly Func<string?>? _currentUserProvider;
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        Func<string?>? currentUserProvider = null,
        ILogger<ApplicationDbContext>? logger = null)
        : base(options)
    {
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Origin).HasMaxLength(256);
            entity.Property(e => e.Destination).HasMaxLength(256);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(256);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(256);
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var user = _currentUserProvider?.Invoke() ?? "<system>";
        var time = DateTime.UtcNow;

        try
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var entityName = entry.Entity?.GetType().Name ?? "<unknown>";
                var state = entry.State.ToString();

                if (entry.State == EntityState.Added)
                {
                    _logger?.LogInformation("DB Change - {Time:u} User={User} Entity={Entity} State={State} NewValues={NewValues}",
                        time, user, entityName, state, SerializePropertyValues(entry.CurrentValues));
                }
                else if (entry.State == EntityState.Deleted)
                {
                    _logger?.LogInformation("DB Change - {Time:u} User={User} Entity={Entity} State={State} OldValues={OldValues}",
                        time, user, entityName, state, SerializePropertyValues(entry.OriginalValues));
                }
                else if (entry.State == EntityState.Modified)
                {
                    var changes = GetModifiedProperties(entry);
                    _logger?.LogInformation("DB Change - {Time:u} User={User} Entity={Entity} State={State} Changes={Changes}",
                        time, user, entityName, state, JsonSerializer.Serialize(changes));
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving changes to DB. User={User} Time={Time}", user, time);
            throw;
        }
    }

    private static object SerializePropertyValues(PropertyValues values)
    {
        try
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in values.Properties)
            {
                dict[prop.Name] = values[prop.Name];
            }
            return dict;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return "<unable to serialize>";
        }
    }

    private static IDictionary<string, object?> GetModifiedProperties(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.IsModified)
            {
                changes[prop.Metadata.Name] = new { Original = prop.OriginalValue, Current = prop.CurrentValue };
            }
        }
        return changes;
    }
}
