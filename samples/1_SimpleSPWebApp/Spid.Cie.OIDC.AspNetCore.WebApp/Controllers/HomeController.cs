using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.WebApp.Models;
using System.Diagnostics;

namespace Spid.Cie.OIDC.AspNetCore.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/home/loggedin" }, SpidCieConst.AuthenticationScheme);
    }

    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/home/loggedout" }, SpidCieConst.AuthenticationScheme);
    }

    public IActionResult LoggedIn()
    {
        return View();
    }

    public IActionResult LoggedOut()
    {
        return View();
    }
}
