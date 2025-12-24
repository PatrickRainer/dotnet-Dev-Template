using System.ComponentModel.DataAnnotations;

namespace MyDevTemplate.Application.TenantServices.Dtos;

public record CreateTenantDto(
    [Required] string TenantName,
    [Required] string CompanyName,
    [Required] string AdminEmail,
    string? Street,
    string? City,
    string? State,
    string? Country,
    string? ZipCode);

public record UpdateTenantDto(
    [Required] string TenantName,
    [Required] string CompanyName,
    string? Street,
    string? City,
    string? State,
    string? Country,
    string? ZipCode);
