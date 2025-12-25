namespace MyDevTemplate.Application.SubscriptionServices.Dtos;

public record SubscriptionDto(Guid Id, string Name, string Description, List<string> Features);
public record CreateSubscriptionDto(string Name, string Description, List<string> Features);
public record UpdateSubscriptionDto(string Name, string Description, List<string> Features);
