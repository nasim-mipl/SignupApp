using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SignupApp.Models;

public class SignupRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
    [MaxLength(25, ErrorMessage = "Name cannot exceed 25 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select gender.")]
    [RegularExpression("^(Male|Female)$", ErrorMessage = "Please select a valid gender.")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    public DateOnly? DateOfBirth { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(254)]
    public string? Email { get; set; }

    public string? Mobile { get; set; }
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select state.")]
    [RegularExpression("^(Delhi|UP)$", ErrorMessage = "Please select a valid state.")]
    public string State { get; set; } = string.Empty;

    public string? City { get; set; }

    [RegularExpression("^(Chess|Cricket|Football|Hockey)$", ErrorMessage = "Please select a valid hobby.")]
    public string? Hobbies { get; set; }

    public bool AgreeTerms { get; set; }
    public IFormFile? Photo { get; set; }

    public SignupModel ToEntity(string? photoPath)
    {
        var mobile = NormalizePhone(Mobile);
        var phone = NormalizePhone(Phone);
        return new SignupModel
        {
            Name = Name.Trim(),
            Gender = Gender,
            DateOfBirth = DateOfBirth!.Value,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            Mobile = !string.IsNullOrWhiteSpace(mobile) ? mobile : phone!,
            Phone = phone,
            State = State,
            City = string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
            Hobbies = string.IsNullOrWhiteSpace(Hobbies) ? null : Hobbies,
            PhotoPath = photoPath,
            AgreeTerms = AgreeTerms
        };
    }

    public static string? NormalizePhone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
