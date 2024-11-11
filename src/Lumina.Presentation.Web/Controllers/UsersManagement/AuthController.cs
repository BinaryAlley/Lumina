#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Filters;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Models.UsersManagement;
using Lumina.Presentation.Web.Common.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.UsersManagement;

/// <summary>
/// Controller for authentication related operations.
/// </summary>
[Authorize]
[Route("/auth/")]
public class AuthController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly ICryptographyService _cryptographyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    public AuthController(IApiHttpClient apiHttpClient, ICryptographyService cryptographyService)
    {
        _apiHttpClient = apiHttpClient;
        _cryptographyService = cryptographyService;
    }

    /// <summary>
    /// Displays the view for registering a new account.
    /// </summary>
    [AllowAnonymous]
    [InitializationCheck]
    [HttpGet("register")]
    public IActionResult Register()
    {
        // check if this is the initial super admin setup
        string? isPendingSuperAdminSetup = HttpContext.Session.GetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
        ViewData["RegistrationType"] = isPendingSuperAdminSetup == "true" ? "Admin" : "User";
        return View(new RegisterRequestModel());
    }

    /// <summary>
    /// Displays the view for logging in.
    /// </summary>
    /// <param name="returnUrl">The url to return to, after login (if any).</param>
    [AllowAnonymous]
    [InitializationCheck]
    [HttpGet("login/{returnUrl?}")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        // if the user is already logged in, redirect them to the home page
        if (User?.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        // check if the application is initialized (does contain at least the super user account); if it's not, redirect to registration view 
        string? isPendingSuperAdminSetup = HttpContext.Session.GetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
        if (isPendingSuperAdminSetup == "true")
            // TODO: should be:
            // return View("~/Views/Auth/Register", new RegisterRequestDto());
            // but doesn't work because of an ASP.NET bug: https://github.com/dotnet/AspNetCore.Docs/issues/25157
            return RedirectToAction("Register", "Auth");
        return View(new LoginRequestModel());
    }

    /// <summary>
    /// Displays the view for recovering the password of an account.
    /// </summary>
    [AllowAnonymous]
    [InitializationCheck]
    [HttpGet("recover-password")]
    public IActionResult RecoverPassword()
    {
        // if the user is already logged in, redirect them to the home page
        if (User?.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View();
    }

    /// <summary>
    /// Displays the view for registering a new account.
    /// </summary>
    [InitializationCheck]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        return View();
    }

    /// <summary>
    /// Registers a new account.
    /// </summary>
    /// <param name="data">User credentials used for registration.</param>
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel data)
    {
        // call different endpoints based on the view hidden field - registration for normal users, or initial application admin account setup
        string endpoint = data.RegistrationType == "Admin" ? "initialization" : "auth/register";
        // attempt API registration
        RegisterResponseModel response = await _apiHttpClient.PostAsync<RegisterResponseModel, RegisterRequestModel>(endpoint, data);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Logs in an new account.
    /// </summary>
    /// <param name="data">User credentials used for login.</param>
    /// <param name="returnUrl">The url to return to, after login (if any).</param>
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel data, string? returnUrl = null)
    {
        try
        {
            // attempt API registration
            LoginResponseModel response = await _apiHttpClient.PostAsync<LoginResponseModel, LoginRequestModel>("auth/login", data);
            // store the received token in a secure cookie            
            Response.Cookies.Delete("Token");
            Response.Cookies.Append("Token", _cryptographyService.Encrypt(response.Token!), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMonths(1),
                Path = "/",
                HttpOnly = true,
                Secure = true
            });
            // tell asp.net we are logged in
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Name, response.Username!),
                new Claim(ClaimTypes.NameIdentifier, response.Id.ToString()!),
                new Claim("Token", response.Token!),
            ];
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            // return success status and redirect URL
            return Json(new { success = true, data = string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl) ? Url.Content("~/") : Url.Content(returnUrl) });
        }
        catch (ApiException ex)
        {
            // in this particular case, we DO NOT want to let the ApiExceptionFilter take care of this exception, because when the user has 2FA enabled, the first auth step involves sending
            // only the username and password, in which case the server API will respond with a validation error, that TOTP code is not valid (empty); it is then that we catch this error here
            // and display the form that asks the user for the TOTP code
            if (ex.ProblemDetails is not null && ex.ProblemDetails.Extensions?.Count > 0 && ex.ProblemDetails.Extensions.TryGetValue("errors", out JsonElement errorsObj))
                if (errorsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    foreach (JsonProperty error in jsonElement.EnumerateObject())
                        if (error.Value.ValueKind == JsonValueKind.Array)
                            foreach (JsonElement errorValue in error.Value.EnumerateArray())
                                if (errorValue.ValueKind == JsonValueKind.String && errorValue.GetString() == "InvalidTotpCode" && string.IsNullOrEmpty(data.TotpCode))
                                    return Json(new { success = true, data = new { isTotpRequired = true } }); // only get here when user did not enter a TOTP code at all, otherwise rethrow the validation error
            throw;
        }
    }
}
