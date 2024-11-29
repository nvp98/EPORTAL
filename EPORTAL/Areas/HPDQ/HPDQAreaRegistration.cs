using System.Web.Mvc;

namespace EPORTAL.Areas.HPDQ
{
    public class HPDQAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "HPDQ";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "HPDQ_default",
                "HPDQ/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}