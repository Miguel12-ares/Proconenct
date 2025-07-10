using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProConnect.Core.Entities;
using System;
using System.Linq;

namespace Proconenct.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UserType[] _roles;

        public AuthorizeRolesAttribute(params UserType[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            if (roleClaim == null || !_roles.Any(r => r.ToString() == roleClaim.Value))
            {
                context.Result = new ForbidResult();
            }
        }
    }
} 