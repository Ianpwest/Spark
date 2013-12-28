using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark.Classes;
using System.IO;
using Spark.Models;
using System.Drawing;
using System.Xml.Linq;

namespace Spark.Controllers
{
    public class SparkController : Controller
    {

        public ActionResult SparkCreate()
        {
            // Need to create a new model that holds all of the subject matters.
            SparkCreateModel scModel = new SparkCreateModel();
            scModel.Tag1 = -1; scModel.Tag2 = -1; scModel.Tag3 = -1; scModel.Tag4 = -1; scModel.Tag5 = -1;
            scModel.UserId = User.Identity.Name;
            List<SelectListItem> lstItems = new List<SelectListItem>();

            foreach(KeyValuePair<int,string> kvp in UtilitiesDatabaseInterface.GetSubjectMatters())
            {
                SelectListItem sli = new SelectListItem {Text = kvp.Value, Value = kvp.Key.ToString() };
                lstItems.Add(sli);
            }

            scModel.SubjectMattersAll = new SelectList(lstItems, "Value", "Text", lstItems.First());
            UtilitiesDatabaseInterface.GenerateTagInfoForSpark(scModel);
            Dictionary<int, string> dictIdAndImgString = new Dictionary<int, string>();
            if(scModel.TagIdAndImages != null && scModel.TagIdAndNames != null)
            {
                foreach (KeyValuePair<int, string> kvp in scModel.TagIdAndImages)
                {
                    if (dictIdAndImgString.ContainsKey(kvp.Key))
                        continue;
                    dictIdAndImgString.Add(kvp.Key, Utilities.GenerateImageString(kvp.Value, ImageLocation.Tag, Server));
                }
            }

            scModel.TagIdAndImages = dictIdAndImgString;

            return View(scModel);
        }

        [HttpPost]
        public ActionResult SparkCreate(SparkCreateModel model)
        {
            model.UserId = User.Identity.Name;
            //model.FileName = Guid.NewGuid().ToString() + ".jpg"; ;
            // Redirect somewhere else when it fails.
            
            if (!SparksDatabaseInterface.CreateSpark(model))
                return RedirectToAction("Index", "Home");

            //// If record was inserted correct, try to add an image if it exists.
            //if (Request.Files.Count == 1)
            //{
            //    Utilities.WriteImageToFile(model.FileName, Request, Server);
            //    SparksDatabaseInterface.LogInteraction(User.Identity.Name, Classes.DatabaseInterfaces.InteractionType.SubmitForm);
            //}

            return RedirectToAction("Index", "Home");
        }

        public ActionResult GetImage(string nCategoryId)
        {
            string strFileName = "";
            int nTest = 0;
            if (int.TryParse(nCategoryId, out nTest))
                strFileName = UtilitiesDatabaseInterface.GetSubjectMatterImageName(nTest);
            else
                UtilitiesDatabaseInterface.LogError(User.Identity.Name, "Failed to find subject matter Id from view.");

            if(string.IsNullOrEmpty(strFileName))
            {
                UtilitiesDatabaseInterface.LogError(User.Identity.Name, "Could not find filename for subject matter.");
                return null;
            }

            string strMessageReturn = Utilities.GenerateImageString(strFileName, ImageLocation.Category, Server);
            return Json(new { success = true, message = strMessageReturn });
        }


        public ActionResult SparkContainer()
        {
            //TODO: get spark information such as topic...etc...
            return View();
        }

    }
}
