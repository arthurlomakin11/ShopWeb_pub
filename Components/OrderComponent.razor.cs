using System;
using System.Linq;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;

using ShopWeb.Models;

using ShopWeb.Data;

namespace ShopWeb.Components
{
    public partial class OrderComponent
    {
        [Parameter]
        public Cart Order { get; set; }

        User User;
        Cart CurrentCart = null;
        bool Loading = false;
        bool Done = false;

        [Inject]
        protected SignInManager<User> SignInManager { get; set; }

        [Inject]
        protected ShopWebContext Context { get; set; }

        protected override void OnInitialized()
        {
            SignManager Manager = new()
            {
                Context = Context,
                SignInManager = SignInManager
            };

            User = Manager.User;
            CurrentCart = User.CurrentCart;

            if(CurrentCart == null)
            {
                CurrentCart = new Cart
                {
                    Buyer = User
                };
                User.Carts.Add(CurrentCart);
                Context.SaveChanges();
            }

            base.OnInitialized();
        }

        public void CopyElementsToCart()
        {
            Loading = true;            

            foreach (CartItem OrderItem in Order.CartItems)
            {
                if (!CurrentCart.CartItems.Exists(o => o.ProductId == OrderItem.ProductId))
                {
                    CurrentCart.CartItems.Add
                    (
                        new CartItem
                        {
                            CartId = Order.Id,
                            ProductId = OrderItem.ProductId,
                            Quantity = OrderItem.Quantity
                        }
                    );
                }
            }
            Context.SaveChanges();

            Done = true;
        }
    }
}
