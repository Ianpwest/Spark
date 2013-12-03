using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark.Classes;

namespace Spark.Controllers
{
    public class SparkController : Controller
    {

        public ActionResult SparkCreate()
        {
            Models.sparks sparkModel = new Models.sparks();

            return View(sparkModel);
        }

        [HttpPost]
        public ActionResult SparkCreate(Models.sparks sparkModel)
        {
            // Do stuff
            DatabaseInterface.CreateSpark(sparkModel);
           
            return RedirectToAction("Index", "Home");
        }
    }
}
