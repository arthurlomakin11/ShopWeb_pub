using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using ShopWeb.Shared;

using ShopWeb.Data;
using System;
using System.Linq;

namespace ShopWeb.Models
{
    public static class StatisticsManager
    {
        static readonly string Key = "uhUHUuGHDUIBuydsesidfesuafpaDSFgsfSEFgwe";
        static int AnonymousUserId;
        public static void WriteStatistics(HttpContext HttpContext)
        {
            if (HttpContext.Request.Cookies[Key] != null)
            {
                string AnonymousUserIdEncoded = HttpContext.Request.Cookies[Key];
                AnonymousUserId = int.Parse(Base64Manager.Decode(AnonymousUserIdEncoded));
            }
            else
            {
                AnonymousUserId = int.Parse(SettingsManager.GetValue("AnonymousUserId")) + 1;
                ShopWebContext SettingsContext = new ();
                SettingsContext.Settings.First(s => s.Key == "AnonymousUserId").Value = AnonymousUserId.ToString();
                SettingsContext.SaveChanges();

                CookieBuilder cookie = new ();
                CookieOptions option = cookie.Build(HttpContext);

                option.Expires = DateTime.Now.AddYears(5);

                HttpContext.Response.Cookies.Append(Key, Base64Manager.Encode(AnonymousUserId.ToString()), option);
            }

            ShopWebContext Context = new ();            

            int? MenuItemId = null;
            if (MenuItem != null)
            {
                MenuItemId = MenuItem.Id;
            }

            int? CategoryId = null;
            if (Category != null)
            {
                CategoryId = Category.Id;
            }

            object[] array = new object[]
            {
                AnonymousUserId,
                MenuItemName,
                MenuItemId,
                CategoryId
            };

            if(MenuItem != null || MenuItemName != null || Category != null)
            {
                Statistics NewStatistics = new()
                {
                    AnonymousUserId = AnonymousUserId,
                    MenuItemName = MenuItemName,
                    MenuItemId = MenuItemId,
                    CategoryId = CategoryId,
                    DateTime = DateTime.Now
                };
                ShopWebContext NewContext = new ();
                NewContext.Statistics.Add(NewStatistics);
                NewContext.SaveChanges();                
            }

            MenuItem = null;
            MenuItemName = null;
            Category = null;
        }

        public static string MenuItemName { get; set; } = null;
        public static MenuItem MenuItem { get; set; } = null;
        public static Category Category { get; set; } = null;
    }
}
