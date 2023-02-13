using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;
using System.Threading.Tasks;

namespace ShopWeb.Components
{
    public partial class CartComponent
    {
        User User;
        Cart CurrentCart = null;
        bool ShowCheckout = false;
        bool Rendered = false;
        bool Agreed = true;
        readonly int HideCartOnCheckout = int.Parse(SettingsManager.GetValue("HideCartOnCheckout"));
        readonly int ShowAgreementCheckboxInCart = int.Parse(SettingsManager.GetValue("ShowAgreementCheckboxInCart"));
        readonly string AgreementTextInCart = SettingsManager.GetValue("AgreementTextInCart");
        readonly decimal MinOrderSum = decimal.Parse(SettingsManager.GetValue("MinOrderSum"));
        protected override async void OnInitialized()
        {
            if(ShowAgreementCheckboxInCart == 1)
            {
                Agreed = false;
            }

            SignManager Manager = new()
            {
                Context = Context,
                SignInManager = SignInManager
            };

            if (Manager.IsSignedIn())
            {
                User = Manager.User;
                CurrentCart = User.CurrentCart;
            }
            else
            {
                CurrentCart = await LocalStorage.GetItemAsync<Cart>("Cart");
            }

            Rendered = true;
            StateHasChanged();

            base.OnInitialized();
        }
        async void Refresh(CartItem CartItem = null)
        {
            if(CartItem != null)
            {
                if(User != null && CurrentCart != null)
                {
                    Context.Update(CartItem);
                    Context.SaveChanges();
                    CurrentCart = User.CurrentCart;
                }
                else
                {
                    await LocalStorage.SetItemAsync("Cart", CurrentCart);
                }
            }
        }

        async void Delete(CartItem CartItem = null)
        {
            if (CartItem != null)
            {
                if (User != null && CurrentCart != null)
                {
                    Context.Remove(CartItem);
                    Context.SaveChanges();
                    CurrentCart = User.CurrentCart;
                }
                else
                {
                    CurrentCart.CartItems.Remove(CartItem);
                    await LocalStorage.SetItemAsync("Cart", CurrentCart);
                }
            }
        }

        async void AddNewCart()
        {
            if (User != null)
            {
                User.Carts.Add(new Cart
                {
                    Buyer = User
                });
                Context.SaveChanges();
            }
            else
            {
                CurrentCart = new Cart();
                await LocalStorage.SetItemAsync("Cart", CurrentCart);
                StateHasChanged();
            }
        }

        async Task HideCheckout()
        {
            await Task.Delay(600);
            ShowCheckout = false;
        }


        void Changed()
        {
            OnInitialized();
            StateHasChanged();
        }
    }
}