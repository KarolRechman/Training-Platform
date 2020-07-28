using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Quiz.Models;
using ReflectionIT.Mvc.Paging;

namespace Quiz.Controllers
{
    /// <summary>
    /// Controller that's responsible for all "Admin Pages"
    /// </summary>
    public class AdminPagesController : Controller
    {
        private readonly DataAccess dataAccess = new DataAccess();
        private readonly Helper helper = new Helper();
        private readonly Regex regex = new Regex(@"^[a-zA-Z0-9''-'\s]{1,100}$");

        public event EventHandler zxczxc;


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

        public async Task<IActionResult> Campaigns_List(string filter, int page = 1, string sortExpression = "Id")
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetInt32("last_Page", page);

                var training_Campaigns = new List<Training_Campaign>();
                training_Campaigns = await dataAccess.GetAllCampaigns();

                foreach (var campaign in training_Campaigns)
                {
                    campaign.Start_Date = campaign.Start_Date.Replace("00:00:00", "").TrimEnd();
                    campaign.End_Date = campaign.End_Date.Replace("23:59:59", "").TrimEnd();
                }
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    training_Campaigns = training_Campaigns.Where(c => c.Name.ToLower().Contains(filter.ToLower()) || c.Start_Date.ToLower().Contains(filter.ToLower())
                                                                    || c.End_Date.ToLower().Contains(filter.ToLower())).ToList();
                }
                var model = PagingList.Create(training_Campaigns, 4, page, sortExpression, "Id");

                model.RouteValue = new RouteValueDictionary {
                         { "filter", filter}
                    };
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit_Campaign(int id)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Training_Campaign campaign = new Training_Campaign();
                campaign = dataAccess.GetCampaignById(id);
                campaign.Presentations = await dataAccess.GetAllPresentations(id);
                campaign.Training_Groups = await dataAccess.GetAllGroups(id);
                campaign.Users_Assigned = dataAccess.GetAllAssignedUsers(0, id, true);

                //ViewBag.Available_Presentations = "";

                var Available_Presentations = await dataAccess.GetAllPresentations();
                Available_Presentations = Available_Presentations.Where(p => !campaign.Presentations.Any(cp => cp.Id == p.Id)).ToList();
                ViewData["Available_Presentations"] = new List<Presentations>();
                ViewData["Available_Presentations"] = Available_Presentations;

                var Available_Groups = await dataAccess.GetAllGroups();
                Available_Groups = Available_Groups.Where(g => !campaign.Training_Groups.Any(tg => tg.Id_Group == g.Id_Group)).ToList();

                ViewData["Available_Groups"] = new List<Training_Group>();
                ViewData["Available_Groups"] = Available_Groups;

                //ViewBag.Available_Groups = helper.Available_Groups(campaign.Training_Groups);

                //var Available_Users = dataAccess.GetAllUsers(campaign.Training_Groups);
                //Available_Users = Available_Users.Where(u => !campaign.Users_Assigned.Any(ua => ua.Id_User == u.Id_User)).ToList();

                ViewData["Available_Users"] = new List<User>();
                ViewData["Available_Users"] = helper.Available_Users(campaign.Users_Assigned, campaign.Training_Groups);


                //ViewBag.Available_Users = helper.Available_Users(campaign.Users_Assigned,campaign.Training_Groups);

                return View(campaign);
            }
        }

        [HttpGet]
        public async Task<JsonResult> Campaign_Details(int id)
        {
            Training_Campaign campaign = new Training_Campaign();
            campaign = dataAccess.GetCampaignById(id);
            campaign.Presentations = await dataAccess.GetAllPresentations(id);
            campaign.Training_Groups = await dataAccess.GetAllGroups(id);
            campaign.Users_Assigned = dataAccess.GetAllAssignedUsers(0, id, true);

            campaign.Users_Assigned = campaign.Users_Assigned.Where(u => u.Info_Group != "").ToList();

            //for (var i = 0; i < campaign.Users_Assigned.Count; i++)
            //{
            //    if (campaign.Users_Assigned[i].Info_Group == "")
            //    {
            //        campaign.Users_Assigned.RemoveAt(i);
            //        i--;
            //    }
            //}
            return Json(campaign);
        }

        [HttpGet]
        public JsonResult Delete_Group(int id)
        {
            Training_Group group = new Training_Group();
            group = dataAccess.GetGroupById(id);
            group.Users = dataAccess.GetAllAssignedUsers(id);
            return Json(group);
        }

        public IActionResult Delete_Group_Confirmed(int id)
        {
            int page = Convert.ToInt32(HttpContext.Session.GetInt32("last_Page"));

            dataAccess.DeleteGroup(id);

            HttpContext.Session.Remove("last_Page");

            return RedirectToAction("Groups_List", "AdminPages", new { page });
        }

        public async Task<IActionResult> Groups_List(string filter, int page = 1, string sortExpression = "Id_Group")
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetInt32("last_Page", page);

                var groups = new List<Training_Group>();
                groups = await dataAccess.GetAllGroups();
                foreach (var gr in groups)
                {
                    gr.Users = dataAccess.GetAllAssignedUsers(gr.Id_Group);
                }

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    groups = groups.Where(g => g.Name.ToLower().Contains(filter.ToLower())).ToList();
                }
                var model = PagingList.Create(groups, 5, page, sortExpression, "Id_Group");

                model.RouteValue = new RouteValueDictionary {
                         { "filter", filter}
                    };
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit_Group(int? id)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }

                Training_Group group = new Training_Group();
                group = dataAccess.GetGroupById(id);

                return View(group);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Group([Bind] Training_Group group)
        {
            Training_Group old_training_Group = new Training_Group();
            old_training_Group = dataAccess.GetGroupById(group.Id_Group);
            string status = "";

            if (old_training_Group.Name == group.Name)
            {
                return View(group);
            }
            else
            {
                status = dataAccess.UpdateGroupName(group);
                if (status.Contains("exists") == true)
                {
                    ViewBag.Msg = "Something went wrong !!";
                    ViewBag.Errors = status;
                }
            }
            return View(group);
        }

        [HttpGet]
        public async Task<IActionResult> Create_Training_Campaign()
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Training_Campaign Training_Campaign = new Training_Campaign();
                Training_Campaign.Presentations = await dataAccess.GetAllPresentations();
                Training_Campaign.Training_Groups = await dataAccess.GetAllGroups();
                return View(Training_Campaign);
            }
        }

        [HttpPost]
        public async Task<JsonResult> Create_Training_Campaign([FromBody] Training_Campaign Training_Campaign)
        {
            string status = "";
            Training_Campaign.GetDates();
            ////4 UPDATE CAMP
            //if (Training_Campaign.Id != 0)
            //{


            //    Training_Campaign old_training_Campaign = new Training_Campaign();

            //    old_training_Campaign = dataAccess.GetCampaignById(Training_Campaign.Id);
            //    old_training_Campaign.Presentations = await dataAccess.GetAllPresentations(Training_Campaign.Id);
            //    old_training_Campaign.Training_Groups = await dataAccess.GetAllGroups(Training_Campaign.Id);
            //    //old_training_Campaign.Users_Assigned = dataAccess.GetAllAssignedUsers(0, Training_Campaign.Id, true);

            //    var objectsThatMatch_Pres = Training_Campaign.Presentations.Where(tc => old_training_Campaign.Presentations.Any(old => old.Id.Equals(tc.Id))).ToList();
            //    var objectsThatMatch_Group = Training_Campaign.Training_Groups.Where(tc => old_training_Campaign.Training_Groups.Any(old => old.Id_Group.Equals(tc.Id_Group))).ToList();

            //    if (old_training_Campaign.Name == Training_Campaign.Name && objectsThatMatch_Pres.Count == old_training_Campaign.Presentations.Count
            //        && Training_Campaign.Presentations.Count == old_training_Campaign.Presentations.Count
            //        && objectsThatMatch_Group.Count == old_training_Campaign.Training_Groups.Count && Training_Campaign.Training_Groups.Count == old_training_Campaign.Training_Groups.Count
            //        && Training_Campaign.Start_Date == old_training_Campaign.Start_Date.Substring(0, old_training_Campaign.Start_Date.IndexOf(" "))
            //        && Training_Campaign.End_Date == old_training_Campaign.End_Date.Substring(0, old_training_Campaign.Start_Date.IndexOf(" "))
            //        && Training_Campaign.Attempts == old_training_Campaign.Attempts)
            //    {
            //        status = "Campaign data didn't change. Please make some changes or return to list !!";
            //        return Json(status);
            //    }
            //    else
            //    {
            //        status = dataAccess.AddNewCampaign(Training_Campaign);
            //        return Json(status);
            //    }
            //}

            if (Training_Campaign.Name == "" || Training_Campaign.Name == null || (regex.IsMatch(Training_Campaign.Name) == false))
            {
                status = "Please write correct name !!";
                return Json(status);
            }
            else if (Training_Campaign.Presentations.Count == 0)
            {
                status = "Please choose a training !!";
                return Json(status);
            }
            else if (Training_Campaign.Start_Date == Training_Campaign.End_Date)
            {
                status = "Please choose dates different then default and more than one day !!";
                return Json(status);
            }
            else if (Training_Campaign.Training_Groups.Count == 0 && Training_Campaign.Users_Assigned.Count == 0)
            {
                status = "You didn't choose anyone for this event !!";
                return Json(status);
            }
            else if (Training_Campaign.Attempts <= 0 || Training_Campaign.Attempts > 99)
            {
                status = "Please choose correct attempts value !!";
                return Json(status);
            }
            else
            {
                if (Training_Campaign.Id != 0)
                {
                    Training_Campaign old_training_Campaign = new Training_Campaign();

                    old_training_Campaign = dataAccess.GetCampaignById(Training_Campaign.Id);
                    old_training_Campaign.Presentations = await dataAccess.GetAllPresentations(Training_Campaign.Id);
                    old_training_Campaign.Training_Groups = await dataAccess.GetAllGroups(Training_Campaign.Id);
                    //old_training_Campaign.Users_Assigned = dataAccess.GetAllAssignedUsers(0, Training_Campaign.Id, true);

                    var objectsThatMatch_Pres = Training_Campaign.Presentations.Where(tc => old_training_Campaign.Presentations.Any(old => old.Id.Equals(tc.Id))).ToList();
                    var objectsThatMatch_Group = Training_Campaign.Training_Groups.Where(tc => old_training_Campaign.Training_Groups.Any(old => old.Id_Group.Equals(tc.Id_Group))).ToList();

                    if (old_training_Campaign.Name == Training_Campaign.Name && objectsThatMatch_Pres.Count == old_training_Campaign.Presentations.Count
                        && Training_Campaign.Presentations.Count == old_training_Campaign.Presentations.Count
                        && objectsThatMatch_Group.Count == old_training_Campaign.Training_Groups.Count && Training_Campaign.Training_Groups.Count == old_training_Campaign.Training_Groups.Count
                        && Training_Campaign.Start_Date == old_training_Campaign.Start_Date.Substring(0, old_training_Campaign.Start_Date.IndexOf(" "))
                        && Training_Campaign.End_Date == old_training_Campaign.End_Date.Substring(0, old_training_Campaign.Start_Date.IndexOf(" "))
                        && Training_Campaign.Attempts == old_training_Campaign.Attempts)
                    {
                        status = "Campaign data didn't change. Please make some changes or return to list !!";
                        return Json(status);
                    }
                    else
                    {
                        status = dataAccess.AddNewCampaign(Training_Campaign);
                        return Json(status);
                    }
                }
                else
                {
                    Training_Campaign.Author_Id = Convert.ToInt32(HttpContext.Session.GetInt32("User_Id"));
                    status = dataAccess.AddNewCampaign(Training_Campaign);

                    return Json(status);
                }
            }
        }

        public IActionResult Campaign_DeleteConfirmed(int id)
        {
            int page = Convert.ToInt32(HttpContext.Session.GetInt32("last_Page"));

            dataAccess.DeleteCampaign(id);

            HttpContext.Session.Remove("last_Page");

            return RedirectToAction("Campaigns_List", "AdminPages", new { page });
        }

        [HttpGet]
        public async Task<JsonResult> Get_For_Delete_Campaign(int id)
        {
            Training_Campaign campaign = new Training_Campaign();
            campaign = dataAccess.GetCampaignById(id);
            campaign.Presentations = await dataAccess.GetAllPresentations(id);
            campaign.Users_Assigned = dataAccess.GetAllAssignedUsers(0, id, false);

            campaign.Users_Assigned = campaign.Users_Assigned.Where(u => u.Info_Group != "").ToList();

            //for (var i = 0; i < campaign.Users_Assigned.Count; i++)
            //{
            //    if (campaign.Users_Assigned[i].Info_Group == "")
            //    {
            //        campaign.Users_Assigned.RemoveAt(i);
            //        i--;
            //    }
            //}

            return Json(campaign);
        }

        [HttpPost]
        public void Add_New_Employee_For_Campaign([FromBody] Training_Campaign campaign)
        {
            dataAccess.AddNewEmployeeForCampaign(campaign);
        }

        [HttpPost]
        public void Add_New_Employees([FromBody] Training_Group group)
        {
            dataAccess.AddNewEmployee(group);
        }

        [HttpPost]
        public void Delete_Employee([FromBody] Training_Group group)
        {
            dataAccess.DeleteEmployee(group);
        }

        [HttpPost]
        public void Delete_Employee_From_Campaign([FromBody] Training_Campaign campaign)
        {
            dataAccess.DeleteEmployeeFromCampaign(campaign);
        }

        [HttpGet]
        public JsonResult Assigned_Employees_Partial(int id)
        {
            List<User> users = new List<User>();
            users = dataAccess.GetAllAssignedUsers(id);

            return Json(users);
        }

        [HttpGet]
        public JsonResult Additional_Employees_Partial(int id)
        {
            List<User> users = new List<User>();
            users = dataAccess.GetAllAssignedUsers(0, id, false);

            var plus_Additional = users.Where(u => u.Info_Group == "").ToList();

            for (var i = 0; i < users.Count; i++)
            {
                if (users[i].Info_Group != "Additional")
                {
                    users[i].Groups_In = users[i].GetGroups();
                }
            }

            foreach (var user in users.Where(u => plus_Additional.Any(pa => pa.Id_User.Equals(u.Id_User))))
            {
                Training_Group group = new Training_Group() { Id_Group = 0, Name = "Additional" };
                user.Groups_In.Add(group);
            }

            users = users.Where(u => u.Info_Group != "").ToList();
            return Json(users);
        }

        public async Task<IActionResult> Index(string filter, int page = 1, string sortExpression = "Id")
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetInt32("last_Page", page);

                var presentations = new List<Presentations>();
                presentations = await dataAccess.GetAllPresentations();

                //helper.CheckLostPresentationsPP(presentations);
                //helper.CheckLostPresentationsPDF(presentations);
                //helper.CheckLostPresentationsVideo(presentations);

                presentations = await dataAccess.GetAllPresentations();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    presentations = presentations.Where(p => p.Name.ToLower().Contains(filter.ToLower()) || p.Type_Name.ToLower().Contains(filter.ToLower())).ToList();
                }

                var model = PagingList.Create(presentations, 5, page, sortExpression, "Id");

                model.RouteValue = new RouteValueDictionary {
                         { "filter", filter}
            };
                if (HttpContext.Session.GetString("Error") != null)
                {
                    ViewBag.DeleteError = HttpContext.Session.GetString("Error");
                    ViewBag.DeleteMsg = "So it'll be deleted anyway from database";
                    HttpContext.Session.Remove("Error");
                    return View(model);
                }
                if (HttpContext.Session.GetString("Exception") != null)
                {
                    ViewBag.DeleteError = "Something went wrong !!";
                    ViewBag.DeleteMsg = HttpContext.Session.GetString("Exception").Substring(0, 200) + "...................     Try one more time, or contact your support.";
                    HttpContext.Session.Remove("Exception");
                    return View(model);
                }
                return View(model);
            }
        }
        //try optimize 2 in 1 !!!!!!!!!!!!!!!!!!!!
        //public IActionResult All_Files_DeleteConfirmed(string id) //id here is a file name
        //{
        //    Presentations presentations = new Presentations();

        //    presentations = dataAccess.GetPresentationByName(id);
        //    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(GetDirectory(), presentations.URL));

        //    if (Directory.Exists(directoryInfo.ToString()))
        //    {
        //        foreach (FileInfo file in directoryInfo.GetFiles())
        //        {
        //            try
        //            {
        //                file.Delete();
        //            }
        //            catch (Exception e)
        //            {
        //                if (e.ToString().Length > 0)
        //                {
        //                    HttpContext.Session.SetString("File_Delete_Exception", e.ToString());
        //                    return RedirectToAction("Edit_Training", new { id = presentations.Id });
        //                }
        //            }
        //        }
        //    }
        //    return RedirectToAction("Edit_Training", new { id = presentations.Id });
        //}
        [HttpGet]
        public IActionResult File_DeleteConfirmed2([FromQuery]string Name, int Id, string All) //id here is a file name
        {
            Presentations presentations = new Presentations();
            //string Training_Name = Name.Substring(0, Name.IndexOf("_"));

            presentations = dataAccess.GetPresentationById(Id);

            if (All == null)
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(helper.GetDirectory(), presentations.URL.ToString(), Name + "." + presentations.Type));

                try
                {
                    fileInfo.Delete();
                }
                catch (Exception e)
                {
                    if (e.ToString().Length > 0)
                    {
                        HttpContext.Session.SetString("File_Delete_Exception", e.ToString());
                        return RedirectToAction("Edit_Training", new { id = presentations.Id });
                    }
                }
                return RedirectToAction("Edit_Training", new { id = presentations.Id });
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(helper.GetDirectory(), presentations.URL));

                if (Directory.Exists(directoryInfo.ToString()))
                {
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            if (e.ToString().Length > 0)
                            {
                                HttpContext.Session.SetString("File_Delete_Exception", e.ToString());
                                return RedirectToAction("Edit_Training", new { id = presentations.Id });
                            }
                        }
                    }
                }
                return RedirectToAction("Edit_Training", new { id = presentations.Id });
            }
        }

        //public IActionResult File_DeleteConfirmed(string id) //id here is a file name
        //{
        //    Presentations presentations = new Presentations();
        //    string Training_Name = id.Substring(0, id.IndexOf("_"));

        //    presentations = dataAccess.GetPresentationByName(Training_Name);

        //    FileInfo fileInfo = new FileInfo(Path.Combine(GetDirectory(), presentations.URL.ToString(), id + "." + presentations.Type));

        //    try
        //    {
        //        fileInfo.Delete();
        //    }
        //    catch (Exception e)
        //    {
        //        if (e.ToString().Length > 0)
        //        {
        //            HttpContext.Session.SetString("File_Delete_Exception", e.ToString());
        //            return RedirectToAction("Edit_Training", new { id = presentations.Id });
        //        }
        //    }
        //    return RedirectToAction("Edit_Training", new { id = presentations.Id });
        //}

        [HttpGet]
        public IActionResult Edit_Training(int id)
        {
            string dir = helper.GetDirectory();

            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.Clear();
                if (id == 0)
                {
                    return NotFound();
                }
                if (HttpContext.Session.GetString("Success") != null)
                {
                    ViewBag.Error = HttpContext.Session.GetString("Success");
                    HttpContext.Session.Remove("Success");
                }
                else
                {
                    ViewBag.Error = "Something went wrong !!";
                }

                if (HttpContext.Session.GetString("File_Delete_Exception") != null)
                {
                    ViewBag.ErrorMsg = HttpContext.Session.GetString("File_Delete_Exception").Substring(0, 200) + "...................     Try one more time, or contact your support.";
                    HttpContext.Session.Remove("File_Delete_Exception");
                }
                if (HttpContext.Session.GetString("ErrorMsg") != null)
                {
                    ViewBag.ErrorMsg = HttpContext.Session.GetString("ErrorMsg");
                    HttpContext.Session.Remove("ErrorMsg");
                }

                Presentations presentations = new Presentations();
                presentations = dataAccess.GetPresentationById(id);
                presentations.Format_Types = dataAccess.GetFormatTypes();

                string url = "/Files/" + presentations.URL;

                var directoryFiles = Directory.GetFiles(Path.Combine(dir, presentations.URL));

                string domainName = HttpContext.Request.PathBase.ToString();

                int number = 0;

                List<Files> files = new List<Files>();
                foreach (string filePath in directoryFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    number = Convert.ToInt16(System.Text.RegularExpressions.Regex.Match(fileName.Replace(presentations.Name, ""), @"\d+").Value);

                    files.Add(new Files
                    {
                        number = number,
                        Name = fileName.Split('.')[0].ToString(),
                        src = domainName + url + fileName.ToString(), // 4 iis express
                                                                      // src = "/Quiz/Test/PowerPoint" + fileName.ToString(), //4 iis, 
                    }); ;
                }
                files = files.OrderBy(x => x.number).ToList();
                presentations.Files = files;

                if (files.Count > 0)
                {
                    HttpContext.Session.SetString("Last_Number", files.Last().number.ToString());
                }
                else
                {
                    HttpContext.Session.SetString("Last_Number", "0");
                }
                return View(presentations);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken, DisableRequestSizeLimit]
        public IActionResult Edit_Training([Bind] Presentations presentations)
        {
            presentations.Author_Id = Convert.ToInt32(HttpContext.Session.GetInt32("User_Id"));
            //string[] Data_Range = presentations.Date_Range.Split("-");
            //string start = Data_Range[0].TrimEnd();
            //string end = Data_Range[1].Trim();
            /////////////////////////////////////////////////////////////////////////////////////////////////
            Presentations old_Presentation = new Presentations();
            old_Presentation = dataAccess.GetPresentationById(presentations.Id);

            //old_Presentation.Start_Date = old_Presentation.Start_Date.Replace("00:00:00", "").TrimEnd();
            //old_Presentation.End_Date = old_Presentation.End_Date.Replace("00:00:00", "").TrimEnd();
            // old_Presentation.End_Date = old_Presentation.End_Date.Replace("23:59:59", "").TrimEnd();
            //////////////////////////////////////////////////////////////////////////
            bool status = false;
            string update_Status = null;

            string dir = helper.GetDirectory();

            if (presentations.FormFile == null && presentations.Name == old_Presentation.Name && presentations.Type == null)
            {
                return RedirectToAction("Edit_Training", new { id = presentations.Id });
            }
            if (presentations.Type == null)
            {
                presentations.Type = old_Presentation.Type;
            }

            presentations.Type_Name = dataAccess.GetNameByType(presentations.Type);
            int number = Convert.ToInt32(HttpContext.Session.GetString("Last_Number"));

            if (presentations.FormFile != null && (presentations.Type.ToLower() != "jpg" || presentations.Type == null) && number != 0)
            {
                HttpContext.Session.SetString("ErrorMsg", "You can have only one file for this format !!");
                return RedirectToAction("Edit_Training", new { id = presentations.Id });
            }
            else
            {
                status = dataAccess.CheckName(presentations.Name, presentations.Id);
                if (status == false)
                {
                    HttpContext.Session.SetString("ErrorMsg", "Training with this name already exists, please choose different name !!");
                    return RedirectToAction("Edit_Training", new { id = presentations.Id });
                }
                else
                {
                    if (presentations.FormFile != null)
                    {
                        status = presentations.FormFile.TrueForAll(x => x.FileName.ToString().ToUpper().Contains(presentations.Type.ToUpper()) == true);

                        if (status != true)
                        {
                            HttpContext.Session.SetString("ErrorMsg", "Files type is incorrect, maybe not all files are the same or You've choosen wrong training type, please check all possibilities one more time !!");
                            return RedirectToAction("Edit_Training", new { id = presentations.Id });
                        }
                        else
                        {
                            var directoryFiles = Directory.CreateDirectory(Path.Combine(dir, presentations.Type_Name, presentations.Name));

                            presentations.URL = Path.Combine(presentations.Type_Name, presentations.Name) + "\\";
                            update_Status = dataAccess.UpdatePresentation(presentations);
                            HttpContext.Session.SetString("ErrorMsg", update_Status);

                            if (update_Status.Contains("please") == true)
                            {
                                return RedirectToAction("Edit_Training", new { id = presentations.Id });
                            }
                            else
                            {
                                DirectoryInfo old_Directory = new DirectoryInfo(Path.Combine(dir, old_Presentation.URL));

                                if (old_Directory.GetFiles().Length == 0 && Directory.Exists(old_Directory.ToString()) == true)
                                {
                                    if (((directoryFiles.FullName == old_Directory.FullName.Substring(0, old_Directory.FullName.LastIndexOf("\\"))) && presentations.FormFile.Count > 0) != true)
                                    {
                                        old_Directory.Delete();
                                    }
                                }

                                if (presentations.New_File_Name == null)
                                {
                                    number = Convert.ToInt32(HttpContext.Session.GetString("Last_Number")) + 1;
                                    HttpContext.Session.Remove("Last_Number");

                                    foreach (IFormFile formFile in presentations.FormFile)
                                    {
                                        string Save_Name = presentations.Name + "_" + number.ToString() + "." + presentations.Type;

                                        string File_Path = Path.Combine(directoryFiles.ToString(), Save_Name);

                                        FileStream fileStream = new FileStream(File_Path, FileMode.Create);

                                        formFile.CopyTo(fileStream);
                                        number += 1;
                                        fileStream.Dispose();
                                    }
                                    HttpContext.Session.SetString("Success", "Well done !!");
                                    return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                }
                                else
                                {
                                    if (presentations.New_File_Name.EndsWith(",") == true)
                                    {
                                        presentations.New_File_Name = presentations.New_File_Name.Replace(",", " ").TrimEnd(' ');
                                    }
                                    string[] File_numbers = System.Text.RegularExpressions.Regex.Split(presentations.New_File_Name, @"\D+");

                                    if (presentations.FormFile.Count != File_numbers.Length)
                                    {
                                        HttpContext.Session.SetString("ErrorMsg", "The quantity of files and numbers is not equal, please count all again !!");
                                        return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                    }

                                    var Invalid_Numbers = new List<string>();
                                    foreach (var file in old_Directory.GetFiles())
                                    {
                                        string file_Number;
                                        file_Number = Convert.ToInt16(System.Text.RegularExpressions.Regex.Match(file.Name, @"\d+").Value).ToString();
                                        Invalid_Numbers.Add(file_Number);
                                    }

                                    Invalid_Numbers = Invalid_Numbers.Where(i_N => File_numbers.Any(f_N => f_N.Equals(i_N))).ToList();
                                    //Where(p => !campaign.Presentations.Any(cp => cp.Id == p.Id)).ToList();

                                    if (Invalid_Numbers.Count == 1)
                                    {
                                        HttpContext.Session.SetString("ErrorMsg", "This number:  --> " + Invalid_Numbers[0].ToUpper() + " <--  is already used in this training, please choose correct one !!");
                                        return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                    }
                                    else if (Invalid_Numbers.Count > 1)
                                    {
                                        string numbers = "";
                                        foreach (var num in Invalid_Numbers)
                                        {
                                            numbers += num + ", ";
                                        }
                                        numbers.Replace(", ", "");
                                        HttpContext.Session.SetString("ErrorMsg", "Those numberes:  --> " + numbers.ToUpper() + " <--  are already used in this training, please choose correct ones !!");
                                        return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                    }
                                    else
                                    {
                                        foreach (string value in File_numbers)
                                        {
                                            if (!string.IsNullOrEmpty(value))
                                            {
                                                //int i = int.Parse(value);
                                                string file_Name = presentations.Name + "_" + value;
                                                string Save_Name = file_Name + "." + presentations.Type;

                                                //foreach (var file in old_Directory.GetFiles())
                                                //{
                                                //    if (file.Name.Equals(Save_Name) == true)
                                                //    {
                                                //        HttpContext.Session.SetString("ErrorMsg", "This number:  --> " + value.ToUpper() + " <--  is already used in this training, please choose correct one !!");
                                                //        return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                                //    }
                                                //}

                                                foreach (IFormFile formFile in presentations.FormFile)
                                                {
                                                    string File_Path = Path.Combine(directoryFiles.ToString(), Save_Name);

                                                    FileStream fileStream = new FileStream(File_Path, FileMode.Create);

                                                    formFile.CopyTo(fileStream);

                                                    fileStream.Dispose();
                                                }
                                            }
                                        }
                                        HttpContext.Session.SetString("Success", "Well done !!");
                                        return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var sourceDirectory = Path.Combine(dir, old_Presentation.URL.TrimEnd('\\'));
                        var destinationDirectory = Path.Combine(dir, presentations.Type_Name, presentations.Name);

                        presentations.URL = Path.Combine(presentations.Type_Name, presentations.Name) + "\\";

                        if (sourceDirectory == destinationDirectory)
                        {
                            update_Status = dataAccess.UpdatePresentation(presentations);

                            HttpContext.Session.SetString("Success", "Well done !!");
                            HttpContext.Session.SetString("ErrorMsg", update_Status);

                            return RedirectToAction("Edit_Training", new { id = presentations.Id });
                        }
                        else
                        {
                            try
                            {
                                Directory.Move(sourceDirectory, destinationDirectory);
                            }
                            catch (Exception e)
                            {
                                if (e.ToString().Length > 0)
                                {
                                    HttpContext.Session.SetString("File_Delete_Exception", e.ToString());
                                    return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                }
                            }
                            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(dir, presentations.URL));

                            try
                            {
                                List<FileInfo> files = directoryInfo.GetFiles().ToList();

                                if (files.Count == 1)
                                {
                                    files[0].MoveTo(Path.Combine(dir, presentations.URL) + presentations.Name + "_1." + presentations.Type);
                                }
                                else
                                {
                                    foreach (FileInfo file in files)
                                    {
                                        int File_number = Convert.ToInt16(System.Text.RegularExpressions.Regex.Match(file.Name.Substring(file.Name.IndexOf("_")), @"\d+").Value);

                                        file.MoveTo(Path.Combine(dir, presentations.URL) + presentations.Name + "_" + File_number.ToString() + "." + presentations.Type);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (e.ToString().Length > 0)
                                {
                                    HttpContext.Session.SetString("File_Delete_Exception", e.ToString());
                                    return RedirectToAction("Edit_Training", new { id = presentations.Id });
                                }
                            }
                            update_Status = dataAccess.UpdatePresentation(presentations);

                            HttpContext.Session.SetString("ErrorMsg", update_Status);

                            if (update_Status.Contains("please") == true)
                            {
                                return RedirectToAction("Edit_Training", new { id = presentations.Id });
                            }
                            else
                            {
                                HttpContext.Session.SetString("Success", "Well done !!");
                                return RedirectToAction("Edit_Training", new { id = presentations.Id });
                            }
                        }
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult Edit_Question(int? id)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                ViewBag.Id = id;

                Questions questions = new Questions();
                questions = dataAccess.GetQuestionById(id);
                questions.Answers = dataAccess.GetAnswers(id);

                return View(questions);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Question([Bind] Questions questions)
        {
            ViewBag.Error = "Something went wrong !!";

            if (questions.Answers.TrueForAll(x => x.Correct_Answer.Equals(false)) == true)
            {
                ViewBag.ErrorMsg = "Please choose at least one correct answer !!!";
            }
            else
            {
                if (questions.Question == null)
                {
                    ViewBag.ErrorMsg = "Please write a question !!!";
                }
                else
                {
                    if (questions.Answers.TrueForAll(x => x.Answer.Length == 0) == true)
                    {
                        ViewBag.ErrorMsg = "Please write some answers !!!";
                    }
                    else
                    {
                        for (var i = 0; i < questions.Answers.Count; i++)
                        {
                            if (questions.Answers[i].Answer == null)
                            {
                                questions.Answers[i].Correct_Answer = false;
                            }
                        }
                        string status = dataAccess.UpdateQuestion(questions);

                        return RedirectToAction("Questions_List", new { id = questions.Id_Presentation });
                    }
                }
            }
            return View(questions);
        }

        public IActionResult Questions_List(int id)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (id == 0)
                {
                    return NotFound();
                }
                List<Questions> questions = new List<Questions>();
                questions = dataAccess.GetQuestions(id).ToList();

                ViewBag.Shuffle_Questions = dataAccess.CheckShuffle_Questions(id);
                ViewBag.Points_Required = dataAccess.CheckPoints_Required(id);

                ViewBag.Id = id;

                if (questions.Count == 0)
                {
                    Presentations presentations = new Presentations();
                    presentations = dataAccess.GetPresentationById(id);
                    presentations.Points_Required = 0;
                    dataAccess.SetRequirements(presentations);
                    ViewBag.Presentation_Name = presentations.Name;
                    return View(questions);
                }
                else
                {
                    ViewBag.Presentation_Name = questions[0].Presentation_Name;
                }
                return View(questions);
            }
        }

        [HttpGet]
        public JsonResult Delete_PresentationAJAX(int id)
        {
            Presentations presentations = dataAccess.GetPresentationById(id);
            return Json(presentations);
        }

        [HttpGet]
        public JsonResult Delete_QuestionAJAX(int? id)
        {
            Questions questions = dataAccess.GetQuestionById(id);
            return Json(questions);
        }

        public IActionResult Question_DeleteConfirmed(int? id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                Questions questions = dataAccess.GetQuestionById(id);
                string result = dataAccess.DeleteQuestion(id);

                return RedirectToAction("Questions_List", new { id = questions.Id_Presentation });
            }
        }

        [HttpGet]
        public JsonResult GetAnswers(int? id)
        {
            List<Answers> answers = new List<Answers>();
            answers = dataAccess.GetAnswers(id);
            return Json(answers);
        }

        public IActionResult DeleteConfirmed(int id)
        {
            int page = Convert.ToInt32(HttpContext.Session.GetInt32("last_Page"));          

            HttpContext.Session.Remove("last_Page");

            if (id == 0)
            {
                return RedirectToAction("Index", "AdminPages", new { page });
            }
            else
            {
                Presentations presentations = dataAccess.GetPresentationById(id);
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(helper.GetDirectory(), presentations.URL));

                if (Directory.Exists(directoryInfo.ToString()))
                {
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            if (e.ToString().Length > 0)
                            {
                                HttpContext.Session.SetString("Exception", e.ToString());
                                return RedirectToAction("Index", "AdminPages", new { page });
                            }
                        }
                    }
                    try
                    {
                        Directory.Delete(directoryInfo.ToString(), true);
                    }
                    catch (Exception e)
                    {
                        if (e.ToString().Length > 0)
                        {
                            HttpContext.Session.SetString("Exception", e.ToString());
                            return RedirectToAction("Index", "AdminPages", new { page });
                        }
                    }

                    string result = dataAccess.DeletePresentation(id);

                    if (result.Contains("deleted"))
                    {
                        return RedirectToAction("Index", "AdminPages", new { page });
                    }
                    else
                    {
                        HttpContext.Session.SetString("Error", result);
                    }
                }
                else
                {
                    HttpContext.Session.SetString("Error", "Directory don't exists on drive !!!");
                    string result2 = dataAccess.DeletePresentation(id);
                    return RedirectToAction("Index", "AdminPages", new { page });
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult GetPartial2([FromBody] Training_Campaign training_Campaign)
        {
            List<User> users = new List<User>();
            users = dataAccess.GetAllUsers(training_Campaign.Training_Groups, training_Campaign.Id);


            //List<User> AssignedUsers = new List<User>();
            //AssignedUsers = dataAccess.GetAllAssignedUsers(0, training_Campaign.Id, false);

            return Json(users);
        }
        public IActionResult GetPartial()
        {
            return PartialView("_Partial");
        }
        [HttpGet]
        public IActionResult Create_Group()
        {
            ModelState.Clear();
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult Create_Group([FromBody] Training_Group training_Group)
        {
            string status = "";

            if (training_Group.Name == "" || training_Group.Name == null || (regex.IsMatch(training_Group.Name) == false))
            {
                status = "Please write correct name !!";
            }
            else
            {
                status = dataAccess.AddGroup(training_Group);
            }
            return Json(status);
        }


        [HttpGet]
        public IActionResult Create_Training()
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Presentations presentations = new Presentations();
                presentations.Format_Types = dataAccess.GetFormatTypes();
                //presentations.Users_Assigned = dataAccess.GetAllUsers(Convert.ToInt32(HttpContext.Session.GetInt32("User_Id")));
                return View(presentations);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken, DisableRequestSizeLimit]
        public IActionResult Create_Training([Bind] Presentations presentations)
        {
            string dir = helper.GetDirectory();
            presentations.Author_Id = Convert.ToInt32(HttpContext.Session.GetInt32("User_Id"));
            presentations.Author_Mandant = Convert.ToInt32(HttpContext.Session.GetInt32("Id_Mandant"));

            ModelState.Clear();
            if (ModelState.IsValid)
            {
                ViewBag.Msg = "Something went wrong !!";

                if (presentations.Name == null || presentations.Name == "")
                {
                    ViewBag.Errors = "Please write name !!";
                    presentations.Format_Types = dataAccess.GetFormatTypes();
                    return View(presentations);
                }
                else if (presentations.Name.Contains("/") || presentations.Name.Contains('\\') || presentations.Name.Contains("."))
                {
                    ViewBag.Errors = "Please write name only with normal letters and numbers !!";
                    presentations.Format_Types = dataAccess.GetFormatTypes();
                    return View(presentations);
                }
                else
                {
                    if (presentations.FormFile == null || presentations.FormFile.Count < 1)
                    {
                        ViewBag.Errors = "Please choose file to upload !!";
                        presentations.Format_Types = dataAccess.GetFormatTypes();
                        return View(presentations);
                    }
                    else
                    {
                        if (presentations.Type == "" || presentations.Type == null)
                        {
                            ViewBag.Errors = "Please choose file type !!";
                            presentations.Format_Types = dataAccess.GetFormatTypes();
                            return View(presentations);

                        }
                        else if ((presentations.Type == "mp4" || presentations.Type == "pdf") && (presentations.FormFile.Count > 1))
                        {
                            ViewBag.Errors = "Please upload only one file of that kind.";
                            presentations.Format_Types = dataAccess.GetFormatTypes();
                            return View(presentations);
                        }
                        else
                        {
                            string File_Type_Directory = null;

                            bool result_Validation_Type = presentations.FormFile.TrueForAll(x => x.FileName.ToString().ToUpper().Contains(presentations.Type.ToUpper()) == true);

                            if (result_Validation_Type != true)
                            {

                                ViewBag.Errors = "Files type is incorrect, maybe not all files are the same or You've choosen wrong training type, Please check all possibilities one more time !!";
                                presentations.Format_Types = dataAccess.GetFormatTypes();
                                //ModelState.Clear();
                                return View(presentations);
                            }
                            else
                            {
                                File_Type_Directory = dataAccess.GetNameByType(presentations.Type);
                                var directoryFiles = Directory.CreateDirectory(Path.Combine(dir, File_Type_Directory, presentations.Name));

                                presentations.URL = Path.Combine(File_Type_Directory, presentations.Name) + "\\";
                                presentations.Type_Name = File_Type_Directory;
                                string status = dataAccess.AddPresentation(presentations);

                                if (status.Contains("exists"))
                                {
                                    ViewBag.Msg = "Try one more time...";
                                    ViewBag.Errors = status;

                                    presentations.Format_Types = dataAccess.GetFormatTypes();
                                    ModelState.Clear();
                                    return View(presentations);
                                }
                                else
                                {
                                    int number = 1;
                                    foreach (IFormFile formFile in presentations.FormFile)
                                    {
                                        string Save_Name = presentations.Name + "_" + number.ToString() + "." + presentations.Type;

                                        string File_Path = Path.Combine(directoryFiles.ToString(), Save_Name);

                                        FileStream fileStream = new FileStream(File_Path, FileMode.Create);

                                        formFile.CopyTo(fileStream);
                                        number += 1;
                                        fileStream.Dispose();
                                    }
                                    ViewBag.Msg = "Well done !!";
                                    ViewBag.Errors = status;
                                    presentations.Format_Types = dataAccess.GetFormatTypes();
                                    ModelState.Clear();
                                    return View(presentations);
                                }
                            }
                        }
                    }
                }
            }
            presentations.Format_Types = dataAccess.GetFormatTypes();
            ModelState.Clear();
            return View(presentations);
        }

        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<Questions> questions = new List<Questions>();
            questions = dataAccess.GetQuestions(id).ToList();

            if (questions == null)
            {
                return NotFound();
            }
            return View(questions);
        }

        [HttpGet]
        public IActionResult Create_Question(int id)
        {
            if (HttpContext.Session.GetInt32("User_Id") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    ViewBag.Id = id;
                    Presentations presentations = new Presentations();
                    presentations = dataAccess.GetPresentationById(id);
                    ViewBag.Presentation_Name = presentations.Name;
                }
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Question([Bind] Questions questions, int id)
        {
            ViewBag.Msg = "Something went wrong !!";
            Presentations presentations = new Presentations();

            if (id == 0)
            {
                return NotFound();
            }
            else
            {
                ViewBag.Id = id;
                presentations = dataAccess.GetPresentationById(id);
                ViewBag.Presentation_Name = presentations.Name;
            }

            questions.Id_Presentation = id;
            questions.Presentation_Name = presentations.Name;

            if (ModelState.IsValid)
            {
                if (questions.Question == null)
                {
                    ViewBag.ErrorsMsgs = "Please write a question !!!";
                }
                else
                {
                    if (questions.Id_Presentation < 1)
                    {
                        ViewBag.ErrorsMsgs = "Please !!!";
                    }
                    else
                    {
                        for (int i = 2; i < questions.Answers.Count; i++)
                        {
                            if (questions.Answers[i].Answer == null)
                            {
                                questions.Answers[i].Correct_Answer = false;
                            }
                        }

                        if ((questions.Answers[0].Answer.Length >= 1) && (questions.Answers[1].Answer.Length >= 1))
                        {
                            if (questions.Answers.TrueForAll(x => x.Correct_Answer.Equals(false)) == true)
                            {
                                ViewBag.ErrorsMsgs = "Please choose at least one correct answer !!!";
                                return View(questions);
                            }
                            else
                            {
                                bool status = dataAccess.AddQuestion(questions, id);
                                if (status == true)
                                {
                                    ViewBag.Msg = "Well done !!";
                                    ViewBag.ErrorsMsgs = "You've just added a question !!";
                                    ModelState.Clear();
                                    return View();
                                }
                                else
                                {
                                    ViewBag.ErrorsMsgs = "Please try again !!!";
                                    return View();
                                }
                            }
                        }
                        else
                        {
                            ViewBag.ErrorsMsgs = "Please write some answers !!!";
                        }
                    }
                }
            }
            return View(questions);
        }

        [HttpPost]
        public void Shuffle_Questions([FromBody]Presentations presentations)
        {
            dataAccess.ShuffleQuestions(presentations);
        }

        [HttpPost]
        public void Set_Requirements([FromBody]List<Presentations> presentations)
        {
            foreach (var pres in presentations)
            {
                if (pres.Points_Required == 0 || pres.Points_Required > pres.Question_Count)
                {
                    //maybe something more, like info message ?!?!?!?!
                    return;
                }
                else
                {
                    //maybe with @pres_list ?!?!?!?!
                    dataAccess.SetRequirements(pres);
                }
            }
        }

        [HttpPost]
        public JsonResult Check_Requirements([FromBody] List<Presentations> presentations)
        {
            var pres = dataAccess.CheckRequirements(presentations);

            return Json(pres);
        }
    }
}