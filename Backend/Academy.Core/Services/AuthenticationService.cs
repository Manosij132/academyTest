using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Shared.Extensions;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Arch.EntityFrameworkCore.UnitOfWork;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using Academy.Shared.Response;
using Academy.Shared.Exceptions;

namespace Academy.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppSetting _appSetting;
        private readonly IAcademyDbContext _academyDbContext;

        public AuthenticationService(IUnitOfWork unitOfWork, IOptions<AppSetting> appSetting, IAcademyDbContext academyDbContext)
        {
            _appSetting = appSetting.Value;
            _academyDbContext = academyDbContext;
        }

        public async Task<Result<string>> ValidateGoogleToken(HttpContext httpContext)
        {
            if (_appSetting.AuthenticateLocal)
                return _appSetting.LoggedInUserEmail;

            string response = string.Empty;

            if (httpContext.Request.Method != HttpMethods.Options)
            {
                if (!httpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                else
                {
                    try
                    {
                        bool valid = true;
                        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = [_appSetting.IssuerWebAuthority.Decrypt()],
                        };

                        var hasAuthValue = httpContext.Request.Headers.TryGetValue("Authorization", out StringValues authString);
                        string idToken = string.Empty;

                        if (hasAuthValue)
                        {
                            var splitArray = httpContext.Request.Headers["Authorization"].ToString().Split(" ");
                            if (splitArray.Length > 1)
                            {
                                idToken = splitArray[1];
                            }
                        }

                        if (!string.IsNullOrEmpty(idToken))
                        {
                            var token = new JwtSecurityToken(jwtEncodedString: idToken);
                            var email = token.Claims.First(c => c.Type == "email").Value;
                            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
                            if (!payload.Audience.Equals(_appSetting.IssuerWebAuthority.Decrypt()))
                                valid = false;
                            if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
                                valid = false;
                            if (!payload.Email.Equals(email))
                                valid = false;
                            if (payload.ExpirationTimeSeconds == null)
                                valid = false;
                            else
                            {
                                DateTime now = DateTime.Now.ToUniversalTime();
                                DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
                                if (now > expiration)
                                {
                                    valid = false;
                                }
                            }
                            if (!valid)
                            {
                                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            }
                            else
                            {
                                response = email;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        httpContext.Response.Headers.Append("Message", ex.Message);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                return Result.Failure<string>(DomainErrors.Common.NullOrEmptyValue("Email"));
            }


            return response;
        }
        public async Task<Result<AuthenticatedUser>> FetchAuthenticatedUser(string email)
        {
            Employee employee = await _academyDbContext.Employees.FirstOrDefaultAsync(x => x.GlobantEmailAddress.Equals(email) && x.IsActive.Equals(true));
            List<Role> roles = new();

            if (employee is not null)
            {
                AuthenticatedUser authenticatedUser = new()
                {
                    Id = employee.Id,
                    Name = employee.EmployeeName,
                    GloberEmail = employee.GlobantEmailAddress,
                    Seniority = employee.Seniority,
                    SeniorityId = employee.SeniorityId ?? 0,
                    Project = employee.Project,
                    CareerMentorEmail = employee.BetterMeLeaderEmail,
                    Client = employee.Client,
                    Community = employee.Community,
                    Ecosystem = employee.Position,
                    GexLeaders = employee.GexLeaders.ToList<string>(),
                };

                roles = [.. (from m in _academyDbContext.EmployeeRoleMaps.DefaultIfEmpty()
                            join r in _academyDbContext.RoleMasters
                            on m.RoleId equals r.RoleId
                            where m.EmployeeId == employee.Id
                            && r.IsActive == true
                            && m.IsActive == true
                            select new Role()
                            {
                                RoleId = r.RoleId,
                                RoleName = r.RoleName ?? Roles.User.ToString(),
                                RoleAssignment = m.RoleAssignment ?? string.Empty
                            })];

                // If no mapping is found in EmployeeRole entity
                // Then assign 'User' role to the user.
                if (roles.Count == 0)
                {
                    roles.Add(new() { RoleId = (byte)Roles.User, RoleAssignment = string.Empty, RoleName = Roles.User.ToString() });
                }
                authenticatedUser.Roles = roles;
                authenticatedUser.IsAuthenticated = true;
                return authenticatedUser;
            }
            else
            {
                return Result.Failure<AuthenticatedUser>(DomainErrors.Common.NotFound(email));
            }
        }
        public async Task<Result<string>> AuthenticateUser(string email)
        {
            var result = await FetchAuthenticatedUser(email);

            if (result.IsFailure)
                return Result.Failure<string>(result.Error);

            string token = GenerateJWToken(result.Value);

            if (string.IsNullOrWhiteSpace(token))
            {
                return Result.Failure<string>(DomainErrors.Common.NullOrEmptyValue("Token"));
            }

            return token;
        }
        private string GenerateJWToken(AuthenticatedUser user)
        {
            var claims = new[]
            {
                new Claim("claimjson", JsonConvert.SerializeObject(user)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(nameof(user.GloberEmail), user.GloberEmail),
            };

            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(_appSetting.JWTSetting.Key));
            SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwtSecurityToken = new(
                issuer: _appSetting.JWTSetting.Issuer,
                audience: _appSetting.JWTSetting.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_appSetting.JWTSetting.DurationInMinutes),
                signingCredentials: signingCredentials);

            string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }
    }
}