using Spark.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spark.Controllers
{
    public class HomeController : Controller
    {
        private const int CONST_MAX_TOPIC_CHARACTERS = 325;

        public ActionResult Homepage()
        {
            //Get the most popular sparks. TODO: Need to have a method that only returns first 20 or next 20 or something so that we can click next to go to the second page.
            List<Models.sparks> lstSparks = Calculations.SortSparksByPopularity(UtilitiesDatabaseInterface.GetDatabaseInstance());

            //Make sure the topics aren't over the max allowed characters
            foreach(Models.sparks spark in lstSparks)
            {
                if(spark.strTopic.Length > CONST_MAX_TOPIC_CHARACTERS)
                {
                    spark.strTopic = spark.strTopic.Substring(0, CONST_MAX_TOPIC_CHARACTERS - 3) + "...";
                }
            }

            //TODO: Make a real error page. We have no sparks to display.
            if (lstSparks == null)
                return View("Error");

            //Put the sparks in the viewbag so that the list can be consumed by the view.
            ViewBag.Sparks = lstSparks;

            return View("Homepage");
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
               return Homepage();
            else
                return WelcomeScreen();
        }

        public ActionResult WelcomeScreen()
        {
            return View("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
