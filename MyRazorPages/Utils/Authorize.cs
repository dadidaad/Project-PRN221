using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyRazorPages.Utils
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(params string[] claim) : base(typeof(AuthorizeFilter))
        {
            Arguments = new object[] { claim };
        }
    }

    public class AuthorizeFilter : IAuthorizationFilter
    {
        readonly string[] _claim;

        public AuthorizeFilter(params string[] claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var IsAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;
            var claimsIndentity = context.HttpContext.User.Identity as ClaimsIdentity;

            if (IsAuthenticated)
            {
                bool flagClaim = false;
                foreach (var item in _claim)
                {
                    if (context.HttpContext.User.HasClaim(ClaimTypes.Role, item))
                        flagClaim = true;
                }
                if (!flagClaim)
                {
                    if (IsAjaxRequest(context.HttpContext.Request))
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized; //Set HTTP 401   
                    else
                        context.Result = new RedirectResult("~/NoPermission");
                }
            }
            else
            {
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden; //Set HTTP 403 -   
                }
                else
                {
                    context.Result = new RedirectResult("~/Account/SignIn/");
                }
            }
            return;
        }
        public bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }
    }
}
