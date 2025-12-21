using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Entities.UserAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.UserServices;

public class UserService
{
    readonly AppDbContext _dbContext;
    readonly ILogger<UserService>? _logger;
    
    
    public UserService(AppDbContext dbContext, ILogger<UserService>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserRootEntity?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Users.SingleOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting user by email {Email}", email);
            throw;
        }
    }

    public async Task AddUserAsync(UserRootEntity user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding user");
            throw;
        }
    }

    public async Task RemoveUserAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
            if (user == null) throw new KeyNotFoundException($"User with email {email} not found");
            
            _dbContext.Users.Remove(user);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing user");
            throw;
        }
    }
}