using System.ComponentModel.DataAnnotations;

namespace MyDevTemplate.Application.RegistrationServices.Dtos;

public record RegistrationDto(
    [Required] string CompanyName,
    [Required] string Street,
    [Required] string City,
    [Required] string ZipCode,
    [Required] string Country,
    Guid? SubscriptionId);
