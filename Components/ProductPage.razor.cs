using System.Collections.Generic;
using System.Linq;
using System;

using Microsoft.AspNetCore.Components;

using ShopWeb.Models;

using ShopWeb.Data;
using ShopWeb.Shared;

namespace ShopWeb.Components
{
    public partial class ProductPage
    {
        Cart Cart = null;
        Cart LocalCart = null;
        User User = null;
        SignManager Manager = null;
        List<Image> Images { get; set; } = new() { };
        readonly string ImagesFolderLocalPath = SettingsManager.GetValue("ImagesFolderLocalPath");
        readonly bool ImageAsBackgroundInProduct = SettingsManager.GetValueBool("ImageAsBackgroundInProduct");
        readonly bool CanUserBuyProducts = SettingsManager.GetValueBool("CanUserBuyProducts");
        readonly bool ShowMultipleImagesInProduct = SettingsManager.GetValueBool("ShowMultipleImagesInProduct");
        readonly string InCartButtonTextInProduct = SettingsManager.GetValue("InCartButtonTextInProduct");
        readonly bool ShowIsAvailableTextInProduct = SettingsManager.GetValueBool("ShowIsAvailableTextInProduct");
        readonly bool AllProductsIsAvailable = SettingsManager.GetValueBool("AllProductsIsAvailable");
        readonly bool ShowVendorCodeInProductCard = SettingsManager.GetValueBool("ShowVendorCodeInProductCard");
        bool InCart = false;

        protected override async void OnParametersSet()
        {            
            Images = CurrentProduct.Images;


            Manager = new SignManager
            {
                Context = Context,
                SignInManager = SignInManager
            };
            if (Manager.IsSignedIn())
            {
                User = Manager.User;
                Cart = User.CurrentCart;
            }

            LocalCart = await LocalStorage.GetItemAsync<Cart>("Cart"); // get cart from LocalStorage


            Cart CurrentCart = null;
            if (User != null && Cart != null)
            {
                CurrentCart = Cart;
            }
            else if (LocalCart != null)
            {
                CurrentCart = LocalCart;
            }


            if (CurrentCart != null)
            {
                PortionsCount = CurrentCart.CartItems.Count(product => product.Product.Id == CurrentProduct.Id);
                if (PortionsCount > 0)
                {
                    InCart = true;
                    StateHasChanged();
                }
            }

            base.OnParametersSet();
        }

        [Parameter]
        public Product CurrentProduct { get; set; }

        int PortionsCount = 0;
        async void AddToCart()
        {
            if (User != null)
            {
                if (Cart == null)
                {
                    Cart = new Cart
                    {
                        Buyer = User
                    };
                    User.Carts.Add(Cart);
                }
                CartItem NewCartItem = new()
                {
                    Quantity = Count,
                    Product = Context.Products.First(p => p.Id == CurrentProduct.Id)
                };
                if (!Cart.CartItems.Exists(item => item.Product.Id == NewCartItem.Product.Id))
                {
                    Cart.CartItems.Add(NewCartItem);
                }

                Manager.SaveChanges();

                PortionsCount++;
                InCart = true;
            }
            else
            {
                LocalCart = await LocalStorage.GetItemAsync<Cart>("Cart"); // get cart from LocalStorage

                if (LocalCart == null)
                {
                    LocalCart = new Cart();
                }

                CartItem NewCartItem = new()
                {
                    Quantity = Count,
                    Product = CurrentProduct
                };
                LocalCart.CartItems.Add(NewCartItem);

                PortionsCount++;
                InCart = true;
                await LocalStorage.SetItemAsync("Cart", LocalCart);
                StateHasChanged();

                //NavManager.NavigateTo("Identity/Account/Login", true);
            }            
        }
        string text = "";
        async void DeleteFromCart()
        {
            if (User != null)
            {
                if (Cart != null)
                {
                    Cart.CartItems.RemoveAll(cart => cart.Product.Id == CurrentProduct.Id);

                    Manager.SaveChanges();

                    InCart = false;

                    PortionsCount = 0;
                }
            }
            else
            {
                LocalCart = await LocalStorage.GetItemAsync<Cart>("Cart"); // get cart from LocalStorage

                LocalCart.CartItems.RemoveAll(item => item.Product.Id == CurrentProduct.Id);
                InCart = false;
                PortionsCount = 0;
                await LocalStorage.SetItemAsync("Cart", LocalCart);

                StateHasChanged();
            }
        }

        decimal _Count = 1;
        public decimal Count
        {
            get => _Count;
            set
            {
                if (value > 0)
                {
                    if (CurrentProduct.Countable)
                    {
                        value = Math.Round(value);
                    }
                    _Count = value;
                }
            }
        }


        void Decrease()
        {
            if (CurrentProduct.Countable)
            {
                Count -= 1M;
            }
            else
            {
                Count -= 0.1M;
            }
        }

        void Increase()
        {
            if (CurrentProduct.Countable)
            {
                Count += 1M;
            }
            else
            {
                Count += 0.1M;
            }
        }
    }
}
