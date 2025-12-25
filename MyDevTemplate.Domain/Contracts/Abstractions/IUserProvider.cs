namespace MyDevTemplate.Domain.Contracts.Abstractions;

public interface IUserProvider
{
    string? GetIdentityProviderId();
}
