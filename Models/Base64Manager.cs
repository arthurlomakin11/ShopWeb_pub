using System;
using System.Text;

namespace ShopWeb.Models
{
    public static class Base64Manager
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
    }
}