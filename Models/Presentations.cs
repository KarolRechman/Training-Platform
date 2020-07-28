using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Class that defines presentations(trainings) object
    /// </summary>
    public class Presentations
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Only 100 characters possible !!"),
         RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Some characters are not allowed !! Please use only letters and numbers.")]
        public string Name { get; set; }

        public string URL { get; set; }
        public string Type { get; set; }
        public string Type_Name { get; set; }
        public string src { get; set; }

        public int Author_Id { get; set; }
        public string Author_Name { get; set; }
        public int Author_Mandant { get; set; }
        public string Create_Date { get; set; }
        public string Last_Modification_Date { get; set; }

        [Display(Name = "Start Date")]
        public string Start_Date { get; set; }

        [Display(Name = "End Date")]
        public string End_Date { get; set; }

        [Display(Name = "Attempt Date")]
        public string Attempt_Date { get; set; }

        [Display(Name = "Attempts left")]
        public int Attempts_Left { get; set; }

        [MaxLength(100, ErrorMessage = "Only 100 characters possible !!"),
         RegularExpression(@"^[0-9,]{1,100}$", ErrorMessage = "Some characters are not allowed !! Please use only numbers and commas")]
        public string New_File_Name { get; set; }

        public List<Formats> Format_Types { get; set; }

        [Required]
        public List<IFormFile> FormFile { get; set; }
        public List<Files> Files { get; set; }
        public List<User> Users_Assigned { get; set; }
        public List<Questions> Questions { get; set; }

        [Display(Name = "Question quantity")]
        public int Question_Count { get; set; }

        public bool Shuffle_Questions { get; set; }
        public string User_Points { get; set; }
        public int Points_Required { get; set; }
        public double Points_Percentage { get; set; }
    }

    /// <summary>
    /// Class that defines training campaign object
    /// </summary>
    public class Training_Campaign
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Only 100 characters possible !!"),
         RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Some characters are not allowed !! Please use only letters and numbers.")]
        public string Name { get; set; }

        public List<Presentations> Presentations { get; set; }
        public List<Training_Group> Training_Groups { get; set; }
        public List<User> Users_Assigned { get; set; }

        public string Date_Range { get; set; }
        public int Author_Id { get; set; }
        public string Author_Name { get; set; }
        public string Create_Date { get; set; }
        public string Last_Modification_Date { get; set; }

        [Display(Name = "Start Date")]
        public string Start_Date { get; set; }

        [Display(Name = "End Date")]
        public string End_Date { get; set; }

        public int Attempts { get; set; }

        /// <summary>
        /// Sets Start_Date and End_Date properties from Date_Range
        /// </summary>
        public void GetDates()
        {
            string[] Data_Range = Date_Range.Split("-");
            Start_Date = Data_Range[0].TrimEnd();
            End_Date = Data_Range[1].Trim();
        }
    }

    /// <summary>
    /// Class that defines formats object
    /// </summary>
    public class Formats
    {
        public int Id { get; set; }
        public string Format { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Class that defines files object
    /// </summary>
    public class Files
    {
        public long number { get; set; }
        public string src { get; set; }
        public string Name { get; set; }
    }
}
