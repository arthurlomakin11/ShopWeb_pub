using ShopWeb.Shared;

using System;
using System.Text;

namespace ShopWeb.Models
{
    public class GeneralPasswordManager
    {
        public static string Encode(string Text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(Text);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Decode(string EncodedString)
        {
            var base64EncodedBytes = Convert.FromBase64String(EncodedString);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetPassword()
        {
            return Decode(SettingsManager.GetValue("GeneralAccountPassword"));
        }
    }
}
