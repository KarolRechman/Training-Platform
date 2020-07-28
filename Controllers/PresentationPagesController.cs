using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz.Models;

namespace Quiz.Controllers
{
    /// <summary>
    /// Controller that's responsible for all "Presentation, test Pages"
    /// </summary>
    public class PresentationPagesController : Controller
    {
        private readonly DataAccess dataAccess = new DataAccess();
        private readonly Helper helper = new Helper();

        //public string GetDirectory()
        //{
        //    var config = configuration();
        //    string directory = config.GetSection("StaticFiles").GetSection("StaticFilesFolder").Value;

        //    return directory;
        //}

        //public IConfigurationRoot configuration()
        //{
        //    var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        //    return builder.Build();
        //}

        /// <summary>
        ///  Processes and sends user's aswers
        /// </summary>
        /// <param name="questions">Questions object, with aswers data</param>
        /// <param name="id">Indicates primary key in presentation table</param>
        /// <returns>Sum of user's points, redirects to trainings list</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit_Answers([Bind]List<Questions> questions, int id)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (HttpContext.Session.GetString("test") == null)
            {
                return RedirectToAction("Trainings_List", "UserPages");
            }
            else
            {
                ViewBag.Error = "Something is not valid !!";
                string points;
                string validation = "";
                if (questions[0].Time == "00:00:00")
                {
                    foreach (var question in questions)
                    {
                        if (question.Answers.TrueForAll(x => x.Correct_Answer.Equals(false)))
                        {
                            validation = "You didn't answer to all questions !!<br>";
                            //return View("Test_Page", questions);
                        }
                    }
                }
                else
                {
                    foreach (var question in questions)
                    {
                        if (question.Answers.TrueForAll(x => x.Correct_Answer.Equals(false)))
                        {
                            ViewBag.Valid = "Please answer to all questions !!";
                            return View("Test_Page", questions);
                        }
                    }
                }


                int? Id_User = HttpContext.Session.GetInt32("User_Id");

                //string time = questions[0].Time;
                //int Id_Campaign = Convert.ToInt32(HttpContext.Session.GetInt32("Id_Campaign"));
                points = dataAccess.SendAnswers(questions, Id_User, id);
                if (points != null)
                {
                    ViewBag.Points = points;
                    ViewBag.Error = validation + " Your points:";

                    foreach (var quest in questions)
                    {
                        foreach (var answer in quest.Answers)
                        {
                            answer.Correct_Answer = false;
                        }
                    }
                }
                ModelState.Clear();

                return View("Test_Page", questions);
            }
        }

        /// <summary>
        /// Calls Test page
        /// </summary>
        /// <param name="Id_Campaign">Indicates primary key in training campaign table</param>
        /// <param name="Id_Presentation">Indicates primary key in presentation table</param>
        /// <returns>Test page with questions/returns>
        public IActionResult Test_Page(int Id_Campaign, int Id_Presentation)
        {
            if (Id_Campaign == 0)
            {
                return RedirectToAction("Trainings_List", "UserPages");
            }
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (HttpContext.Session.GetString("test") == null)
            {
                return RedirectToAction("Trainings_List", "UserPages");
            }
            else
            {
                dataAccess.ReduceAttempt(HttpContext.Session.GetInt32("User_Id"), Id_Campaign, Id_Presentation);
                List<Questions> questions = new List<Questions>();
                questions = dataAccess.GetQuestions(Id_Presentation).ToList();
                if (questions.Count == 0)
                {
                    //int Training_Id = Convert.ToInt32(HttpContext.Session.GetInt32("Training_Id"));
                    //HttpContext.Session.Remove("Training_Id");
                    return RedirectToAction("Training_Page", "PresentationPages", new { Id_Campaign, Id_Presentation });
                }
                ModelState.Clear();

                var CheckShuffle_Questions = dataAccess.CheckShuffle_Questions(Id_Presentation);
                if (CheckShuffle_Questions == "True")
                {
                    questions.Shuffle();
                }

                foreach (var question in questions)
                {
                    question.Answers = dataAccess.GetAnswers(question.Id);
                    question.Id_Presentation = Id_Presentation;
                    question.Answers.Shuffle();
                }
                ViewBag.Presentation_Name = questions.FirstOrDefault().Presentation_Name;
                ViewBag.Id_Campaign = Id_Campaign;
                //HttpContext.Session.Remove("Campaign_Id");

                return View(questions);
            }
        }

        /// <summary>
        /// Calls training page
        /// </summary>
        /// <param name="Id_Campaign">Indicates primary key in training campaign table</param>
        /// <param name="Id_Presentation">Indicates primary key in presentation table</param>
        /// <returns>Training page with presentation</returns>
        public IActionResult Training_Page(int Id_Campaign, int Id_Presentation)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetString("test", "test");
                // HttpContext.Session.SetString("training", "training");
                HttpContext.Session.SetInt32("Id_Campaign", Id_Campaign);
                HttpContext.Session.SetInt32("Id_Presentation", Id_Presentation);

                ViewBag.Id_Campaign = Id_Campaign;

                var presentation = dataAccess.GetPresentationById(Id_Presentation);
                presentation.Id = Id_Presentation;
                presentation.Questions = dataAccess.GetQuestions(Id_Presentation);
                ViewBag.Presentation_Name = presentation.Name;

                if (presentation.URL == null || presentation.URL == "")
                {
                    HttpContext.Session.SetString("Error", "Sorry this presentation is no longer available !!!");
                    HttpContext.Session.SetString("Msg", "Please contact with your supervisor.");
                    return RedirectToAction("Campaigns_Training_List", "UserPages");
                }

                string url = "/Files/" + presentation.URL;

                var directoryFiles = Directory.GetFiles(Path.Combine(helper.GetDirectory(), presentation.URL));

                string domainName = HttpContext.Request.PathBase.ToString();

                if (presentation.Type.ToLower() == "pdf" || presentation.Type.ToLower() == "mp4")
                {
                    presentation.src = domainName + url + Path.GetFileName(directoryFiles.FirstOrDefault());
                }
                else
                {
                    long number = 0;

                    List<Files> files = new List<Files>();
                    foreach (string filePath in directoryFiles)
                    {
                        string fileName = Path.GetFileName(filePath);
                        number = Convert.ToInt64(System.Text.RegularExpressions.Regex.Match(fileName.Substring(fileName.IndexOf("_")), @"\d+").Value);
                        files.Add(new Files
                        {
                            number = number - 1,
                            Name = fileName.Split('.')[0].ToString(),
                            src = domainName + url + fileName.ToString(), // 4 iis express
                                                                          // src = "/Quiz/Test/PowerPoint" + fileName.ToString(), //4 iis, 
                        });
                    }
                    //create function !!!!!!!!!!!!!!!
                    if (files.First().number != 0)
                    {
                        for (var i = 0; i < files.Count; i++)
                        {
                            files[i].number = i;
                        }
                    }
                    files = files.OrderBy(x => x.number).ToList();
                    presentation.Files = files;
                }
                return View(presentation);
            }
        }
    }
}