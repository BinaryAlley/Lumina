#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Filters;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Models.UsersManagement;
using Lumina.Presentation.Web.Common.Security;
using Lumina.Presentation.Web.Common.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.UsersManagement;

/// <summary>
/// Controller for authentication related operations.
/// </summary>
[Authorize]
[Route("{culture}/auth")]
public class AuthController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly ICryptographyService _cryptographyService;
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public AuthController(IApiHttpClient apiHttpClient, ICryptographyService cryptographyService, IUrlService urlService)
    {
        _apiHttpClient = apiHttpClient;
        _cryptographyService = cryptographyService;
        _urlService = urlService;
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
        return View();
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
            return Redirect(_urlService.GetAbsoluteUrl("Index", "Home")!);
        // check if the application is initialized (does contain at least the super user account); if it's not, redirect to registration view 
        string? isPendingSuperAdminSetup = HttpContext.Session.GetString(HttpContextItemKeys.PENDING_SUPER_ADMIN_SETUP);
        if (isPendingSuperAdminSetup == "true")
            // TODO: should be:
            // return View("~/Views/Auth/Register", new RegisterRequestDto());
            // but doesn't work because of an ASP.NET bug: https://github.com/dotnet/AspNetCore.Docs/issues/25157
            return Redirect(_urlService.GetAbsoluteUrl("Register", "Auth")!);
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
            return Redirect(_urlService.GetAbsoluteUrl("Index", "Home")!);
        return View();
    }

    /// <summary>
    /// Displays the view for changing the password of an account.
    /// </summary>
    [HttpGet("change-password")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    /// <summary>
    /// Displays the view for registering a new account.
    /// </summary>
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        return View();
    }

    /// <summary>
    /// Registers a new account.
    /// </summary>
    /// <param name="data">User credentials used for registration.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel data, CancellationToken cancellationToken)
    {
        // call different endpoints based on the view hidden field - registration for normal users, or initial application admin account setup
        string endpoint = data.RegistrationType == "Admin" ? "initialization" : "auth/register";
        // attempt API registration
        RegisterResponseModel response = await _apiHttpClient.PostAsync<RegisterResponseModel, RegisterRequestModel>(endpoint, data, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Logs in an account.
    /// </summary>
    /// <param name="data">User credentials used for login.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel data, CancellationToken cancellationToken)
    {
        try
        {
            // attempt API registration
            LoginResponseModel response = await _apiHttpClient.PostAsync<LoginResponseModel, LoginRequestModel>("auth/login", data, cancellationToken).ConfigureAwait(false);
            // store the received token in a secure cookie            
            Response.Cookies.Delete("Token");
            Response.Cookies.Append("Token", _cryptographyService.Encrypt(response.Token!), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMonths(1),
                Path = "/",
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict
            });
            // tell asp.net we are logged in
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Name, response.Username!),
                new Claim(ClaimTypes.NameIdentifier, response.Id.ToString()!),
                new Claim("Token", response.Token!),
            ];
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)).ConfigureAwait(false);
            // get current culture from route data or use default if not set
            string currentCulture = HttpContext.Request.RouteValues["culture"]?.ToString() ?? "en-US";
            // handle ReturnUrl and ensure it includes the correct culture
            string redirectUrl;
            if (!string.IsNullOrEmpty(data.ReturnUrl) && Url.IsLocalUrl(data.ReturnUrl)) // ensure that return URL contains the correct culture
                redirectUrl = data.ReturnUrl.StartsWith($"/{currentCulture}/") ? data.ReturnUrl : $"/{currentCulture}{data.ReturnUrl}";
            else
            {
                // if no ReturnUrl is provided or it's invalid, use a default URL with respect to base paths
                redirectUrl = Url.Content("~/");

                // handle reverse proxy scenarios by respecting any base path or forwarded headers
                if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues pathBase))
                    redirectUrl = pathBase + redirectUrl;

                // ensure that default redirect contains the correct culture
                if (!redirectUrl.StartsWith($"/{currentCulture}/"))
                    redirectUrl = $"/{currentCulture}{redirectUrl}";
            }

            // return success status and redirect URL
            return Json(new { success = true, data = redirectUrl });
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

    /// <summary>
    /// Logs out an account and redirects to the login page.
    /// </summary>
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        // if the user is not logged in, redirect them to the home page
        if (User?.Identity?.IsAuthenticated == false)
            return Redirect(_urlService.GetAbsoluteUrl("Index", "Home")!);
        Response.Cookies.Delete("Token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return Redirect(_urlService.GetAbsoluteUrl("Login", "Auth")!);
    }

    /// <summary>
    /// Recovers the password of an account.
    /// </summary>
    /// <param name="data">User credentials used for password recovery.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("recover-password")]
    public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordRequestModel data, CancellationToken cancellationToken)
    {
        RecoverPasswordResponseModel response = await _apiHttpClient.PostAsync<RecoverPasswordResponseModel, RecoverPasswordRequestModel>("auth/recover-password", data, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Changes the password of an account.
    /// </summary>
    /// <param name="data">User credentials used for password change.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [ValidateAntiForgeryToken]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel data, CancellationToken cancellationToken)
    {
        data = data with { Username = User?.Identity?.Name }; // assign the currently logged in user as the user for which to change the password
        ChangePasswordResponseModel response = await _apiHttpClient.PostAsync<ChangePasswordResponseModel, ChangePasswordRequestModel>("auth/change-password", data, cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }
}
