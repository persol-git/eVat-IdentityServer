using Evat.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace Evat.IdentityServer.Helper
{
    public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            if (password.ToLowerInvariant().Contains("evat"))
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PersonaXVariant",
                    Description = "Variants of PersonaX cannot be used in a password."
                }));
            }

            if (password.ToLowerInvariant().Contains("persol"))
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PersolVariant",
                    Description = "Variants of Persol cannot be used in a password."
                }));
            }

            return await Task.FromResult(IdentityResult.Success);
        }
    }
}
