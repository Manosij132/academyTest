using Academy.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace Academy.API.Middlewares
{
    public class CustomJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSetting _appSetting;

        public CustomJwtMiddleware(RequestDelegate next, IOptions<AppSetting> appSetting)
        {
            _next = next;
            _appSetting = appSetting.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Endpoint endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAuthorizeData>() == null ||
                           endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
            if (allowAnonymous)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await IsValidAsync(context, token);
            }
            await _next(context);
        }

        private async Task IsValidAsync(HttpContext context, string token)
        {
            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(_appSetting.JWTSetting.Key));

            var tokenHandler = new JwtSecurityTokenHandler();

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _appSetting.JWTSetting.Issuer,
                ValidAudience = _appSetting.JWTSetting.Audience,
                IssuerSigningKey = symmetricSecurityKey
            };

            var claimsPrincipal = await tokenHandler.ValidateTokenAsync(token, validationParameters);

            if (claimsPrincipal.ClaimsIdentity?.IsAuthenticated == true)
            {
                var globerEmailClaim = claimsPrincipal.ClaimsIdentity?.Claims.FirstOrDefault(c => c.Type == "GloberEmail");// .Value.EndsWith("globant.com");

                if (globerEmailClaim == null || string.IsNullOrWhiteSpace(globerEmailClaim.Value))
                {
                    throw new SecurityTokenValidationException("GloberEmail claim is missing or empty.");
                }
                if (!IsValidEmail(globerEmailClaim.Value))
                {
                    throw new SecurityTokenValidationException("GloberEmail format is invalid.");
                };
            }
            else
            {
                throw new SecurityTokenValidationException("Invalid Token");
            }
        }

        private bool IsValidEmail(string email)
        {
            return email.EndsWith("globant.com");
        }
    }
}
