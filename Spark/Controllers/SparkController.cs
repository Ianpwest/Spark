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
                WriteImageToFile(model.FileName);
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

        private void WriteImageToFile(String strFileName)
        {
            //Add logic here to only accept certain MIME types?
            string strMimeType = Request.Files[0].ContentType;
            Stream streamFileStream = Request.Files[0].InputStream;

            int fileLength = Request.Files[0].ContentLength;
            byte[] fileData = new byte[fileLength];
            streamFileStream.Read(fileData, 0, fileLength);
            fileData = ResizeImage(fileData, fileLength);

            XDocument xDoc = XDocument.Load(Server.MapPath("~/App_Data/Config.xml"));
            IEnumerable<XElement> configuration = xDoc.Elements();

            var strFilePath = (from r in xDoc.Elements()
                               where r.Element("filepath").Attribute("name").Value == "sparkImgRoot"
                               select r.Element("filepath").Attribute("value").Value).FirstOrDefault();

            try
            {
                strFilePath += strFileName;

                System.IO.FileStream _FileStream = new System.IO.FileStream(strFilePath, System.IO.FileMode.Create,
                              System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from
                // a byte array.
                _FileStream.Write(fileData, 0, fileData.Length);

                // close file stream
                _FileStream.Close();
            }
            catch
            {

            }
        }

        /// <summary>
        /// Takes an image byte array and resizes it to a set height, width
        /// </summary>
        /// <param name="arryImage">byte array containing the image data</param>
        /// <returns>Resized byte array data</returns>
        private byte[] ResizeImage(byte[] arryImage, int fileLength)
        {
            //Memory stream for old image
            MemoryStream msImage = new MemoryStream();

            //Memory stream for the resized image
            MemoryStream msNewImage = new MemoryStream();

            msImage.Write(arryImage, 0, fileLength);

            Bitmap bmpImage = new Bitmap(msImage);

            //Set the constant for the size of the image here.
            Bitmap resizedImage = new Bitmap(400, 200);

            using (Graphics gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(bmpImage, new Rectangle(0, 0, 400, 200),
                    new Rectangle(0, 0, bmpImage.Width, bmpImage.Height), GraphicsUnit.Pixel);
            }

            resizedImage.Save(msNewImage, System.Drawing.Imaging.ImageFormat.Jpeg);

            return msNewImage.ToArray();

        }
    }
}
