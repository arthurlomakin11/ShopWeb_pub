using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Data;
using System.Linq;
using System.Security.Claims;

namespace ShopWeb.Models
{
    public class SignManager
    {
        public User User 
        {
            get                
            {
                User CurrentUser = SignInManager.UserManager.GetUserAsync(UserClaim).Result;
                CurrentUser = SignInManager.UserManager.Users
                    .Include(u => u.Carts)
                        .ThenInclude(c => c.ShopAdress)
                            .ThenInclude(c => c.Shops)
                    .Include(u => u.Carts)
                        .ThenInclude(c => c.CartItems)
                            .ThenInclude(i => i.Product)
                    .AsSingleQuery()
                    .First(user => user == CurrentUser);
                return CurrentUser;
            }
        }
        public bool IsSignedIn()
        {
            return SignInManager.IsSignedIn(UserClaim);
        }
        readonly ClaimsPrincipal UserClaim = new HttpContextAccessor().HttpContext.User;
        public SignInManager<User> SignInManager { get; set; }
        public ShopWebContext Context { get; set; }

        public void SaveChanges()
        {
            Context.SaveChanges();
        }
    }
}
