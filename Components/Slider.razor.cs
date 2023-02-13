using Microsoft.AspNetCore.Components;
using ShopWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopWeb.Components
{
    public partial class Slider
    {
        [Parameter]
        public MenuItem SliderParent { get; set; }
        IEnumerable<MenuItem> SliderElements { get; set; }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            SliderElements = from item in Context.Menu
                             where item.Parent == SliderParent
                             where item.Active == true
                             orderby item.SequentialNumber ascending
                             select item;
        }
    }
}
