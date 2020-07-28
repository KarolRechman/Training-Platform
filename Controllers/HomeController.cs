using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz.Models;
using Wangkanai.Detection;

namespace Quiz.Controllers
{
    /// <summary>
    /// Controller that's responsible for "Landing pages, log in, log out, error, privacy"...
    /// </summary>
    public class HomeController : Controller
    {
        string _browser_Name = "";
        private readonly IUserAgent _useragent;
        private readonly IBrowser _browser;

        private IHttpContextAccessor _accessor;

        DataAccess dataAccess = new DataAccess();

        /// <summary>
        /// Constructor for HomeController
        /// </summary>
        /// <param name="browserResolver">Provides information about user's browser</param>
        /// <param name="accessor">Provides access to HTTP-specific information about an individual HTTP request</param>
        public HomeController(IBrowserResolver browserResolver, IHttpContextAccessor accessor)
        {
            _useragent = browserResolver.UserAgent;
            _browser = browserResolver.Browser;
            _browser_Name = _browser.Type.ToString();
            _accessor = accessor;
        }

        /// <summary>
        /// Log out method, clears whole session
        /// </summary>
        /// <param name="id">User id number from database</param>
        /// <returns>Redirects to Index page, landing page</returns>
        public IActionResult Logout(string id)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Calls Index page, checks browser compatibility
        /// </summary>
        /// <returns>Index page View or redirects to Admin or User pages</returns>
        [HttpGet]
        public IActionResult Index()
        {
            if (_browser_Name == "Generic")
            {
                ViewBag.Type = "Please don't use Internet Explorer !!!";
                ViewBag.Type2 = "The content of these pages is not intended for this browser.";
                ViewBag.Type3 = "You could choose Microsoft Edge, Chrome or other browser... Please !";
                HttpContext.Session.Clear();
            }
            if (HttpContext.Session.GetString("User_Admin") == "False")
            {
                return RedirectToAction("Trainings_List", "UserPages");
            }
            if (HttpContext.Session.GetString("User_Admin") == "True")
            {
                return RedirectToAction("Index", "AdminPages");
            }

            return View();
        }
        /// <summary>
        /// Processes and sends log in data
        /// </summary>
        /// <param name="logins">Logins object with users data</param>
        /// <returns>Redirects to Admin or User pages or provides information about unsuccessful log in</returns>
        [HttpPost]
        public IActionResult Index([Bind]Login logins)
        {
            logins.Ip_Address = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//maybe it's useless ?!?!?!?!?
            logins.Host_Name = Dns.GetHostName();
            logins.App_Name = "Training_Test";
            logins.Browser_Name = _browser_Name;

            IPAddress[] iP_Addresses = Dns.GetHostAddresses(logins.Host_Name);

            foreach (IPAddress ip4 in iP_Addresses.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                if (logins.Ip_Address == "::1")
                {
                    logins.Ip_Address = ip4.ToString();
                }
            }

            if (ModelState.IsValid)
            {
                User user = new User();

                if (logins.login == "Test_User" && logins.Password == "1")
                {
                    user.Id_User = 0;
                    user.Id_Mandant = 0;
                    user.First_Name = "Test_User";
                    user.Last_Name = "Test_User";

                    user.Email = "";
                    user.DB_Admin = false;

                    HttpContext.Session.SetInt32("User_Id", user.Id_User);
                    HttpContext.Session.SetInt32("Id_Mandant", user.Id_Mandant);
                    HttpContext.Session.SetString("User_Admin", user.DB_Admin.ToString());
                    HttpContext.Session.SetString("User_First_Name", user.First_Name);
                    HttpContext.Session.SetString("User_Last_Name", user.Last_Name);

                    return RedirectToAction("Trainings_List", "UserPages");
                }
                //else if (logins.login == "KRechman" && logins.Password == "1")
                //{
                //    user.Id_User = 17705;
                //    user.DB_Admin = true;
                //    HttpContext.Session.SetInt32("User_Id", user.Id_User);
                //    HttpContext.Session.SetString("User_Admin", user.DB_Admin.ToString());

                //    return RedirectToAction("Index", "AdminPages");
                //}
                else
                {

                    //user = dataAccess.LoginUserLocal(logins);
                    user = dataAccess.LoginUser(logins);
                    if (user.Info_Login == null || user.Info_Login == "")
                    {
                        if (dataAccess.IsAdmin(user.Id_User))
                        {                      
                            user.DB_Admin = true;
                        }

                        HttpContext.Session.SetInt32("User_Id", user.Id_User);
                        HttpContext.Session.SetInt32("Id_Mandant", user.Id_Mandant);
                        HttpContext.Session.SetString("User_Admin", user.DB_Admin.ToString());
                        HttpContext.Session.SetString("User_First_Name", user.First_Name);
                        HttpContext.Session.SetString("User_Last_Name", user.Last_Name);

                        if (user.DB_Admin == false)
                        {
                            return RedirectToAction("Trainings_List", "UserPages");
                        }
                        else
                        {
                            return RedirectToAction("Index", "AdminPages");
                        }
                    }
                    else
                    {
                        ViewBag.Type = user.Info_Login.Replace("<p>", " ").Replace("</p>", " ");
                        return View();
                    }
                }

            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
