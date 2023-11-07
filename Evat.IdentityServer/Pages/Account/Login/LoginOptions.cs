namespace Evat.IdentityServer.Pages.Account.Login
{
    public class LoginOptions
    {
        public static bool AllowLocalLogin = true;
        public static bool AllowRememberLogin = true;
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);
        public static string InvalidCredentialsErrorMessage = "Invalid username or password";
        public static string LockOutErrorMessage = "Your account has been locked, kindly check you email to re-activate";
        public static string UserInactiveErrorMessage = "Account has been made inactive or not yet enabled. contact your company administrator";
        public static string InputRequiredErrorMessage = "All input fields are required";
        public static string CodeRequiredErrorMessage = "A code must be supplied for password reset.";
        public static string UsernameRequiredErrorMessage = "A username must be supplied for password reset.";
        public static string AccountAlreadyConfirmedErrorMessage = "Your account has already been confirmed. Contact GRA support at evat@gra.com if your account was not confirmed by you.";
        public static string RedirectRequiredErrorMessage = "A redirect url must be supplied for password reset.";
        public static string LockOutFailedErrorMessage = "Lockout is not enable for this user.";
    }
}