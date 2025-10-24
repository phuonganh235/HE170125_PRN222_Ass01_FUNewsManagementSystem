using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ASS1.Services;

namespace ASS1.Attributes
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly int[] _allowedRoles;

        public AuthorizeRoleAttribute(params int[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<IAuthenticationService>();
            
            if (authService == null || !authService.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var user = authService.GetCurrentUserAsync().Result;
            if (user == null || !_allowedRoles.Contains(user.AccountRole))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    public class RequireAdminAttribute : AuthorizeRoleAttribute
    {
        public RequireAdminAttribute() : base(3) { }
    }

    public class RequireStaffAttribute : AuthorizeRoleAttribute
    {
        public RequireStaffAttribute() : base(1, 3) { } // Staff and Admin
    }

    public class RequireLecturerAttribute : AuthorizeRoleAttribute
    {
        public RequireLecturerAttribute() : base(2, 3) { } // Lecturer and Admin
    }
}
