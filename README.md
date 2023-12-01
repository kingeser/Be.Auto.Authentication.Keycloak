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
public class SecureController : Controller
{
    // Your secure actions here
}
