using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Quiz.Models;
using ReflectionIT.Mvc.Paging;

namespace Quiz.Controllers
{
    /// <summary>
    /// Controller that's responsible for all "User's Pages"
    /// </summary>
    public class UserPagesController : Controller
    {
        DataAccess dataAccess = new DataAccess();
        Helper helper = new Helper();

        /// <summary>
        /// Calls Training list page
        /// </summary>
        /// <param name="filter">Word we're looking for</param>
        /// <param name="page">Page number</param>
        /// <param name="sortExpression">Sets order by specific column</param>
        /// <returns>Training list page with available trainings for specific user</returns>
        public async Task<IActionResult> Trainings_List(string filter, int page = 1, string sortExpression = "Id")
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //if (HttpContext.Session.GetString("training") == "training")
            //{
            //    int Id_Campaign = Convert.ToInt32(HttpContext.Session.GetInt32("Id_Campaign"));
            //    int Id_Presentation = Convert.ToInt32(HttpContext.Session.GetInt32("Id_Presentation"));
            //    HttpContext.Session.Remove("Id_Campaign");
            //    HttpContext.Session.Remove("Id_Presentation");
            //    return RedirectToAction("Training_Page", "PresentationPages", new { Id_Campaign, Id_Presentation });
            //}
            else
            {
                HttpContext.Session.Remove("test");
                int Id_User = Convert.ToInt32(HttpContext.Session.GetInt32("User_Id"));
                //HttpContext.Session.SetInt32("Campaign_Id", id);

                var campaigns = new List<Training_Campaign>();
                campaigns = await dataAccess.AvailableTrainings(Id_User);

                foreach (var camp in campaigns)
                {
                    camp.Start_Date = camp.Start_Date.Replace("00:00:00", "").TrimEnd();
                    camp.End_Date = camp.End_Date.Replace("23:59:59", "").TrimEnd();

                    foreach (var pres in camp.Presentations)
                    {
                        Tuple<string, double, int, string, string> result = dataAccess.GetUserPoints(camp.Id, pres.Id, Id_User);

                        pres.User_Points = result.Item1;
                        pres.Points_Percentage = result.Item2;
                        pres.Attempt_Date = result.Item4;
                        pres.Attempts_Left = camp.Attempts - result.Item3;
                        pres.Author_Name = result.Item5;
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    campaigns = campaigns.Where(c => c.Name.ToLower().Contains(filter.ToLower()) || c.Presentations[0].Name.ToLower().Contains(filter.ToLower())).ToList();
                }

                var model = PagingList.Create(campaigns, 4, page, sortExpression, "Id");

                model.RouteValue = new RouteValueDictionary {
                         { "filter", filter}
            };

                if (HttpContext.Session.GetString("Error") == null)
                {
                    ViewBag.Error = null;
                    ViewBag.Msg = null;
                }
                else
                {
                    ViewBag.Error = HttpContext.Session.GetString("Error");
                    ViewBag.Msg = HttpContext.Session.GetString("Msg");
                    HttpContext.Session.Remove("Error");
                    HttpContext.Session.Remove("Msg");
                }
                return View(model);
            }
        }

        /// <summary>
        /// Retrieves all test results for specific user
        /// </summary>
        /// <param name="campaign">Training campaign object</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get_All_Results([FromBody] Training_Campaign campaign)
        {
            int Id_User = Convert.ToInt32(HttpContext.Session.GetInt32("User_Id"));

            var presentations = dataAccess.GetAllResults(campaign.Id, campaign.Presentations[0].Id, Id_User);

            return Json(presentations);
        }
    }
}
