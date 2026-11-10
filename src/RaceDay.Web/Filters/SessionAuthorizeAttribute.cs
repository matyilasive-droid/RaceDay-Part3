using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Filters;

/// <summary>Blocks anonymous users (redirect to login) and users of the wrong role (access denied).</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string? Role { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.Session.GetUser();
        if (user is null)
        {
            var req = context.HttpContext.Request;
            context.Result = new RedirectToActionResult("Login", "Account",
                new { returnUrl = req.Path.Value + req.QueryString.Value });
            return;
        }

        if (Role is not null && !user.Role.Equals(Role, StringComparison.OrdinalIgnoreCase))
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
}
