using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ShopWeb.Data;

namespace ShopWeb.Models
{
    public class CartManager
    {
        public Cart CurrentCart { get; set; }
        public async Task<dynamic> CurrentCartProductsCount(SignInManager<User> SignInManager, ILocalStorageService LocalStorage)
        {
            SignManager Manager = new()
            {
                Context = new(),
                SignInManager = SignInManager
            };

            try
            {
                if (Manager.IsSignedIn())
                {
                    User User = Manager.User;
                    CurrentCart = User.CurrentCart;
                }
                else
                {
                    CurrentCart = await LocalStorage.GetItemAsync<Cart>("Cart");
                }

                return CurrentCart.CartItems.Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}
