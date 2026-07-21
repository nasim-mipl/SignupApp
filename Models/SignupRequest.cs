using System.ComponentModel.DataAnnotations;

namespace SignupApp.Models;

public class SignupRequest
{
    [Required]
    [MaxLength(25)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? Mobile { get; set; }

    public string? Phone { get; set; }

    [Required]
    public string State { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Hobbies { get; set; }

    public SignupModel ToEntity()
    {
        var contact = !string.IsNullOrWhiteSpace(Mobile)
            ? Mobile.Trim()
            : Phone?.Trim() ?? string.Empty;

        return new SignupModel
        {
            Name = Name.Trim(),
            Gender = Gender,
            DateOfBirth = DateOfBirth,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            Mobile = contact,
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
            State = State,
            City = string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
            Hobbies = string.IsNullOrWhiteSpace(Hobbies) ? null : Hobbies
        };
    }
}
