using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ShopWeb.Data;

namespace ShopWeb.Components
{
    public partial class Feedback
    {
        [Parameter]
        public string FeedbackHeader { get; set; }

        [Parameter]
        public int TextAreaRows { get; set; } = 2;


        public string Fio { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public bool Submited = false;

        ShopWebContext Context = new ShopWebContext();
        void Submit()
        {
            Context.Messages.Add(new Message
            {
                Fio = Fio,
                Email = Email,
                Phone = Phone,
                TextMessage = Message
            });
            Context.SaveChanges();

            Submited = true;
        }
    }
}
