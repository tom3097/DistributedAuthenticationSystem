using System.Web.Mvc;

namespace DistributedAuthSystem.Controllers
{
    public class HomeController : Controller
    {
        #region methods

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        #endregion
    }
}
