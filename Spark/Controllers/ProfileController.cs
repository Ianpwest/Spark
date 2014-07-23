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
            return View();
        }

    }
}
