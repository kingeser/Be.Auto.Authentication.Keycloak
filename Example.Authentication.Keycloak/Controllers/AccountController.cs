using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.Authentication.Keycloak.Controllers;

[AllowAnonymous]
[Route("/account")]
public class AccountController : ControllerBase
{
    [HttpGet("sign-out")]
    public async Task<IActionResult> SignOutAsync()
    {
      
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}