﻿namespace Naos.Core.Authentication.App.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Policy;
    using Microsoft.AspNetCore.Http;
    using Naos.Foundation.Application;

    public class EasyAuthPolicyEvaluator : PolicyEvaluator
    {
        public EasyAuthPolicyEvaluator(IAuthorizationService authorization)
            : base(authorization)
        {
        }

        public override async Task<PolicyAuthorizationResult> AuthorizeAsync(
            AuthorizationPolicy policy,
            AuthenticateResult authenticationResult,
            HttpContext context,
            object resource)
        {
            var result = await base.AuthorizeAsync(policy, authenticationResult, context, resource);
            if(!result.Succeeded)
            {
                // If user is not authenticated, send them to the easyauth login
                if(!context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = 302;
                    context.Response.Redirect("/.auth/login/aad");
                    //return PolicyAuthorizationResult.Success(); // handled
                }
            }

            return result;
        }
    }
}
