using System.Web.Mvc;

namespace HR.Web.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            return Content("HR Application is running! Basic test successful.");
        }
        
        public ActionResult Status()
        {
            var status = new
            {
                Application = "HR Questionnaire System",
                Status = "Running",
                Framework = "ASP.NET MVC",
                Message = "Basic functionality working"
            };
            
            return Json(status, JsonRequestBehavior.AllowGet);
        }
    }
}
