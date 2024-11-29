using System.Web.Mvc;

namespace EPORTAL.Areas.View360
{
    public class View360AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "View360";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "View360_default",
                "View360/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}