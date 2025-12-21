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
        
        _logger?.LogInformation( "UserService created");
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
}