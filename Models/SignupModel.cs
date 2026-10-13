namespace SignupApp.Models;

public class SignupModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string State { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Hobbies { get; set; }
    public string? PhotoPath { get; set; }
    public bool AgreeTerms { get; set; }
}
