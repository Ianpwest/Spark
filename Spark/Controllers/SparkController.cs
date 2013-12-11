using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark.Classes;
using System.IO;

namespace Spark.Controllers
{
    public class SparkController : Controller
    {

        public ActionResult SparkCreate()
        {
            // Need to create a new model that holds all of the subject matters.
            Models.SparkCreateModel scModel = new Models.SparkCreateModel();
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
        public ActionResult SparkCreate(Models.SparkCreateModel sparkCreateModel)
        {
            // Redirect somewhere else when it fails.
            if (Request.Files.Count == 1)
            {
                Guid gName = Guid.NewGuid();

                //Add logic here to only accept certain MIME types?
                string strMimeType = Request.Files[0].ContentType;
                Stream streamFileStream = Request.Files[0].InputStream;
                string strFileName = Path.GetFileName(Request.Files[0].FileName);

                int fileLength = Request.Files[0].ContentLength;
                byte[] fileData = new byte[fileLength];
                streamFileStream.Read(fileData, 0, fileLength);
                //fileData = ResizeImage(fileData, fileLength);

                //MVCEventBench.Models.Image myImage = new MVCEventBench.Models.Image();
                //myImage.gEventImageGUID = Guid.NewGuid();
                //myImage.gEvent = gEvent;
                //myImage.imgContent = fileData;
                //myImage.strMIMEType = strMimeType;
                //myImage.strFileName = strFileName;

                //db.AddToImage(myImage);
                //db.SaveChanges();
                
            }
            if (!SparksDatabaseInterface.CreateSpark(sparkCreateModel))
                return RedirectToAction("Index", "Home");
            
            return RedirectToAction("Index", "Home");
        }
    }
}
