using Microsoft.AspNetCore.Components;

using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;
using System;

namespace ShopWeb.Components
{
    public partial class CartItemComponent
    {
        [Parameter]
        public ShopWebContext Context { get; set; }
        [Parameter]
        public CartItem CartItem { get; set; }
        decimal _Quantity;
        readonly int ShowDescriptionInCart = int.Parse(SettingsManager.GetValue("ShowDescriptionInCart"));
        public decimal Quantity 
        {
            get => decimal.Parse(_Quantity.ToString("G29"));
            set
            {
                if(value > 0)
                {
                    if (CartItem.Product.Countable)
                    {
                        value = Math.Round(value);
                    }
                    _Quantity = value;
                    CartItem.Quantity = value;
                    RefreshEvent.InvokeAsync(CartItem);
                }
            }
        }
        void Decrease()
        {
            if (CartItem.Product.Countable)
            {
                Quantity -= 1M;
            }
            else
            {
                Quantity -= 0.1M;
            }
        }

        void Increase()
        {
            if (CartItem.Product.Countable)
            {
                Quantity += 1M;
            }
            else
            {
                Quantity += 0.1M;
            }
        }
        void Delete()
        {            
            DeleteEvent.InvokeAsync(CartItem);
        }
        protected override void OnInitialized()
        {
            _Quantity = CartItem.Quantity;
        }

        [Parameter]
        public EventCallback<CartItem> RefreshEvent { get; set; }
        [Parameter]
        public EventCallback<CartItem> DeleteEvent { get; set; }
    }
}