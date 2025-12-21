using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Entities.RoleAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.RoleServices;

public class RoleService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RoleService>? _logger;

    public RoleService(AppDbContext dbContext, ILogger<RoleService>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<RoleRootEntity?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting role by id {Id}", id);
            throw;
        }
    }

    public async Task<List<RoleRootEntity>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Roles.ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all roles");
            throw;
        }
    }

    public async Task AddRoleAsync(RoleRootEntity role, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Roles.AddAsync(role, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding role");
            throw;
        }
    }

    public async Task UpdateRoleAsync(RoleRootEntity role, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating role");
            throw;
        }
    }

    public async Task RemoveRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (role == null) throw new KeyNotFoundException($"Role with id {id} not found");

            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing role");
            throw;
        }
    }
}
