using System.Web.Mvc;

namespace EPORTAL.Areas.TagSign
{
    public class TagSignAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TagSign";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TagSign_default",
                "TagSign/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}