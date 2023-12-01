# ASP.NET Core Keycloak Integration

## Overview

This project provides seamless integration between ASP.NET Core Razor Pages and Controllers with Keycloak authentication, specifically focusing on role-based authorization. The `KeycloakAsyncAuthorizationFilter` allows for automatic discovery of Razor Pages and Controllers, automatically adding them as roles in the Keycloak server. With this filter, you can use the `[Authorize]` attribute without explicitly specifying roles, enabling automatic role-based validation.

## Installation

To use this integration, follow these steps:

1. Add the `KeycloakAsyncAuthorizationFilter` to the service collection in the `Startup.cs` file:

    ```csharp
    builder.Services.AddRazorPages().AddMvcOptions(t =>
    {
        t.Filters.Add<KeycloakAsyncAuthorizationFilter>();
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "Test";
        options.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://localhost:8080/realms/master";
        options.ClientId = "test-client";
        options.ClientSecret = "85dFFVJ9zIghgWJQFaKKfxEIStt22vYG";
        options.ResponseType = "code";
        options.SaveTokens = false;
        options.Scope.Add("openid");
        options.UsePkce = true;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role,
        };
        options.RequireHttpsMetadata = false;
    });
    ```

2. If you are in a development environment, use the `MigrateKeycloakRoles` method to automatically migrate Razor Pages and Controllers to Keycloak roles. Specify your Keycloak server details in the provided options:

    ```csharp
    if (app.Environment.IsDevelopment())
    {
        app.MigrateKeycloakRoles(option =>
        {
            option.KeycloakUrl = "http://localhost:8080";
            option.AdminClientId = "admin-cli";
            option.AdminRealm = "master";
            option.AdminClientSecret = "pMlTGidScso7dQ27AFY4DXczO8Baegtr";
            option.ClientId = "test-client";
            option.ClientRealm = "master";
            option.AdminScope = null;
        });
    }


    
    ```



## Usage

Once the integration is set up, you can use the `[Authorize]` attribute without specifying roles explicitly. The roles will be automatically mapped based on your Razor Pages and Controllers.

```csharp

[Authorize]
[Route("/user")]
public class UserController : ControllerBase
{

    [HttpGet("/get-user")]
    public string GetUser()
    {

        return "Success";
    }
    [HttpGet("/create-user")]
    public string Createuser()
    {

        return "Success";
    }
    [HttpGet("/update-user")]
    public string UpdateUser()
    {

        return "Success";
    }
    [HttpGet("/delete-user")]
    public string DeleteUser()
    {

        return "Success";
    }
}
  ```
