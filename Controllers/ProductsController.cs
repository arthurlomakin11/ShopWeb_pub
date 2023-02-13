using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ShopWeb.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Models;
using ShopWeb.Shared;

namespace ShopWeb.Controllers
{
    public class ProductsController : Controller
    {
        public ProductsController()
        {
        }
        public ViewResult List(string category, string subcategory)
        {
            ShopWebContext context = new();

            List<Category> Categories = context.Categories
                .Where(c => c.Active == true)
                .Include(c => c.Images)
                .Include(c => c.Parent)
                .AsSingleQuery()
                .ToList();
            ViewData["Categories"] = Categories;
            ViewData["Category"] = category;
            StatisticsManager.MenuItemName = "Продукція";

            var products = context.Products.Include(product => product.Images);

            if (string.IsNullOrWhiteSpace(category))
            {
                bool ProductsMenuInSeparatePage = SettingsManager.GetValueBool("ProductsMenuInSeparatePage");
                if (ProductsMenuInSeparatePage)
                {
                    return View("SeparatePageCategoryList");
                }
                else
                {
                    ViewData["ShowCategoryList"] = true;
                    ViewData["ShowSubCategoryList"] = false;

                    ViewBag.Products = products.Where(p => p.Active == true).ToList();

                    return View();
                }
            }            
            else if(string.IsNullOrWhiteSpace(subcategory))
            {
                ViewData["ShowCategoryList"] = true;
                ViewData["ShowSubCategoryList"] = false;

                Category CategoryFound = (from c in Categories
                                        where c.Name.Replace(" ", "").Replace("'", "") == category
                                        select c)
                                        .FirstOrDefault();

                StatisticsManager.Category = CategoryFound;

                List<Product> ProductsList;
                if (!CategoryFound.IsSmart)
                {
                    ProductsList = (from product in products
                                    where product.Active == true
                                    where product.Category.Name.Replace(" ", "").Replace("'", "") == category ||
                                    product.Category.Parent.Name.Replace(" ", "").Replace("'", "") == category
                                    select product).ToList();
                }
                else
                {
                    ProductsList = context.Products.FromSqlRaw(CategoryFound.SmartQuery)
                                            .Include(product => product.Images).ToList();
                }

                ViewBag.Products = ProductsList;

                return View();
            }
            else
            {
                ViewData["ShowCategoryList"] = true;
                ViewData["ShowSubCategoryList"] = true;

                
                Category CategoryFound = (from c in Categories
                                          where c.Name.Replace(" ", "").Replace("'", "") == subcategory
                                          where c.Parent.Name.Replace(" ", "").Replace("'", "") == category
                                          select c)
                                         .FirstOrDefault();

                StatisticsManager.Category = CategoryFound;


                List<Product> ProductsList = (from product in products
                                              where product.Active == true
                                              where product.Category.Parent.Name.Replace(" ", "").Replace("'", "") == category
                                              where product.Category.Name.Replace(" ", "").Replace("'", "") == subcategory
                                              select product).ToList();
                ViewBag.Products = ProductsList;

                return View();
            }
        }
    }
}
