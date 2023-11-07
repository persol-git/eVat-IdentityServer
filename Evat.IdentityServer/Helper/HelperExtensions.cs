namespace Evat.IdentityServer.Helper
{
    public static class HelperExtensions
    {
        public static string ShortEmailFormatter(string data)
        {
            var newStr = data.Remove(2, 7);
            var strRes = newStr.Insert(2, "*******");
            var pos = strRes.IndexOf("@");
            if (pos >= 0)
            {
                var dt = strRes.Remove(pos, 3);
                var strfinal = dt.Insert(pos + 1, "@***");
                return strfinal;
            }

            return strRes;
        }

        public static bool PhoneNumberCheck(string str)
        {
            if (str == null)
            {
                return false;
            }
            else
            {
                foreach (var c in str)
                    if (c < '0' || c > '9')
                        return false;

                return true;
            }

        }

        public static string PhoneNumberFormatter(string phone)
        {
            var res = GetLast(phone, 2);

            var fmtPhone = phone.Remove(2, 4);

            var strRes = fmtPhone.Insert(2, "****");
            var resultR = strRes.Insert(2, res);

            return resultR;
        }

        public static string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }
}
