namespace MyDevTemplate.Application.ApiKeyServices.Dtos;

public record AddApiKeyDto(
    string Key,
    string Label,
    DateTime? ExpiresAtUtc,
    string? TenantId = null);
