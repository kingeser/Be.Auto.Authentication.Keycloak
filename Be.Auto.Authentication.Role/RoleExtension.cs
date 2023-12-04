namespace Be.Auto.Authentication.Keycloak.Role;

internal static class RoleExtension
{
    internal static string Normalize(string role)
    {

        foreach (var routeOptionsIgnoredKeyword in new[] { "Controller", "Page", "Model", "Async", "Service", "Manager" }.OrderByDescending(t => t.Length))

        {
            if (role.StartsWith(routeOptionsIgnoredKeyword, StringComparison.OrdinalIgnoreCase))
            {
                role = role.Remove(0, routeOptionsIgnoredKeyword.Length);
            }

            if (role.EndsWith(routeOptionsIgnoredKeyword, StringComparison.OrdinalIgnoreCase))
            {
                role = role.Remove(role.Length - routeOptionsIgnoredKeyword.Length, routeOptionsIgnoredKeyword.Length);

            }
        }
        return role;
    }
}