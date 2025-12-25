using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.UserAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.UserGroupServices;

public class UserGroupService : ICrudService<UserGroup, Guid>
{
    readonly AppDbContext _dbContext;
    readonly ILogger<UserGroupService>? _logger;
    readonly IValidator<UserGroup> _validator;
    readonly IFeatureService _featureService;

    public UserGroupService(
        AppDbContext dbContext, 
        IValidator<UserGroup> validator, 
        IFeatureService featureService,
        ILogger<UserGroupService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _featureService = featureService;
        _logger = logger;
    }

    public async Task<UserGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserGroups
            .Include(g => g.Users)
            .SingleOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<List<UserGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserGroups
            .Include(g => g.Users)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> AddAsync(UserGroup entity, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(entity, cancellationToken);
        var result = await _dbContext.UserGroups.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result.Entity.Id;
    }

    public async Task UpdateAsync(UserGroup entity, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(entity, cancellationToken);
        if (_dbContext.Entry(entity).State == EntityState.Detached)
        {
            _dbContext.UserGroups.Update(entity);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.UserGroups.SingleOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (group == null) throw new KeyNotFoundException($"UserGroup with id {id} not found");

        _dbContext.UserGroups.Remove(group);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddFeatureToGroupAsync(Guid groupId, string feature, CancellationToken cancellationToken = default)
    {
        var group = await GetByIdAsync(groupId, cancellationToken);
        if (group == null) throw new KeyNotFoundException($"UserGroup with id {groupId} not found");

        if (!await _featureService.IsFeatureSubscribedAsync(feature))
        {
            throw new InvalidOperationException($"Tenant is not subscribed to feature {feature}");
        }

        group.AddFeature(feature);

        await UpdateAsync(group, cancellationToken);
    }

    public async Task RemoveFeatureFromGroupAsync(Guid groupId, string feature, CancellationToken cancellationToken = default)
    {
        var group = await GetByIdAsync(groupId, cancellationToken);
        if (group == null) throw new KeyNotFoundException($"UserGroup with id {groupId} not found");

        group.RemoveFeature(feature);

        await UpdateAsync(group, cancellationToken);
    }

    public async Task AddUserToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var group = await GetByIdAsync(groupId, cancellationToken);
        if (group == null) throw new KeyNotFoundException($"UserGroup with id {groupId} not found");

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User with id {userId} not found");

        group.AddUser(user);
        await UpdateAsync(group, cancellationToken);
    }

    public async Task RemoveUserFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var group = await GetByIdAsync(groupId, cancellationToken);
        if (group == null) throw new KeyNotFoundException($"UserGroup with id {groupId} not found");

        var user = group.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            group.RemoveUser(user);
            await UpdateAsync(group, cancellationToken);
        }
    }
}
