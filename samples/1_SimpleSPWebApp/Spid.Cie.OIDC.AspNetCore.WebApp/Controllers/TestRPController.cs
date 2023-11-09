using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.WebApp.Models;
using System.Diagnostics;

namespace Spid.Cie.OIDC.AspNetCore.WebApp.Controllers;

public class TestRPController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public TestRPController(ILogger<HomeController> logger)
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

    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/TestRP/loggedin" }, SpidCieConst.AuthenticationScheme);
    }

    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/TestRP/loggedout" }, SpidCieConst.AuthenticationScheme);
    }

    public IActionResult LoggedIn()
    {
        return View();
    }

    public IActionResult LoggedOut()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature =
        HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        };

        if (exceptionHandlerPathFeature?.Error?.InnerException != null)
        {
            model.Error = exceptionHandlerPathFeature.Error.InnerException.Data.Contains("Error")
                ? exceptionHandlerPathFeature.Error.InnerException.Data["Error"] as string
                : exceptionHandlerPathFeature.Error.InnerException.GetType().Name;

            model.ErrorDescription = exceptionHandlerPathFeature.Error.InnerException.Data.Contains("ErrorDescription")
                ? exceptionHandlerPathFeature.Error.InnerException.Data["ErrorDescription"] as string
                : exceptionHandlerPathFeature.Error.InnerException.Message;
        }
        else if (exceptionHandlerPathFeature?.Error != null)
        {
            model.Error = exceptionHandlerPathFeature.Error.Data.Contains("Error")
                ? exceptionHandlerPathFeature.Error.Data["Error"] as string
                : exceptionHandlerPathFeature.Error.GetType().Name;

            model.ErrorDescription = exceptionHandlerPathFeature.Error.Data.Contains("ErrorDescription")
                ? exceptionHandlerPathFeature.Error.Data["ErrorDescription"] as string
                : exceptionHandlerPathFeature.Error.Message;
        }

        return View(model);
    }
}
