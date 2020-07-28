using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quiz.Models
{
    public class Menu_ViewComponent : ViewComponent
    {
        private readonly DataAccess dataAccess = new DataAccess();

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int Id_User = Convert.ToInt32(HttpContext.Session.GetInt32("User_Id"));
            // HttpContext.Session.GetInt32("Id_Mandant");
            bool DB_Admin = Convert.ToBoolean(HttpContext.Session.GetString("User_Admin"));

            if (Id_User == 17705 || Id_User == 17214)
            {
                DB_Admin = true;
            }

            var menu = new List<Parent_Menu>();
            menu = await dataAccess.GetMenu(DB_Admin);

            return View("Menu_Partial", menu);
        }
    }
}
