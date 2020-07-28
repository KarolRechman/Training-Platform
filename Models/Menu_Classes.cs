using System.Collections.Generic;

namespace Quiz.Models
{
    /// <summary>
    /// Class that defines menu parent object
    /// </summary>
    public class Parent_Menu : IMenu
    {
        public int Id_Parent { get; set; }
        public string Name { get; set; }

        public List<Child_Menu> Child_Menus { get; set; }
        ///////////////////////////////////////////////////
        public int Id_Menu { get; set; }
        public string Controller_Name { get; set; }
        public string Action_Name { get; set; }
        ///////////////////////////////////////////////////
        public string Css_Class { get; set; }
        public string Icon_Css_Class { get; set; }

        public string Additional_Css { get; set; }
        public string Additional_HTML { get; set; }
        public string HTML_Attributes { get; set; }
        public string HTML_Events { get; set; }
        public string JS_Function { get; set; }
    }

    /// <summary>
    /// Class that defines menu child object
    /// </summary>
    public class Child_Menu : IMenu
    {
        public int Id_Parent { get; set; }
        public string Name { get; set; }

        public int Id_Menu { get; set; }
        public string Controller_Name { get; set; }
        public string Action_Name { get; set; }
        public string Css_Class { get; set; }
        public string Icon_Css_Class { get; set; }

        public string Additional_Css { get; set; }
        public string Additional_HTML { get; set; }
        public string HTML_Attributes { get; set; }
        public string HTML_Events { get; set; }
        public string JS_Function { get; set; }
    }
}
