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
            // Need to create a new model that holds all of the subject matters.
            Models.sparks sparkModel = new Models.sparks();

            return View(sparkModel);
        }

        [HttpPost]
        public ActionResult SparkCreate(Models.sparks sparkModel)
        {
            // Redirect somewhere else when it fails.
            if(!DatabaseInterface.CreateSpark(sparkModel, User.Identity.Name))
                return RedirectToAction("Index", "Home");
            
            return RedirectToAction("Index", "Home");
        }
    }
}
