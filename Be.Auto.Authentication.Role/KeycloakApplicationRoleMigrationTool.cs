﻿using System.Data;
using System.Reflection;
using System.Text;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using FS.Keycloak.RestApiClient.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Be.Auto.Authentication.Keycloak.Role
{
    internal class KeycloakApplicationRoleMigrationTool
    {
        private readonly KeycloakOption _options;
        private readonly ApplicationPartManager _applicationPartManager;


        internal KeycloakApplicationRoleMigrationTool(KeycloakOption options, ApplicationPartManager applicationPartManager)
        {
            _options = options;
            _applicationPartManager = applicationPartManager;
        }
        internal void Migrate()
        {
            var controllerFeature = new ControllerFeature();

            _applicationPartManager.PopulateFeature(controllerFeature);

            var controllerTypes = controllerFeature.Controllers.Where(t => (t.GetCustomAttribute<AuthorizeAttribute>() != null || t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Any(m => m.GetCustomAttribute<AuthorizeAttribute>() != null)) && $"{t.FullName}".StartsWith(AppDomain.CurrentDomain.FriendlyName));

            var pageTypes = (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                             where FindBaseType(t) == typeof(PageModel) && t.GetCustomAttribute<AuthorizeAttribute>() != null
                                && $"{t.FullName}".StartsWith(AppDomain.CurrentDomain.FriendlyName)
                             select t);

            var roles = new List<Tuple<string?, string?>>();

            foreach (var pageType in pageTypes)
            {
                var pageRole = RoleExtension.Normalize($"{pageType.FullName}".Replace($"{AppDomain.CurrentDomain.FriendlyName}.", string.Empty));
                var typeName = NormalizeName(pageRole);
                roles.Add(new Tuple<string?, string?>(pageRole, typeName));
            }

            foreach (var type in controllerTypes)
            {
                var typeName =
                    RoleExtension.Normalize($"{type.FullName}".Replace($"{AppDomain.CurrentDomain.FriendlyName}.", string.Empty));

                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(t => (t.GetCustomAttribute<AuthorizeAttribute>() != null && type.GetCustomAttribute<AllowAnonymousAttribute>() == null) || (type.GetCustomAttribute<AuthorizeAttribute>() != null && t.GetCustomAttribute<AllowAnonymousAttribute>() == null));
                roles.AddRange(from methodInfo in methods  let method = methodInfo.Name select $"{typeName}.{method}" into controllerRole select new Tuple<string?, string?>(controllerRole, NormalizeName(typeName)));

            }

            roles = roles.Where(t => !string.IsNullOrEmpty(t.Item1)).ToList();

            if (!roles.Any()) return;

            foreach (var role in roles)
            {
                CreateRoleIfNotExistAsync(new RoleRepresentation(Guid.NewGuid().ToString(), role.Item1, role.Item2));

            }

        }



        private void CreateRoleIfNotExistAsync(RoleRepresentation role)
        {
            var credentials = new ClientCredentialsFlow
            {
                KeycloakUrl = _options.KeycloakUrl,
                Realm = _options.AdminRealm,
                ClientId = _options.AdminClientId,
                ClientSecret = _options.AdminClientSecret,
                Scope = _options.AdminScope,

            };

            using var httpClient = AuthenticationHttpClientFactory.Create(credentials);
            using var clientApi = ApiClientFactory.Create<ClientsApi>(httpClient);
            var clients = clientApi.GetClients(_options.AdminRealm);
            var client = clients.Find(t => t.ClientId == _options.ClientId);
            if (client != null)
            {
                using var roleApi = ApiClientFactory.Create<RoleContainerApi>(httpClient);

                var existRole = _TryFindRole(role, roleApi, client);

                if (existRole == null)
                {
                    roleApi.PostClientsRolesById(_options.ClientRealm, client.Id, role);

                }
            }
            else
            {
                throw new NullReferenceException($"{_options.ClientId} client not found!");
            }

        }


        private RoleRepresentation? _TryFindRole(RoleRepresentation role, RoleContainerApi roleApi, ClientRepresentation client)
        {
            try
            {
                var existRole = roleApi.GetClientsRolesByIdAndRoleName(_options.ClientRealm, client.Id, role.Name);
                return existRole;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static string NormalizeName(string input)
        {

            if (string.IsNullOrEmpty(input))
                return input;

            input = input.TrimStart().TrimEnd().Trim();


            input = input.Replace(".", "");

            if (input.EndsWith("Index", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Replace("Index", string.Empty);
            }

            var result = new StringBuilder(input.Length * 2);

            result.Append(input[0]);

            for (var i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && !char.IsWhiteSpace(input[i - 1]))
                {
                    result.Append(' ');
                    result.Append('»');
                    result.Append(' ');
                }

                result.Append(input[i]);
            }

            return result.ToString();
        }



        private static Type FindBaseType(Type type)
        {
            var baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object))
            {
                type = baseType;
                baseType = type.BaseType;
            }

            return type;
        }

    }
}
