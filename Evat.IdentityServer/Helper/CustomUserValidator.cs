using Duende.IdentityServer.Services;
using Evat.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Evat.IdentityServer.Helper
{
    public class CustomUserValidator : IUserValidator<ApplicationUser>
    {
        private readonly IDbConnection _db;
        private readonly IIdentityServerInteractionService _interaction;

        public CustomUserValidator(IIdentityServerInteractionService interaction, IDbConnection db)
        {
            _interaction = interaction;
            _db = db;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            // check if the company has not been disabled
            var isCompanyActive = true; //query company

            if (isCompanyActive)
            {
                return await Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "CompanyInActive",
                    Description = "Your company has been diabled. Contact administrator for support."
                }));
            }

        }
    }
}
