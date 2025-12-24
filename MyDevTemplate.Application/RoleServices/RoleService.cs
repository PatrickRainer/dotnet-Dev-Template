using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Entities.RoleAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.RoleServices;

public class RoleService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RoleService>? _logger;
    private readonly IValidator<RoleRoot> _validator;

    public RoleService(AppDbContext dbContext, IValidator<RoleRoot> validator, ILogger<RoleService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<RoleRoot?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<List<RoleRoot>> GetAllRolesAsync(CancellationToken cancellationToken = default)
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

    public async Task AddRoleAsync(RoleRoot role, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(role, cancellationToken);
            await _dbContext.Roles.AddAsync(role, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding role");
            throw;
        }
    }

    public async Task UpdateRoleAsync(RoleRoot role, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(role, cancellationToken);
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (ValidationException)
        {
            throw;
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
