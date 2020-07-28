using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Quiz.Models
{
    /// <summary>
    /// Class that defines Login object
    /// </summary>
    public class Login
    {
        [Required]
        public string login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string App_Name { get; set; }
        public string Host_Name { get; set; }
        public string Ip_Address { get; set; }
        public string Browser_Name { get; set; }
    }

    /// <summary>
    /// Class that defines User object
    /// </summary>
    public class User
    {
        public int Id_User { get; set; }
        public string Personal_Number { get; set; }
        public int Id_Mandant { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public bool Administrator { get; set; }
        public bool Onsite_Tech { get; set; }
        public bool DB_Admin { get; set; }
        public bool Supervisor { get; set; }

        public string Mandant_Culture { get; set; }

        public string Info_Login { get; set; }
        public string Info_Group { get; set; }
        public List<Training_Group> Groups_In { get; set; }

        public bool Assigned { get; set; }
        public int Sum_of_Points { get; set; }

        /// <summary>
        /// Retrieves info about groups in which
        /// </summary>
        /// <returns>List of training group objects</returns>
        public List<Training_Group> GetGroups()
        {
            List<Training_Group> groups = new List<Training_Group>();

            if (Info_Group != "Additional")
            {
                var Groups_Array = Info_Group.TrimEnd().Split(",");
                Groups_Array = Groups_Array.Where(g => g != "").ToArray();

                for (var i = 0; i < Groups_Array.Length; i++)
                {
                    Training_Group group = new Training_Group();

                    Groups_Array[i] = Groups_Array[i].TrimStart().TrimEnd();

                    group.Id_Group = Convert.ToInt32(Groups_Array[i].Substring(0, Groups_Array[i].IndexOf(" ")));
                    group.Name = Groups_Array[i].Replace(group.Id_Group.ToString(), "").TrimStart().TrimEnd();
                    groups.Add(group);
                }
            }
            return groups;
        }
    }

    public class Training_Group
    {
        public int Id_Group { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Only 100 characters possible !!"),
        RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Some characters are not allowed !! Please use only letters and numbers.")]
        public string Name { get; set; }

        public List<User> Users { get; set; }
    }
}
