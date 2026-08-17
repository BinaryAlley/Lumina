#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Security;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// API endpoint for the <c>/{culture}/auth/api-login</c> route.
/// </summary>
public class LoginEndpoint : BaseEndpoint<LoginRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly ICryptographyService _cryptographyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    public LoginEndpoint(IApiHttpClient apiHttpClient, ICryptographyService cryptographyService)
    {
        _apiHttpClient = apiHttpClient;
        _cryptographyService = cryptographyService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Authentication.LOGIN);
        DontAutoTag();
        Options(options => options.WithTags("Authentication"));
        AllowAnonymous();
        EnableAntiforgery();
    }

    /// <summary>
    /// Authenticates an account and signs the user in.
    /// </summary>
    /// <param name="request">The request containing the account credentials.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // attempt API login
            LoginResponse response = await _apiHttpClient.PostAsync<LoginResponse, LoginRequest>(ApiRoutes.Authentication.LOGIN_ACCOUNT, request, cancellationToken).ConfigureAwait(false);
            // store the received token in a secure cookie
            HttpContext.Response.Cookies.Delete("Token");
            HttpContext.Response.Cookies.Append("Token", _cryptographyService.Encrypt(response.Token!), new CookieOptions
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
            string currentCulture = Culture;
            // handle ReturnUrl and ensure it includes the correct culture
            string redirectUrl;
            if (!string.IsNullOrEmpty(request.ReturnUrl) && IsLocalUrl(request.ReturnUrl)) // ensure that return URL contains the correct culture
                redirectUrl = request.ReturnUrl.StartsWith($"/{currentCulture}/") ? request.ReturnUrl : $"/{currentCulture}{request.ReturnUrl}";
            else
            {
                // if no ReturnUrl is provided or it's invalid, use a default URL with respect to base paths
                redirectUrl = "/";

                // handle reverse proxy scenarios by respecting any base path or forwarded headers
                if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-Prefix", out Microsoft.Extensions.Primitives.StringValues pathBase))
                    redirectUrl = pathBase + redirectUrl;

                // ensure that default redirect contains the correct culture
                if (!redirectUrl.StartsWith($"/{currentCulture}/"))
                    redirectUrl = $"/{currentCulture}{redirectUrl}";
            }

            // return success status and redirect URL
            return JsonSuccess(redirectUrl);
        }
        catch (ApiException apiException)
        {
            // in this particular case, we DO NOT want to let the exception handling middleware take care of this exception, because when the user has 2FA enabled, the first auth step involves sending
            // only the username and password, in which case the server API will respond with a validation error, that TOTP code is not valid (empty); it is then that we catch this error here
            // and display the form that asks the user for the TOTP code
            if (apiException.ProblemDetails is not null && apiException.ProblemDetails.Extensions?.Count > 0 && apiException.ProblemDetails.Extensions.TryGetValue("errors", out JsonElement errorsObj))
                if (errorsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    foreach (JsonProperty error in jsonElement.EnumerateObject())
                        if (error.Value.ValueKind == JsonValueKind.Array)
                            foreach (JsonElement errorValue in error.Value.EnumerateArray())
                                if (errorValue.ValueKind == JsonValueKind.String && errorValue.GetString() == "InvalidTotpCode" && string.IsNullOrEmpty(request.TotpCode))
                                    return JsonSuccess(new { isTotpRequired = true }); // only get here when user did not enter a TOTP code at all, otherwise rethrow the validation error
            throw;
        }
    }

    /// <summary>
    /// Determines whether the specified URL is a local URL.
    /// </summary>
    /// <param name="url">The URL to inspect.</param>
    /// <returns><see langword="true"/> if the URL is local, <see langword="false"/> otherwise.</returns>
    private bool IsLocalUrl(string? url)
    {
        IUrlHelperFactory urlHelperFactory = HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        RouteData routeData = HttpContext.GetRouteData() ?? new RouteData();
        IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(HttpContext, routeData, new ActionDescriptor()));
        return urlHelper.IsLocalUrl(url);
    }
}
