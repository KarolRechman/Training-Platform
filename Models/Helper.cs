using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quiz.Models
{
    public class Helper
    {
        private readonly DataAccess dataAccess = new DataAccess();

        /// <summary>
        /// Retrieves directory path from "appsettings.json" file
        /// </summary>
        /// <returns>Current directory, path were files are stored</returns>
        public string GetDirectory()
        {
            var config = configuration();
            string directory = config.GetSection("StaticFiles").GetSection("StaticFilesFolder").Value;

            return directory;
        }

        /// <summary>
        /// Builds configuration object, based on "appsettings.json" file
        /// </summary>
        /// <returns>Configuration object</returns>
        public IConfigurationRoot configuration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        /// <summary>
        /// Checks for trainings that are on disk, but doesn't exists in database, then method is restoring them.
        /// There're 3 methods for each files format, in future will be one for all
        /// </summary>
        /// <param name="presentations">List of presentation objects</param>
        public void CheckLostPresentationsPP(List<Presentations> presentations)
        {
            string directory = GetDirectory();

            string dirPathPP = directory + "\\Power Point";

            if (Directory.Exists(dirPathPP) == true)
            {

                var dirs = from dir in Directory.EnumerateDirectories(dirPathPP, "", SearchOption.AllDirectories) select dir;

                foreach (var dir in dirs)
                {
                    string Lost_Presentation = dir.Substring(dir.LastIndexOf("\\") + 1);

                    if (presentations.Exists(x => x.Name.Equals(Lost_Presentation)))
                    {
                        Lost_Presentation = "jest ok";
                    }
                    else
                    {
                        Presentations presentation_New = new Presentations()
                        {
                            Name = Lost_Presentation,
                            Type = "jpg",
                            Type_Name = "Power Point",
                            URL = "Power Point" + "\\" + Lost_Presentation + "\\"
                        };

                        dataAccess.AddPresentation(presentation_New);
                    }
                }
                List<string> workDirs = new List<string>(dirs);
            }
        }

        public void CheckLostPresentationsPDF(List<Presentations> presentations)
        {
            string directory = GetDirectory();

            string dirPathPP = directory + "\\PDF";

            if (Directory.Exists(dirPathPP) == true)
            {
                var dirs = from dir in Directory.EnumerateDirectories(dirPathPP, "", SearchOption.AllDirectories) select dir;

                foreach (var dir in dirs)
                {
                    string Lost_Presentation = dir.Substring(dir.LastIndexOf("\\") + 1);

                    if (presentations.Exists(x => x.Name.Equals(Lost_Presentation)))
                    {
                        Lost_Presentation = "jest ok";
                    }
                    else
                    {
                        Presentations presentation_New = new Presentations()
                        {
                            Name = Lost_Presentation,
                            Type = "pdf",
                            Type_Name = "PDF",
                            URL = "PDF" + "\\" + Lost_Presentation + "\\"
                        };

                        dataAccess.AddPresentation(presentation_New);
                    }
                }
                List<string> workDirs = new List<string>(dirs);
            }
        }

        public void CheckLostPresentationsVideo(List<Presentations> presentations)
        {
            string directory = GetDirectory();

            string dirPathPP = directory + "\\Video";
            if (Directory.Exists(dirPathPP) == true)
            {

                var dirs = from dir in Directory.EnumerateDirectories(dirPathPP, "", SearchOption.AllDirectories) select dir;

                foreach (var dir in dirs)
                {
                    string Lost_Presentation = dir.Substring(dir.LastIndexOf("\\") + 1);

                    if (presentations.Exists(x => x.Name.Equals(Lost_Presentation)))
                    {
                        Lost_Presentation = "jest ok";
                    }
                    else
                    {
                        Presentations presentation_New = new Presentations()
                        {
                            Name = Lost_Presentation,
                            Type = "mp4",
                            Type_Name = "Video",
                            URL = "Video" + "\\" + Lost_Presentation + "\\"
                        };

                        dataAccess.AddPresentation(presentation_New);
                    }
                }
                List<string> workDirs = new List<string>(dirs);
            }
        }

        /// <summary>
        /// Retrieves employees that are not assigned to any group or training campagin
        /// </summary>
        /// <param name="Users_Assigned">List of user objects</param>
        /// <param name="training_Groups">List of training group objects</param>
        /// <returns></returns>
        public List<User> Available_Users(List<User> Users_Assigned, List<Training_Group> training_Groups)
        {
            var Available_Users = dataAccess.GetAllUsers(training_Groups);
            Available_Users = Available_Users.Where(u => !Users_Assigned.Any(ua => ua.Id_User == u.Id_User)).ToList();
            return Available_Users;
        }
    }


}
