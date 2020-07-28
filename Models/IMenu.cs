namespace Quiz.Models
{
    /// <summary>
    /// Interface that defines menu objects
    /// </summary>
    public interface IMenu
    {
        int Id_Menu { get; set; }
        string Name { get; set; }
        string Controller_Name { get; set; }
        string Action_Name { get; set; }
        string Css_Class { get; set; }
        string Icon_Css_Class { get; set; }
        
        string Additional_Css { get; set; }
        string Additional_HTML { get; set; }
        string HTML_Attributes { get; set; }
        string HTML_Events { get; set; }
        string JS_Function { get; set; }
    }
}
