using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz.Models;

namespace Quiz.Controllers
{
    public class AnalyticsPagesController : Controller
    {
        private readonly DataAccess dataAccess = new DataAccess();

        // GET: AnalyticsPages
        public async Task<ActionResult> Analytics_Dashboard()
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                List<Training_Campaign> campaigns = new List<Training_Campaign>();
                campaigns = await dataAccess.GetAllCampaigns();

                foreach (var camp in campaigns)
                {
                    camp.Presentations = await dataAccess.GetAllPresentations(camp.Id);
                }

                campaigns = campaigns.Where(c => !c.Presentations.Count.Equals(0)).ToList();
                return View(campaigns);
            }
        }

        [HttpGet]
        public async Task<JsonResult> Trainings_Select_Async(int id)
        {
            var pres = await dataAccess.GetAllPresentations(id);

            return Json(pres);
        }

        [HttpPost]
        public JsonResult Trainings_Chart([FromBody] Training_Campaign Training_Campaign)
        {
            if (Training_Campaign.Presentations.FirstOrDefault().Type == null || Training_Campaign.Presentations.FirstOrDefault().Type == "")
            {
                Training_Campaign.Presentations[0].Type = "1";
            }

            var result = dataAccess.GetAllResults(Training_Campaign.Id, Training_Campaign.Presentations.FirstOrDefault().Id, 0, Convert.ToInt32(Training_Campaign.Presentations.FirstOrDefault().Type));

            return Json(result);
        }

        [HttpPost]
        public JsonResult AllParticipants([FromBody] Training_Campaign Training_Campaign)
        {
            if (Training_Campaign.Id != 0 && Training_Campaign.Presentations.FirstOrDefault().Id != 0)
            {
                var result = dataAccess.GetAllParticipants(Training_Campaign.Id, Training_Campaign.Presentations.FirstOrDefault().Id);
                return Json(result);
            }
            else
            {
                return Json("");
            }           
        }

        //// GET: AnalyticsPages/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: AnalyticsPages/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: AnalyticsPages/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: AnalyticsPages/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: AnalyticsPages/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: AnalyticsPages/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: AnalyticsPages/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}