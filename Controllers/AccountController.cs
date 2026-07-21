using Microsoft.AspNetCore.Mvc;
using SignupApp.Models;

namespace SignupApp.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Signup(SignupModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Abhi database me save nahi karna.
        // Sirf check karenge ki data aa raha hai.

        ViewBag.Message = "Form Submitted Successfully";

        return View(model);
    }
}