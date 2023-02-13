using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using ShopWeb.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShopWeb.Components
{
    public partial class Wheel
    {
        List<Product> Products = new ShopWebContext().Products.Where(p => p.IsGift == true).ToList();
        bool Done = false;
        Product ProductFound;
        public Wheel()
        {
            OnTimeOutOccured = async text =>
            {
                ProductFound = Products.First(p => p.Name == text);                
                DatabaseFacade Database = new ShopWebContext().Database;
                Database.ExecuteSqlRaw("INSERT INTO Gifts (CartId, ProductId, ProductPrice, Amount) VALUES({0}, {1}, {2}, {3})", Cart.Id, ProductFound.Id, ProductFound.Price, ProductFound.GiftAmount);

                string GreetingsCode = @$"
                    window.greetings = async () => {{
                        let text = `Вітаємо, {User.Fio}! Ви виграли {ProductFound.GiftAmount:G29} кг {ProductFound.Name}`
                        let el = document.querySelector("".wheel_greetings_text"")
                        text_end = 0
                        while (text_end <= text.length)
                        {{
                                el.innerText = text.slice(0, text_end)
                            text_end++
                            await new Promise(r => setTimeout(r, 50))
                        }}
                    }}
                    greetings();
                ";
                Done = true;
                StateHasChanged();
                await JS.InvokeVoidAsync("eval", GreetingsCode);
            };

            string GetFileText(string path)
            {
                return File.ReadAllText(_env.ContentRootFileProvider.GetFileInfo(path).PhysicalPath);
            }

            LoadedEvent = async () => 
            {   
                string winwheeljs = GetFileText("wwwroot/wheel/Winwheel.min.js");
                string tweenmaxjs = GetFileText("wwwroot/wheel/TweenMax.min.js");
                string wheeljs = GetFileText("wwwroot/wheel/wheel.js");

                await JS.InvokeVoidAsync("eval", winwheeljs);
                await JS.InvokeVoidAsync("eval", tweenmaxjs);
                await JS.InvokeVoidAsync("eval", JsCode);
                await JS.InvokeVoidAsync("eval", wheeljs);
            };


            string[] colors = { "#ee1c24", "#3cb878", "#f6989d", "#00aef0", "#f26522", "#e70697" };

            int colorInt = 0;
            IEnumerable<Segment> segments = new List<Segment>();
            foreach (Product Product in Products)
            {
                segments = segments.Append(new Segment
                {
                    text = Product.Name,
                    fillStyle = colors[colorInt]
                });

                colorInt++;
                if (colorInt >= colors.Length)
                {
                    colorInt = 0;
                }
            }
            segmentsString = "window.segments = " + JsonConvert.SerializeObject(segments);
            JsCode = segmentsString;
        }
        string JsCode = "";

        [Parameter]
        public Cart Cart { get; set; }

        [Parameter]
        public User User { get; set; }

        class Segment
        {
            public string text { get; set; }
            public string fillStyle { get; set; }
            public string textFontFamily { get; set; } = "Arial";
        }
        string segmentsString;

        [JSInvokable]
        public static void WheelFinished(string text)
        {
            OnTimeOutOccured(text);
        }
        [JSInvokable]
        public static void LoadedMethod()
        {
            LoadedEvent();
        }

        public string text = "";

        protected delegate void OnTimeout(string text);
        private static event OnTimeout OnTimeOutOccured;
        protected delegate void Loaded();
        private static event Loaded LoadedEvent;


        void Redirect()
        {
            NavManager.NavigateTo("/OrderDone", true);
        }
    }
}
