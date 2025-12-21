using System.ComponentModel.DataAnnotations;

namespace MyDevTemplate.Domain.Entities.Common;

public record EmailAddress
{
    [Required]
    public string Value { get; init; }
    
    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or whitespace", nameof(value));
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, 
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            throw new ArgumentException("Invalid email format", nameof(value));
        
        Value = value;
    }
}