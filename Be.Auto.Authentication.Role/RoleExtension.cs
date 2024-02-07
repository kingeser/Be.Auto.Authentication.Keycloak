namespace Be.Auto.Authentication.Keycloak.Role;

internal static class RoleExtension
{
  
    internal static string Normalize(string role)
    {

        foreach (var routeOptionsIgnoredKeyword in new[] { "Controller", "Page", "Model" }.OrderByDescending(t => t.Length))

        {

            if (role.EndsWith(routeOptionsIgnoredKeyword, StringComparison.OrdinalIgnoreCase))
            {
                role = role.Remove(role.Length - routeOptionsIgnoredKeyword.Length, routeOptionsIgnoredKeyword.Length);

            }
        }
        return role;
    }
}