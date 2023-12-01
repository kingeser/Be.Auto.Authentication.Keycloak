using System.Reflection;
using System.Text;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using FS.Keycloak.RestApiClient.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Be.Auto.Authentication.Keycloak.Role
{
    internal class KeycloakApplicationRoleMigrationTool
    {
        private readonly KeycloakOption _options;


        internal KeycloakApplicationRoleMigrationTool(KeycloakOption options)
        {
            _options = options;
        }
        internal void Migrate()
        {
         
            var types =
                (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                 where t.GetCustomAttribute<AuthorizeAttribute>() != null
                 let interfaces = t.GetInterfaces().Where(x => !$"{x.FullName}".StartsWith(AppDomain.CurrentDomain.FriendlyName))
                 select t).ToList();

            foreach (var type in types)
            {
                var roles = new List<Tuple<string?, string?>>();
                var baseType = FindBaseType(type);
                var typeName = AddSpaceBetweenCamelCase(type.Name);
                if (baseType == typeof(PageModel))
                {
                    var pageRole = $"Pages.{type.Name?.Replace("Model", string.Empty)}";

                    roles.Add(new Tuple<string?, string?>(pageRole, typeName));

                }
                else if (baseType == typeof(ControllerBase))
                {
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                    roles.AddRange(from methodInfo in methods let name = type.Name.Replace("Controller", "") let method = methodInfo.Name select $"Controllers.{name}.{method}" into controllerRole select new Tuple<string?, string?>(controllerRole, typeName));
                }

                roles = roles.Where(t => !string.IsNullOrEmpty(t.Item1)).ToList();

                if (!roles.Any()) continue;

                foreach (var role in roles)
                {

                    CreateRoleIfNotExistAsync(new RoleRepresentation(Guid.NewGuid().ToString(), role.Item1, role.Item2));

                }

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

        private static string AddSpaceBetweenCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder(input.Length * 2);
            result.Append(input[0]);

            for (var i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && !char.IsWhiteSpace(input[i - 1]))
                    result.Append(' ');

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
