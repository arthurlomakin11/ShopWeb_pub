using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;

namespace ShopWeb.Components
{
    public partial class MainPageProductsCollectionsList
    {
        public ShopWebContext Context = new();
        public int ShowProductsCollectionsOnMainPage = int.Parse(SettingsManager.GetValue("ShowProductsCollectionsOnMainPage"));
        public Cart CurrentCart { get; set; } = null;
        List<ProductCollection> ProductCollections { get; set; }
        public MainPageProductsCollectionsList()
        {
            ProductCollections = Context.ProductCollections
                                        .Where(p => p.Active == true)
                                        .Where(p => p.ShowOnMainPage == true)
                                        .Include(p => p.Products)
                                            .ThenInclude(p => p.Product)
                                                .ThenInclude(product => product.Images)
                                        .AsSingleQuery()
                                        .ToList();            
        }

        protected override async void OnInitialized()
        {
            SignManager Manager = new()
            {
                Context = Context,
                SignInManager = SignInManager
            };

            if (Manager.IsSignedIn())
            {
                User User = Manager.User;
                CurrentCart = User.CurrentCart;
            }
            else
            {
                CurrentCart = await LocalStorage.GetItemAsync<Cart>("Cart");
            }

            base.OnInitialized();
        }
    }
}
