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
            scModel.UserId = User.Identity.Name;
            List<SelectListItem> lstItems = new List<SelectListItem>();

            foreach(KeyValuePair<int,string> kvp in UtilitiesDatabaseInterface.GetSubjectMatters())
            {
                SelectListItem sli = new SelectListItem {Text = kvp.Value, Value = kvp.Key.ToString() };
                lstItems.Add(sli);
            }

            scModel.SubjectMattersAll = new SelectList(lstItems, "Value", "Text", lstItems.First());

            return View(scModel);
        }

        [HttpPost]
        public ActionResult SparkCreate(SparkCreateModel model)
        {
            model.UserId = User.Identity.Name;
            model.FileName = Guid.NewGuid().ToString() + ".jpg"; ;
            // Redirect somewhere else when it fails.
            
            if (!SparksDatabaseInterface.CreateSpark(model))
                return RedirectToAction("Index", "Home");

            // If record was inserted correct, try to add an image if it exists.
            if (Request.Files.Count == 1)
            {
                Utilities.WriteImageToFile(model.FileName, Request, Server);
                SparksDatabaseInterface.LogInteraction(User.Identity.Name, Classes.DatabaseInterfaces.InteractionType.SubmitForm);
            }

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

            // Loads the config xml document.
            XDocument xDoc = XDocument.Load(Server.MapPath("~/App_Data/Config.xml"));
            IEnumerable<XElement> configuration = xDoc.Elements();

            // Extracts the server location of the image files from the config doc.
            // TODO - can't search through multiple child tags with this method.
            var strFilePath = (from r in xDoc.Elements()
                               where r.Element("filepath").Attribute("name").Value == "sparkCategoryImgRoot"
                               select r.Element("filepath").Attribute("value").Value).FirstOrDefault();

            byte[] byArray = new byte[0];
            FileStream fs;
            BinaryReader br;

            try
            {
                fs = new FileStream(strFilePath + strFileName, FileMode.Open, FileAccess.Read);
                byArray = new byte[fs.Length];
                br = new BinaryReader(fs);

                byArray = br.ReadBytes((int)fs.Length);
            }
            catch (Exception ex)
            {
                UtilitiesDatabaseInterface.LogError(User.Identity.Name, "Filestream error.", ex.ToString(), ex.StackTrace, "SparkController", "GetImage", "fs");
            }

            string strMessageReturn = String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(byArray));
            return Json(new { success = true, message = strMessageReturn });
        }

    }
}
