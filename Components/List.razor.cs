using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;

namespace ShopWeb.Components
{
    public partial class List
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        readonly string ProductsDefaultSortType = SettingsManager.GetValue("ProductsDefaultSortType");
        readonly bool ShowSorting = SettingsManager.GetValueBool("ShowSorting");
        readonly bool ProductsSearchEnabled = SettingsManager.GetValueBool("ProductsSearchEnabled");
        readonly bool AddPaginationInProductsList = SettingsManager.GetValueBool("AddPaginationInProductsList");
        readonly int PaginatedProductsOnPageCount = SettingsManager.GetValueInt("PaginatedProductsOnPageCount");
        public List()
        {
            if (!string.IsNullOrWhiteSpace(ProductsDefaultSortType))
            {
                SortValue = Enum.Parse<SortEnum>(ProductsDefaultSortType);
            }
        }
        public enum SortEnum
        {
            Rating,
            PriceAsc,
            PriceDesc,
            Alphabet
        }

        SortEnum SortValue = SortEnum.Rating;


        [Parameter]
        public List<Product> Products { get; set; }
        public List<Product> SortedProducts()
        {
            IEnumerable<Product> Sorted = SortValue switch
            {
                SortEnum.PriceAsc => Products.OrderBy(p => p.Price),
                SortEnum.PriceDesc => Products.OrderByDescending(p => p.Price),
                SortEnum.Rating => Products.OrderByDescending(p => p.Promotional).ThenByDescending(p => p.Rating).ThenBy(p => p.Name).ThenBy(p => p.Category),
                SortEnum.Alphabet => Products.OrderBy(p => p.Name),
                _ => new List<Product>()
            };

            return Sorted.ToList();
        }
        public string text = "";        
        public List<Product> ProductsSearchList()
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                return new ShopWebContext().Products
                .Where(product =>
                    product.Name.Trim().Contains(SearchText.Trim()) ||
                    product.Description.Trim().Contains(SearchText.Trim()) ||

                    StringExtensions.Translate(product.VendorCode.Trim(), "А", "A")
                    .Contains
                    (
                        StringExtensions.Translate(SearchText.Trim(), "А", "A")
                    )
                )
                .Where(product => product.Active == true)
                .Include(p => p.Images)
                .ToList();
            }
            else
            {
                return null;
            }
        }

        public string _searchText = "";
        public string SearchText 
        {
            get 
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                ActivePage = 0;
            }
        }
        public int ActivePage { get; set; } = 0;

        public void GetToPreviousPage()
        {
            GetToActivePage(ActivePage - 1);
        }
        public void GetToNextActivePage()
        {
            GetToActivePage(ActivePage + 1);
        }
        public async void GetToActivePage(int PageNumber)
        {
            if(ActivePage != PageNumber)
            {
                ActivePage = PageNumber;
                await JSRuntime.InvokeVoidAsync("SmoothlyScrollToTop");
            }
        }
    }
}
