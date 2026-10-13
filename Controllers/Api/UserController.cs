using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignupApp.Data;
using SignupApp.Models;

namespace SignupApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly ApplicationDbContext? _context;
    private readonly ILogger<UserController> _logger;
    private readonly IWebHostEnvironment _environment;

    public UserController(ILogger<UserController> logger, IWebHostEnvironment environment, ApplicationDbContext? context = null)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (_context is null)
            return Ok(new { message = "API Working", database = false, hint = "Set ConnectionStrings__DefaultConnection or DATABASE_URL" });
        try { return Ok(new { message = "API Working", database = _context.Database.CanConnect() }); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection check failed");
            return Ok(new { message = "API Working", database = false });
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Register([FromForm] SignupRequest request, CancellationToken cancellationToken)
    {
        if (_context is null)
            return StatusCode(503, new { message = "Database is not configured. Set the database connection string." });

        NormalizeOptionalFields(request);

        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            ModelState.AddModelError(nameof(request.DateOfBirth), "Date of birth cannot be in the future.");

        if (string.IsNullOrWhiteSpace(request.Mobile) && string.IsNullOrWhiteSpace(request.Phone))
            ModelState.AddModelError(nameof(request.Mobile), "Please enter either Mobile or Phone number.");

        if (!ValidPhone(request.Mobile))
            ModelState.AddModelError(nameof(request.Mobile), "Mobile must contain exactly 10 digits.");
        if (!ValidPhone(request.Phone))
            ModelState.AddModelError(nameof(request.Phone), "Phone must contain exactly 10 digits.");

        if (!request.AgreeTerms)
            ModelState.AddModelError(nameof(request.AgreeTerms), "You must agree to the Terms & Conditions.");

        if (!ValidCityForState(request.State, request.City))
            ModelState.AddModelError(nameof(request.City), "Please select a city that belongs to the selected state.");

        if (request.Photo is not null)
        {
            var extension = Path.GetExtension(request.Photo.FileName);
            if (request.Photo.Length <= 0 || request.Photo.Length > MaxPhotoSize)
                ModelState.AddModelError(nameof(request.Photo), "Photo must be a non-empty file up to 5 MB.");
            if (!AllowedExtensions.Contains(extension) ||
                request.Photo.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
                ModelState.AddModelError(nameof(request.Photo), "Only JPG, JPEG, PNG, and WEBP photos are allowed.");
        }

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        string? savedPhotoPath = null;
        try
        {
            if (!await _context.Database.CanConnectAsync(cancellationToken))
                return StatusCode(503, new { message = "Cannot connect to PostgreSQL. Check the database settings." });

            if (request.Photo is not null)
            {
                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var uploadDirectory = Path.Combine(webRoot, "uploads", "users");
                Directory.CreateDirectory(uploadDirectory);
                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(request.Photo.FileName).ToLowerInvariant()}";
                var fullPath = Path.Combine(uploadDirectory, fileName);
                await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await request.Photo.CopyToAsync(stream, cancellationToken);
                savedPhotoPath = $"/uploads/users/{fileName}";
            }

            var user = request.ToEntity(savedPhotoPath);
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "User Saved Successfully", id = user.Id, photoPath = user.PhotoPath });
        }
        catch (DbUpdateException ex)
        {
            DeleteSavedPhoto(savedPhotoPath);
            _logger.LogError(ex, "Failed to save signup data");
            return StatusCode(500, new { message = "Could not save user. Check database connection and try again." });
        }
        catch (Exception ex)
        {
            DeleteSavedPhoto(savedPhotoPath);
            _logger.LogError(ex, "Unexpected error during signup");
            return StatusCode(500, new { message = "Server error. Please try again later." });
        }
    }

    private void DeleteSavedPhoto(string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath)) return;
        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(webRoot, photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not clean up uploaded photo after failed registration."); }
    }

    private static bool ValidPhone(string? value) =>
        string.IsNullOrWhiteSpace(value) || Regex.IsMatch(value.Trim(), @"^\d{10}$");

    private static bool ValidCityForState(string state, string? city) =>
        string.IsNullOrWhiteSpace(city) ||
        (state == "Delhi" && city == "New Delhi") ||
        (state == "UP" && (city == "Noida" || city == "Lucknow"));

    private static void NormalizeOptionalFields(SignupRequest request)
    {
        request.Name = request.Name?.Trim() ?? string.Empty;
        request.Gender = request.Gender?.Trim() ?? string.Empty;
        request.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        request.Mobile = SignupRequest.NormalizePhone(request.Mobile);
        request.Phone = SignupRequest.NormalizePhone(request.Phone);
        request.State = request.State?.Trim() ?? string.Empty;
        request.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        request.Hobbies = string.IsNullOrWhiteSpace(request.Hobbies) ? null : request.Hobbies.Trim();
    }
}
