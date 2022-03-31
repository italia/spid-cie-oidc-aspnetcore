using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature =
        HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        string errorMessage = string.Empty;

        if (exceptionHandlerPathFeature?.Error != null)
        {
            var messages = FromHierarchy(exceptionHandlerPathFeature?.Error, ex => ex.InnerException)
                .Select(ex => ex.Message)
                .ToList();
            errorMessage = String.Join(" ", messages);
        }

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            Message = errorMessage
        });
    }

    private IEnumerable<TSource> FromHierarchy<TSource>(TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
    {
        for (var current = source; canContinue(current); current = nextItem(current))
        {
            yield return current;
        }
    }

    private IEnumerable<TSource> FromHierarchy<TSource>(TSource source,
        Func<TSource, TSource> nextItem)
        where TSource : class
    {
        return FromHierarchy(source, nextItem, s => s != null);
    }
}
