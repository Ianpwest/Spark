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
        #region Spark Create

        /// <summary>
        /// GET response for the SparkCreate view.
        /// Initializes all tag elements, argument type, userid, subject matter selectlist, and tag id,name, and image information.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult SparkCreate()
        {
            // Need to create a new model that holds all of the subject matters.
            SparkCreateModel scModel = new SparkCreateModel();
            scModel.Tag1 = -1; scModel.Tag2 = -1; scModel.Tag3 = -1; scModel.Tag4 = -1; scModel.Tag5 = -1;
            scModel.ArgEntryType = ArgumentEntryType.Neither;
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

        /// <summary>
        /// POST response to the SparkCreate view.
        /// Attempts to create the spark in the database. Redirects to specific pages depending upon the success of the creation.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult SparkCreate(SparkCreateModel model)
        {
            if (model == null)
                return RedirectToAction("Index", "Home"); // TODO - redirect to error page and log error.
            model.UserId = User.Identity.Name;

            int sparkPK = SparksDatabaseInterface.CreateSpark(model);

            // Spark failed to create - TODO : error log and redirect to error page.
            if (sparkPK == int.MinValue)
                return RedirectToAction("Index", "Home");


            if (model.ArgEntryType == ArgumentEntryType.Neither)
            {
                //Show the spark container
                return RedirectToAction("SparkContainer", "Spark", new { nSparkId = sparkPK });
            }

            arguments argumentModel = new arguments();

            //TODO: if this returns 0 (we have no user logged in) return a failure screen
            argumentModel.FKAccounts = AccountsDatabaseInterface.GetAccountsPKByUsername(User.Identity.Name);
            if (model.ArgEntryType == ArgumentEntryType.Agree)
                argumentModel.bIsAgree = true;
            else
                argumentModel.bIsAgree = false;

            argumentModel.FKSparks = sparkPK;

            return View("SparkArgumentCreate", argumentModel);
        }

        /// <summary>
        /// Returns a Json object to a javascript function containing the 64 bit encoded jpg information string given a broad category (subject matter)
        /// primary key.
        /// </summary>
        /// <param name="nCategoryId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Method called from javascript function that uploads a new tag generated by the view.
        /// The expected arguments are the tag name and the 64 bit encoded jpg image string from the view's img tag.
        /// Writes the image of the tag to a file then uploads the new tag to the database.
        /// Returns a Json object containing the uploaded tag's information to repopulate the the tag control.
        /// Return format is a comma delimmited set : PK,strName,strImage
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strImage"></param>
        /// <returns></returns>
        public ActionResult UploadNewTag(string strName, string strImage)
        {
            Guid gImg = Guid.NewGuid();
            Utilities.WriteImageToFile(gImg.ToString(), strImage, ImageLocation.Tag, Server);
            KeyValuePair<int,string> tagKVP = SparksDatabaseInterface.UploadTag(strName, gImg.ToString());
            if (tagKVP.Key == int.MinValue)
                // TODO - add error logging or redirection action here.
                return null;

            string strJpgImage = Utilities.GenerateImageString(gImg.ToString(), ImageLocation.Tag, Server);
            string strMessage = tagKVP.Key.ToString() + "," + tagKVP.Value + "," + strJpgImage;
            return Json(new { success = true, message = strMessage });
        }

        private SparkCreateModel ParseStringToModel(string strModelInfo)
        {
            SparkCreateModel scm = new SparkCreateModel();

            string[] strArray = strModelInfo.Split(',');
            if (strArray.Length < 9)
                return null;

            scm.Title = strArray[0];
            scm.Topic = strArray[1];
            scm.SubjectMatterId = int.Parse(strArray[2]);
            scm.Tag1 = int.Parse(strArray[3]);
            scm.Tag2 = int.Parse(strArray[4]);
            scm.Tag3 = int.Parse(strArray[5]);
            scm.Tag4 = int.Parse(strArray[6]);
            scm.Tag5 = int.Parse(strArray[7]);
            if (int.Parse(strArray[8]) == 1)
                scm.ArgEntryType = ArgumentEntryType.Agree;
            else
                scm.ArgEntryType = ArgumentEntryType.Disagree;

            return scm;
        }

        #endregion

        #region Spark Container

        //GET
        public ActionResult SparkContainer(int nSparkId)
        {
            //Get the spark
            //TODO:What if we fail?
            Models.sparks spark = SparksDatabaseInterface.GetSpark(nSparkId);

            //Pass the arguments for this spark to the view
            ViewBag.Arguments = GetArgumentsForSpark(nSparkId);

            //We failed to get a Spark for the given id
            if(spark == null)
            {
                return View("Error");
            }

            return View(spark);
        }

        private List<List<SparkArgumentModel>> GetArgumentsForSpark(int nSparkId)
        {
            List<List<SparkArgumentModel>> lstReturn = new List<List<SparkArgumentModel>>();

            List<SparkArgumentModel> lstArgumentsAgree = new List<SparkArgumentModel>();
            List<SparkArgumentModel> lstArgumentsDisagree = new List<SparkArgumentModel>();

            lstReturn.Add(lstArgumentsAgree);
            lstReturn.Add(lstArgumentsDisagree);

            //Begin Testing code
            List<Models.arguments> lstArguments = SparksDatabaseInterface.GetAllArgumentsForSpark(nSparkId); 

            //No arguments were found
            if (lstArguments == null || lstArguments.Count == 0)
            {
                return lstReturn;
            }

            //Run analytics and create sparkArgumentModels to return
            foreach (Models.arguments argument in lstArguments)
            {
                SparkArgumentModel sam = SparksDatabaseInterface.BuildSparkArgumentModel(argument);
                
                //Determine which list to put the argument in (agree vs disagree)
                if (sam.bIsAgree)
                    lstArgumentsAgree.Add(sam);
                else
                    lstArgumentsDisagree.Add(sam);
            }

            return lstReturn;
        }

        public ActionResult GetExpandedArgumentView(int id)
        {
            //using the id build the expanded argument view to return.
            arguments argument = SparksDatabaseInterface.GetArgument(id);

            if (argument == null)
                return View("Error");

            //Get the rest of the extended argument properties.
            SparkArgumentModel sam = SparksDatabaseInterface.BuildSparkArgumentModel(argument);

            //return PartialView("SparkArgumentExpanded", sam);
            return View("SparkArgumentExpanded", sam);
        }

        [HttpGet]
        public ActionResult CreateArgument(bool bAgree, int nSparkID)
        {
            arguments argumentModel = new arguments();

            //TODO: if this returns 0 (we have no user logged in) return a failure screen
            argumentModel.FKAccounts = AccountsDatabaseInterface.GetAccountsPKByUsername(User.Identity.Name);
            argumentModel.bIsAgree = bAgree;
            argumentModel.FKSparks = nSparkID;

            return View("SparkArgumentCreate", argumentModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateArgument(Models.arguments argumentModel)
        {
            if(SparksDatabaseInterface.CreateArgument(argumentModel))
            {
                //We were successful in adding the argument
                //TODO: Go to the right spark
                return RedirectToAction("SparkContainer", new { nSparkID = argumentModel.FKSparks });
            }

            //We failed TODO:What do we do when we fail
            return RedirectToAction("Error");//remove this line
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strDataConcat"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult CastArgumentVote(string strDataConcat)
        {
            string strId = strDataConcat.Split(',')[0].ToString(); // Argument id
            string strBool = strDataConcat.Split(',')[1].ToString(); // Upvote or downvote

            int nArgumentId = -1;
            if(!int.TryParse(strId, out nArgumentId))
            {
                SparksDatabaseInterface.LogError(User.Identity.Name,
                    "Error converting return argument to int value from ajax call in javascript method = CastArgumentVote in SparkContainer.js file.");
                return Json(new { success = false });
            }

            bool bConvert = false;
            if (!bool.TryParse(strBool, out bConvert))
            {
                SparksDatabaseInterface.LogError(User.Identity.Name,
                    "Error converting return argument to boolean value from ajax call in javascript method = CastArgumentVote in SparkContainer.js file.");
                return Json(new { success = false });
            }
            int nStatus = SparksDatabaseInterface.CastArgumentVote(nArgumentId, bConvert, User.Identity.Name);
            bool bSuccess = (nStatus >= 0) ? true : false; // non-negative statuses are success
            bool bNewVote = (nStatus == 0) ? true : false; // a zero status is a new vote
            bool bReverseVote = (nStatus == 3) ? true : false; // a 3 indicates the vote has been deleted.

            return Json(new { success = bSuccess, bNewVote = bNewVote, bReverseVote = bReverseVote });
        }
        #endregion
    }
}
