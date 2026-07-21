public class SignupModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Gender { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string Mobile { get; set; }

    public string? Phone { get; set; }

    public string State { get; set; }

    public string? City { get; set; }

    public string? Hobbies { get; set; }
}