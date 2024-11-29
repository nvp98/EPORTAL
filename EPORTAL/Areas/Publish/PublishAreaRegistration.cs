using System.Web.Mvc;

namespace EPORTAL.Areas.Publish
{
    public class PublishAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Publish";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Publish_default",
                "Publish/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}