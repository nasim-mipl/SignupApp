using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignupApp.Data;
using SignupApp.Models;

namespace SignupApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ApplicationDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API Working", database = _context.Database.CanConnect() });
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] SignupRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Mobile) && string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "Please enter either Mobile or Phone number." });
        }

        try
        {
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
    }
}
