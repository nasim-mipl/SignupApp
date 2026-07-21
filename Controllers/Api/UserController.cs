using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignupApp.Data;
using SignupApp.Models;

namespace SignupApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext? _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger, ApplicationDbContext? context = null)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (_context is null)
        {
            return Ok(new
            {
                message = "API Working",
                database = false,
                hint = "Set ConnectionStrings__DefaultConnection in Render Environment"
            });
        }

        try
        {
            return Ok(new { message = "API Working", database = _context.Database.CanConnect() });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection check failed");
            return Ok(new { message = "API Working", database = false });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] SignupRequest request)
    {
        if (_context is null)
        {
            return StatusCode(503, new
            {
                message = "Database is not configured. Add ConnectionStrings__DefaultConnection on Render."
            });
        }

        NormalizeOptionalFields(request);

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !new EmailAddressAttribute().IsValid(request.Email))
        {
            return BadRequest(new { message = "Enter a valid email address." });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error => error.ErrorMessage))
                .ToList();

            return BadRequest(new { message = string.Join(" ", errors), errors });
        }

        if (string.IsNullOrWhiteSpace(request.Mobile) && string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "Please enter either Mobile or Phone number." });
        }

        try
        {
            if (!await _context.Database.CanConnectAsync())
            {
                return StatusCode(503, new
                {
                    message = "Cannot connect to SQL Server. Check your connection string on Render."
                });
            }

            var user = request.ToEntity();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User Saved Successfully", id = user.Id });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save signup data");
            return StatusCode(500, new { message = "Could not save user. Check database connection and try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during signup");
            return StatusCode(500, new { message = "Server error. Please try again later." });
        }
    }

    private static void NormalizeOptionalFields(SignupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            request.Email = null;

        if (string.IsNullOrWhiteSpace(request.Phone))
            request.Phone = null;

        if (string.IsNullOrWhiteSpace(request.City))
            request.City = null;

        if (string.IsNullOrWhiteSpace(request.Hobbies))
            request.Hobbies = null;
    }
}
