using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IDbContext
{
    DbSet<Flight> Flights { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
