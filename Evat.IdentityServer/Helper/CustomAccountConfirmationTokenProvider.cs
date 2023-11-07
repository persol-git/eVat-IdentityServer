using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Evat.IdentityServer.Helper
{
    public class CustomAccountConfirmationTokenProvider<TUser>
                              : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomAccountConfirmationTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<AccountConfirmationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
                                           : base(dataProtectionProvider, options, logger)
        {

        }
    }

    public class AccountConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public AccountConfirmationTokenProviderOptions()
        {
            Name = "AccountDataProtectorTokenProvider";
            TokenLifespan = TimeSpan.FromDays(3);
        }
    }
}
