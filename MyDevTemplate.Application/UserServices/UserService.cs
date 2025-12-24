using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Domain.Entities.Common;
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

    public async Task<UserRoot?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailAddress = new EmailAddress(email);
            return await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == emailAddress, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting user by email {Email}", email);
            throw;
        }
    }

    public async Task<Guid> AddUserAsync(UserRoot user, CancellationToken cancellationToken = default)
    {
        try
        {
           var result= await _dbContext.Users.AddAsync(user, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity.Id;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding user");
            throw;
        }
    }

    public async Task<int> RemoveUserAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailAddress = new EmailAddress(email);
            var user = await _dbContext.Users
                .AsTracking()
                .SingleOrDefaultAsync(u => u.Email == emailAddress, cancellationToken);
            if (user == null) throw new KeyNotFoundException($"User with email {email} not found");

            _dbContext.Users.Remove(user);

            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing user");
            throw;
        }
    }

    public async Task<Guid> UpsertUserFromEntraAsync(string oid, string email)
    {
        try
        {
            // Try to find the user by email
            var emailAddress = new EmailAddress(email);
            var user = await _dbContext.Users
                .AsTracking()
                .SingleOrDefaultAsync(u => u.Email == emailAddress);

            if (user != null)
            {
                // Update OID if it's missing or different
                if (!string.IsNullOrWhiteSpace(oid) && user.IdentityProviderId != oid)
                {
                    user.IdentityProviderId = oid;
                    await _dbContext.SaveChangesAsync();
                    _logger?.LogInformation("Updated user {Email} with OID {Oid}", email, oid);
                }

                return user.Id;
            }
            else
            {
                // Create a new user
                var newUser = new UserRoot(new EmailAddress(email), string.Empty, string.Empty, oid);
                var result = await AddUserAsync(newUser);
                _logger?.LogInformation("Created new user {Email} with OID {Oid}", email, oid);

                return result;
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error upserting user from Entra with email {Email} and OID {Oid}", email, oid);
            throw;
        }
    }
}