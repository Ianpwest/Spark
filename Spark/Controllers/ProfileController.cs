using Spark.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spark.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult MyActivity()
        {
            List<Models.sparks> lstSparks = SparksDatabaseInterface.GetSparksForUser(User.Identity.Name, 0, 10);
            List<Models.SparkTileModel> lstSparkTiles = new List<Models.SparkTileModel>();

            //Get the tiles.
            lstSparkTiles = Utilities.GetSparkTiles(lstSparks, User, Server);

            //TODO: Make a real error page. We have no sparks to display.
            if (lstSparkTiles == null)
                return View("Error");

            //Put the sparks in the viewbag so that the list can be consumed by the view.
            ViewBag.SparksTiles = lstSparkTiles;

            return View();
        }

    }
}
