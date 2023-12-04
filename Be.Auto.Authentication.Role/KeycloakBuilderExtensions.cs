using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Be.Auto.Authentication.Keycloak.Role;

public static class KeycloakBuilderExtensions
{
    public static IApplicationBuilder MigrateKeycloakRoles(this IApplicationBuilder builder, Action<KeycloakOption> opt)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (opt == null)
        {
            throw new ArgumentNullException(nameof(opt));
        }

        var options = new KeycloakOption()
        {
            ClientId = string.Empty,
            AdminRealm = string.Empty,
            ClientRealm = string.Empty,
            AdminClientId = string.Empty,
            AdminClientSecret = string.Empty,
            KeycloakUrl = string.Empty,
            AdminScope = string.Empty,
        
        };

        opt(options);

        if (string.IsNullOrEmpty(options.KeycloakUrl))
        {
            throw new ArgumentNullException(nameof(options.KeycloakUrl));
        }

        if (string.IsNullOrEmpty(options.AdminRealm))
        {
            throw new ArgumentNullException(nameof(options.AdminRealm));
        }

        if (string.IsNullOrEmpty(options.AdminClientId))
        {
            throw new ArgumentNullException(nameof(options.AdminClientId));
        }

        if (string.IsNullOrEmpty(options.AdminClientSecret))
        {
            throw new ArgumentNullException(nameof(options.AdminClientSecret));
        }
        if (string.IsNullOrEmpty(options.ClientId))
        {
            throw new ArgumentNullException(nameof(options.ClientId));
        }
        if (string.IsNullOrEmpty(options.ClientRealm))
        {
            throw new ArgumentNullException(nameof(options.ClientRealm));
        }

        var tool = new KeycloakApplicationRoleMigrationTool(options,builder.ApplicationServices.GetRequiredService<ApplicationPartManager>());

        tool.Migrate();

        return builder;
    }



}