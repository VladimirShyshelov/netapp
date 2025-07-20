using System.Security.Principal;

namespace CysterApp.Services;

public class UserPrivilegeService
{
    private readonly WindowsPrincipal _principal =
        new(WindowsIdentity.GetCurrent());

    public bool IsAdmin()
    {
        return _principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public IReadOnlyCollection<string> GetRoles()
    {
        var roles = new List<string>();
        if (IsAdmin()) roles.Add("Local Administrator");
        if (_principal.IsInRole("Domain Admins")) roles.Add("Domain Administrator");
        return roles.Count == 0 ? new[] { "Standard User" } : roles;
    }

    public string GetRolesText()
    {
        return string.Join(", ", GetRoles());
    }
}