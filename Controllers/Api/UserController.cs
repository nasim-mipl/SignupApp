using Microsoft.AspNetCore.Mvc;
using SignupApp.Data;
using SignupApp.Models;

namespace SignupApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API Working");
    }

    [HttpPost]
    public IActionResult Register([FromBody] SignupModel model)
    {
        _context.Users.Add(model);
        _context.SaveChanges();

        return Ok(new
        {
            message = "User Saved Successfully"
        });
    }
}