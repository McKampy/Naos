﻿namespace Naos.Authentication.Application.Web
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;

    public class BasicAuthenticationHandler : BaseAuthenticationHandler<AuthenticationHandlerOptions, AuthenticationHandlerEvents>
    {
        private readonly IAuthenticationService service;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationHandlerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAuthenticationService service)
            : base(options, logger, encoder, clock)
        {
            EnsureArg.IsNotNull(service, nameof(service));

            this.service = service;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                this.Logger.LogInformation("{LogKey:l} basic handle", LogKeys.Authentication);

                if (this.Request.Host.Host.SafeEquals("localhost") && this.Options.IgnoreLocal)
                {
                    // ignore for localhost
                    var identity = new ClaimsIdentity(
                        this.Options.Claims.Safe().Select(c => new Claim(c.Key, c.Value))
                        .Insert(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationKeys.ApiKeyScheme))
                        .Insert(new Claim(ClaimTypes.Name, ClaimsIdentity.DefaultIssuer))
                        .DistinctBy(c => c.Type),
                        this.Scheme.Name);
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
                    this.Logger.LogInformation($"{{LogKey:l}} basic authenticated (name={identity.Name})", LogKeys.Authentication);

                    return AuthenticateResult.Success(ticket);
                }

                string value;
                if (this.Request.Query.TryGetValue("basic", out var queryValue))
                {
                    // also allow the auth header to be sent in the querystring (for easy dashboard usage)
                    value = queryValue.ToString();
                }
                else
                {
                    if (!this.Request.Headers.ContainsKey(AuthenticationKeys.AuthorizationHeaderName))
                    {
                        return AuthenticateResult.NoResult(); //Authorization header not in request
                    }

                    if (!AuthenticationHeaderValue.TryParse(this.Request.Headers[AuthenticationKeys.AuthorizationHeaderName], out var headerValue))
                    {
                        return AuthenticateResult.NoResult(); //Invalid Authorization header
                    }
                    else
                    {
                        value = headerValue.Parameter;
                    }

                    if (!AuthenticationKeys.BasicScheme.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
                    {
                        return AuthenticateResult.NoResult(); //Not a basic authentication header
                    }
                }

                if (value.IsNullOrEmpty())
                {
                    return AuthenticateResult.NoResult(); //No apikey authentication value in header or query
                }

                //var context = new ApiKeyValidationContext(this.Context, this.Scheme, this.Options) { ApiKey = headerValue.Parameter };
                //await this.Events.OnValidation(context);
                //if (context.Result != null)
                //{
                //    return context.Result;
                //}

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
                var (authenticated, claims) = this.service.Validate(value);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
                if (authenticated)
                {
                    var identity = new ClaimsIdentity(
                        this.Options.Claims.Safe().Select(c => new Claim(c.Key, c.Value)).Concat(claims.Safe())
                            .Insert(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationKeys.ApiKeyScheme)).DistinctBy(c => c.Type),
                        this.Scheme.Name);
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
                    this.Logger.LogInformation($"{{LogKey:l}} basic authenticated (name={identity.Name})", LogKeys.Authentication);

                    return AuthenticateResult.Success(ticket);
                }

                this.Logger.LogWarning("{LogKey:l} basic not authenticated", LogKeys.Authentication);
                return AuthenticateResult.NoResult();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"{{LogKey:l}} {ex.Message}", LogKeys.Authentication);
                var context = new ErrorContext(this.Context, this.Scheme, this.Options) { Exception = ex };
                if (this.Events != null)
                {
                    await this.Events.Error(context).AnyContext();
                    if (context.Result != null)
                    {
                        return context.Result;
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>
        /// A Task.
        /// </returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authResult = await this.HandleAuthenticateOnceSafeAsync().AnyContext();

            if (authResult.Succeeded)
            {
                return;
            }

            var eventContext = new ChallengeContext(this.Context, this.Scheme, this.Options, properties)
            {
                Exception = authResult.Failure
            };

            if (this.Events != null)
            {
                await this.Events.Challenge(eventContext).AnyContext();

                if (eventContext.Handled)
                {
                    return;
                }
            }

            this.Response.Headers["WWW-Authenticate"] = $"{AuthenticationKeys.BasicScheme} realm=\"{this.Options.Realm}\", charset=\"UTF-8\"";
            this.Response.StatusCode = (int)eventContext.StatusCode;
        }
    }
}
