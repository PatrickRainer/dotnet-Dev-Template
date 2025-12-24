namespace MyDevTemplate.Application.ApiKeyServices.Dtos;

public record UpdateApiKeyDto(
    string Label,
    bool IsActive,
    DateTime? ExpiresAtUtc);
