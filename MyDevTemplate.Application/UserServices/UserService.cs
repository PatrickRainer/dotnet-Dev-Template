using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDevTemplate.Application.Common.Interfaces;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;
using MyDevTemplate.Persistence;

namespace MyDevTemplate.Application.UserServices;

public class UserService : ICrudService<UserRoot, Guid>
{
    readonly AppDbContext _dbContext;
    readonly ILogger<UserService>? _logger;
    readonly IValidator<UserRoot> _validator;

    public UserService(AppDbContext dbContext, IValidator<UserRoot> validator, ILogger<UserService>? logger = null)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<UserRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting user by id {Id}", id);
            throw;
        }
    }

    public async Task<List<UserRoot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Users.ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error getting all users");
            throw;
        }
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

    public async Task<Guid> AddAsync(UserRoot user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(user, cancellationToken);
            var result = await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return result.Entity.Id;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error adding user");
            throw;
        }
    }

    public async Task UpdateAsync(UserRoot user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(user, cancellationToken);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error updating user {Id}", user.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null) throw new KeyNotFoundException($"User with id {id} not found");

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing user {Id}", id);
            throw;
        }
    }

    public async Task<int> RemoveUserByEmailAsync(string email, CancellationToken cancellationToken = default)
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

    public async Task<Guid> UpsertAfterLogin(string identityProviderId, string email)
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
                user.IdentityProviderId = identityProviderId;
                user.LastLoginAtUtc = DateTime.UtcNow;
                
                await _validator.ValidateAndThrowAsync(user);
                await _dbContext.SaveChangesAsync();

                return user.Id;
            }
            else
            {
                // Create a new user
                var newUser = new UserRoot(new EmailAddress(email), string.Empty, string.Empty, identityProviderId)
                    {LastLoginAtUtc = DateTime.UtcNow};
                var result = await AddAsync(newUser);

                return result;
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error upserting user from Entra with email {Email} and OID {Oid}", email, identityProviderId);
            throw;
        }
    }
}