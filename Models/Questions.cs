using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quiz.Models
{
    /// <summary>
    /// Class that defines menu Questions object
    /// </summary>
    public class Questions
    {       
        public int Id_Presentation { get; set; }        
        public string Presentation_Name { get; set; }        
        public int Id { get; set; }

        [Required]
        public string Question { get; set; }

        public List<Answers> Answers { get; set; }
        public string Time { get; set; }
    }

    /// <summary>
    /// Class that defines menu Answers object
    /// </summary>
    public class Answers
    {
        public int Id_Answer { get; set; }
        public string Answer { get; set; }
        public bool Correct_Answer { get; set; }
    }
}
