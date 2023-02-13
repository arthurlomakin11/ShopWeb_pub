using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ShopWeb.Components
{
    public partial class Checkout
    {
        public ShopWebContext Context = new();
        public bool Done = false;
        User User;
        List<Shop> Shops { get; set; }
        List<DeliveryTime> DeliveryTimes { get; set; }

        readonly int SelectAdressInCheckout = int.Parse(SettingsManager.GetValue("SelectAdressInCheckout"));
        readonly int SelectLocalityInCheckout = int.Parse(SettingsManager.GetValue("SelectLocalityInCheckout"));
        readonly int ShowWheelInCheckout = int.Parse(SettingsManager.GetValue("ShowWheelInCheckout"));
        readonly int PickupEnabledInCheckout = int.Parse(SettingsManager.GetValue("PickupEnabledInCheckout"));
        readonly int OnlyOneShopDelivery = int.Parse(SettingsManager.GetValue("OnlyOneShopDelivery"));
        readonly string DeliveryPriceCheckoutPattern = SettingsManager.GetValue("DeliveryPriceCheckoutPattern");
        List<DeliveryPriceTag> DeliveryPriceTags;
        bool Loaded = false;
        protected override async void OnInitialized()
        {
            base.OnInitialized();

            Context.ChangeTracker.AutoDetectChangesEnabled = false;
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            DeliveryPriceTags = Context.DeliveryPriceTags.AsNoTracking().ToList();

            Adresses = Context.Adresses
                    .AsNoTracking()
                    .Include(a => a.ShopAdresses)
                    .AsNoTracking()
                    .Include(a => a.Parent)
                    .AsNoTracking()
                    .ToList();

            FilterAdresses = Context.Adresses
                .AsNoTracking()
                .Where(a => a.Parent == null)
                .ToList();

            DeliveryTimes = Context.DeliveryTimes.AsNoTracking().Where(time => time.Active == true).ToList();

            Shops = Context.Shops.AsNoTracking().Where(s => s.Pickup == true).ToList();


            checkoutModel = new CheckoutModel
            {
                ShopAdressId = Adresses.First().Id
            };

            SignManager Manager = new()
            {
                Context = Context,
                SignInManager = SignInManager
            };

            if (Manager.IsSignedIn())
            {
                User = Manager.User;
                CurrentCart = User.CurrentCart;

                checkoutModel.Adress = User.LastCartAdress;
            }
            else
            {
                CurrentCart = await LocalStorage.GetItemAsync<Cart>("Cart");
            }

            Loaded = true;
            StateHasChanged();
        }

        Cart CurrentCart;

        public IEnumerable<Adress> Adresses;
        public IEnumerable<Adress> FilterAdresses;

        bool ClosedBool = false;

        void Close()
        {
            ClosedBool = true;
            Closed.InvokeAsync();
        }

        [Parameter]
        public EventCallback Closed { get; set; }

        public static Adress EmptyAdress { get; set; } = new Adress
        {
            Name = "Виберіть адресу"
        };

        class CheckoutModel
        {
            [Required(ErrorMessage = @"Поле адреса обов'язкове")]
            [StringLength(100, ErrorMessage = "Адреса дуже довга")]
            public string Adress { get; set; }

            [ValidAdress]
            public int ShopAdressId { get; set; }

            [ValidAdress]
            public int ShopFilterAdressId { get; set; }

            public string Comment { get; set; }

            public string AdressTextFilter { get; set; } = "";

            public DeliveryType DeliveryType { get; set; } = DeliveryType.Delivery;

            public int? DeliveryTimeId { get; set; } = null;

            public int? ShopId { get; set; } = null;

            public string FullName { get; set; }

            public string PhoneNumber { get; set; }
        }       
        void ChangeAdressName(ChangeEventArgs e)
        {
            string value = e.Value.ToString();
            int AdressId = 0;
            if (value.Length != 0)
            {
                Adress SelectedAdress = Adresses.FirstOrDefault(a => a.FullName == value);
                if (SelectedAdress != null)
                {
                    AdressId = SelectedAdress.Id;
                }
            }            
            checkoutModel.ShopAdressId = AdressId;
        }

        void ChangeShopId(ChangeEventArgs e)
        {
            int value = int.Parse(e.Value.ToString());
            checkoutModel.ShopId = value;
        }

        void ChangeFilterAdress(ChangeEventArgs e)
        {
            checkoutModel.ShopFilterAdressId = int.Parse(e.Value.ToString());
        }

        void ChangeDeliveryTime(ChangeEventArgs e)
        {
            int value = int.Parse(e.Value.ToString());
            checkoutModel.DeliveryTimeId = value;
        }

        public class ValidAdress : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                int adressId = (int) value;
                if(adressId != 0 || int.Parse(SettingsManager.GetValue("SelectAdressInCheckout")) == 0 || int.Parse(SettingsManager.GetValue("OnlyOneShopDelivery")) == 1)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Виберіть адресу");
                }
            }
        }

        private CheckoutModel checkoutModel;
        public string text = "";

        private void HandleInvalidSubmit()
        {
            if(checkoutModel.DeliveryType == DeliveryType.Pickup || OnlyOneShopDelivery == 1)
            {
                HandleValidSubmit();
            }
        }        
        private async void HandleValidSubmit()
        {
            if(User == null)
            {            
                ShopWebContext CheckoutContext = new();
                CheckoutContext.ChangeTracker.AutoDetectChangesEnabled = false;
                CheckoutContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                DateTime CreationDateTime = DateTime.Now;
                CreationDateTime = CreationDateTime.Date
                    .AddHours(CreationDateTime.Hour)
                    .AddMinutes(CreationDateTime.Minute)
                    .AddSeconds(CreationDateTime.Second);

                Cart NewCart = new()
                {
                    BuyerId = User?.Id,
                    Comment = checkoutModel.Comment,                    
                    CartStatus = new()
                    {
                        CreationDateTime = CreationDateTime,
                        Deleted = false
                    },
                    Adress = checkoutModel.DeliveryType == DeliveryType.Delivery ? checkoutModel.Adress : null,
                    DeliveryType = checkoutModel.DeliveryType,
                    PhoneNumber = checkoutModel.PhoneNumber,
                    FullName = checkoutModel.FullName,
                    Status = StatusEnum.Formalized
                };




                if (checkoutModel.DeliveryType == DeliveryType.Delivery)
                {
                    DeliveryPriceTag SuitableDeliveryPriceTag = GetSuitableDeliveryPriceTag();
                    NewCart.DeliveryPrice = SuitableDeliveryPriceTag != null ? SuitableDeliveryPriceTag.Price : 0;                  
                }
                else if (OnlyOneShopDelivery == 0)
                {
                    NewCart.DeliveryTime = DeliveryTimes.First(time => time.Id == checkoutModel.DeliveryTimeId).Name;
                    
                    NewCart.ShopId = checkoutModel.ShopId;
                }
                else
                {
                    NewCart.ShopId = Context.Shops.First().Id;
                }


                List<Product> Products = new ShopWebContext().Products.AsNoTracking().Where(p => p.Active == true).ToList();                

                foreach (CartItem CartItem in CurrentCart.CartItems)
                {
                    Product Product = Products.First(p => p.Id == CartItem.Product.Id);

                    NewCart.CartItems.Add(new()
                    {
                        ProductId = Product.Id,
                        Quantity = CartItem.Quantity,
                        Price = Product.Price,
                        DollarsPrice = Product.DollarsPrice
                    });
                }

                CheckoutContext.Add(NewCart);
                CheckoutContext.SaveChanges();
                CheckoutContext.Dispose();

                if(User == null)
                {
                    await LocalStorage.RemoveItemAsync("Cart");
                }

                if (checkoutModel.DeliveryType == DeliveryType.Delivery)
                {
                    Context.Database.ExecuteSqlRaw("UPDATE Carts SET ShopAdressId = {0} WHERE CartStatus_CreationDateTime = {1} AND PhoneNumber = {2} AND FullName={3}", checkoutModel.ShopAdressId, CreationDateTime, checkoutModel.PhoneNumber, checkoutModel.FullName);                    

                    Adress SelectedAdress = Adresses.Where(a => a.Id == checkoutModel.ShopAdressId).First();
                    ShopsAdresses ShopsAdresses;
                    if (SelectedAdress.ShopAdresses.Count > 0)
                    {
                        ShopsAdresses = Context.ShopsAdresses.AsNoTracking()
                            .Where(sa => sa.AdressId == SelectedAdress.Id)
                            .Include(sa => sa.Shop).AsNoTracking()
                            .OrderBy(shop => shop.SequentialNumber)
                            .First();
                    }
                    else
                    {
                        ShopsAdresses = Context.ShopsAdresses.AsNoTracking()
                            .Where(sa => sa.AdressId == 1)
                            .Include(sa => sa.Shop).AsNoTracking()
                            .First();
                    }

                    if (ShopsAdresses.Shop.ProcessedManually == true)
                    {
                        Context.Database.ExecuteSqlRaw("UPDATE Carts SET Status = {0} WHERE CartStatus_CreationDateTime = {1}", StatusEnum.ShopsCanceled, CreationDateTime);
                    }

                    Context.Database.ExecuteSqlRaw("UPDATE Carts SET ShopId = {0} WHERE CartStatus_CreationDateTime = {1}", ShopsAdresses.Shop.Id, CreationDateTime);
                }
            }
            else
            {
                CurrentCart.BuyerId = User?.Id;
                CurrentCart.Comment = checkoutModel.Comment;
                CurrentCart.CartStatus = new()
                {
                    CreationDateTime = DateTime.Now,
                    Deleted = false
                };
                CurrentCart.Adress = checkoutModel.DeliveryType == DeliveryType.Delivery ? checkoutModel.Adress : null;
                CurrentCart.DeliveryType = checkoutModel.DeliveryType;
                CurrentCart.PhoneNumber = checkoutModel.PhoneNumber;
                CurrentCart.Status = StatusEnum.Formalized;



                if (checkoutModel.DeliveryType == DeliveryType.Delivery)
                {
                    DeliveryPriceTag SuitableDeliveryPriceTag = GetSuitableDeliveryPriceTag();
                    CurrentCart.DeliveryPrice = SuitableDeliveryPriceTag != null ? SuitableDeliveryPriceTag.Price : 0;
                }
                else
                {
                    CurrentCart.DeliveryTime = DeliveryTimes.First(time => time.Id == checkoutModel.DeliveryTimeId).Name;

                    CurrentCart.ShopId = checkoutModel.ShopId;
                }


                List<Product> Products = new ShopWebContext().Products.AsNoTracking().Where(p => p.Active == true).ToList();

                CurrentCart.CartItems.ForEach(c => c.Price = c.Product.Price);
                CurrentCart.CartItems.ForEach(c => c.DollarsPrice = c.Product.DollarsPrice);

                Context.Update(CurrentCart);
                Context.SaveChanges();

                if (User == null)
                {
                    await LocalStorage.RemoveItemAsync("Cart");
                }

                if (checkoutModel.DeliveryType == DeliveryType.Delivery)
                {
                    Context.Database.ExecuteSqlRaw("UPDATE Carts SET ShopAdressId = {0} WHERE Id = {1}", checkoutModel.ShopAdressId, CurrentCart.Id);

                    Adress SelectedAdress = Adresses.Where(a => a.Id == checkoutModel.ShopAdressId).First();
                    ShopsAdresses ShopsAdresses;
                    if (SelectedAdress.ShopAdresses.Count > 0)
                    {
                        ShopsAdresses = Context.ShopsAdresses.AsNoTracking()
                            .Where(sa => sa.AdressId == SelectedAdress.Id)
                            .Include(sa => sa.Shop).AsNoTracking()
                            .OrderBy(shop => shop.SequentialNumber)
                            .First();
                    }
                    else
                    {
                        ShopsAdresses = Context.ShopsAdresses.AsNoTracking()
                            .Where(sa => sa.AdressId == 1)
                            .Include(sa => sa.Shop).AsNoTracking()
                            .First();
                    }

                    if (ShopsAdresses.Shop.ProcessedManually == true)
                    {
                        Context.Database.ExecuteSqlRaw("UPDATE Carts SET Status = {0} WHERE Id = {1}", StatusEnum.ShopsCanceled, CurrentCart.Id);
                    }

                    Context.Database.ExecuteSqlRaw("UPDATE Carts SET ShopId = {0} WHERE Id = {1}", ShopsAdresses.Shop.Id, CurrentCart.Id);
                }
            }

            Context.Dispose();

            Done = true;
            if (ShowWheelInCheckout == 0)
            {
                NavManager.NavigateTo("/OrderDone", true);
            }
        }

        DeliveryPriceTag GetSuitableDeliveryPriceTag()
        {
            try
            {
                decimal CartFinalSum = CurrentCart.Sum;

                DeliveryPriceTag x = DeliveryPriceTags.OrderByDescending(d => d.StartCartPrice)
                    .FirstOrDefault(d => d.StartCartPrice < CartFinalSum);

                return x;
            }
            catch
            {
                return null;
            }            
        }
    }
}