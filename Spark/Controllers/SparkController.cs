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

        [HttpPost]
        public ActionResult SparkCreate(SparkCreateModel model)
        {
            model.UserId = User.Identity.Name;

            int sparkPK = SparksDatabaseInterface.CreateSpark(model);
            // Spark failed to create - TODO : error log and redirect to error page.
            if (sparkPK == int.MinValue)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult SparkCreateWithArg(string strModelInfo)
        {
            SparkCreateModel scmModel = ParseStringToModel(strModelInfo);
            if (scmModel == null)
                return RedirectToAction("Index", "Home"); // TODO - redirect to error page and log error.
            scmModel.UserId = User.Identity.Name;
            
            int sparkPK = SparksDatabaseInterface.CreateSpark(scmModel);
            // Spark failed to create - TODO : error log and redirect to error page.
            if (sparkPK == int.MinValue)
                return RedirectToAction("Index", "Home");
            
            
            if (scmModel.ArgEntryType == ArgumentEntryType.Neither)
            {
                // TODO- redirect to action with error page.
            }

            arguments argumentModel = new arguments();

            //TODO: if this returns 0 (we have no user logged in) return a failure screen
            argumentModel.FKAccounts = AccountsDatabaseInterface.GetAccountsPKByUsername(User.Identity.Name);
            if (scmModel.ArgEntryType == ArgumentEntryType.Agree)
                argumentModel.bIsAgree = true;
            else
                argumentModel.bIsAgree = false;

            argumentModel.FKSparks = sparkPK;


            return PartialView("SparkArgumentCreate", argumentModel);
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

        public ActionResult UploadNewTag(string strName, string strImage)
        {
            Guid gImg = Guid.NewGuid();
            Utilities.WriteImageToFile(gImg.ToString(), strImage, ImageLocation.Tag, Server);
            SparksDatabaseInterface.UploadTag(strName, gImg.ToString());
            return Json(new { success = true, message = string.Empty });
        }

        //GET
        public ActionResult SparkContainer(/*int nSparkId*/) //Testing need to uncomment this out
        {
            //TESTING DELETE
            int nSparkId = 1;

            //Get the spark
            //TODO:What if we fail?
            Models.sparks spark = SparksDatabaseInterface.GetSpark(nSparkId);

            //Pass the arguments for this spark to the view
            ViewBag.Arguments = GetArgumentsForSpark(nSparkId);

            //We failed to get a Spark for the given id
            if(spark == null)
            {
                //We need to return an error view TODO
            }

            return View(spark);
        }

        private List<List<SparkArgumentModel>> GetArgumentsForSpark(int nSparkId)
        {
            List<List<SparkArgumentModel>> lstReturn = new List<List<SparkArgumentModel>>();

            List<SparkArgumentModel> lstArgumentsAgree = new List<SparkArgumentModel>();
            List<SparkArgumentModel> lstArgumentsDisagree = new List<SparkArgumentModel>();

            //Begin Testing code
            List<Models.arguments> lstArguments = SparksDatabaseInterface.GetAllArgumentsForSpark(1); //hard coded id, controller method should take an id


            //No arguments were found
            if (lstArguments == null || lstArguments.Count == 0)
            {
                return null;
            }

            //Run analytics and create sparkArgumentModels to return
            foreach (Models.arguments argument in lstArguments)
            {
                SparkArgumentModel sam = BuildSparkArgumentModel(argument);
                
                //Determine which list to put the argument in (agree vs disagree)
                if (sam.bIsAgree)
                    lstArgumentsAgree.Add(sam);
                else
                    lstArgumentsDisagree.Add(sam);
            }

            //Prepare the lists to be returned
            lstReturn.Add(lstArgumentsAgree);
            lstReturn.Add(lstArgumentsDisagree);

            return lstReturn;
        }

        public ActionResult GetExpandedArgumentView(int id)
        {
            //using the id build the expanded argument view to return.
            arguments argument = SparksDatabaseInterface.GetArgument(id);

            //Get the rest of the extended argument properties.
            SparkArgumentModel sam = BuildSparkArgumentModel(argument);

            return PartialView("SparkArgumentExpanded", sam);
        }

        private SparkArgumentModel BuildSparkArgumentModel(arguments argument)
        {
            //Need to pass this argument to the analytics engine to fill in the rest of the fields?
            SparkArgumentModel sam = new SparkArgumentModel();

            sam.id = argument.PK;
            sam.bIsAgree = argument.bIsAgree;
            sam.nArgumentScore = 25; //this should come from the analytics result.
            sam.nCommentCount = 420; //this should come from the analytics result.
            sam.nDownVote = 20; //analytics result
            sam.nInfluenceScore = 355; //analytics result
            sam.nUpVote = 300; //analytics result
            sam.strArgument = argument.strArgument;
            sam.strCitations = argument.strCitations;
            sam.strConclusion = argument.strConclusion;
            sam.strUserName = AccountsDatabaseInterface.GetUsername(argument.FKAccounts);

            return sam;
        }

        [HttpGet]
        public ActionResult CreateArgument(bool bAgree, int nSparkID)
        {
            arguments argumentModel = new arguments();

            //TODO: if this returns 0 (we have no user logged in) return a failure screen
            argumentModel.FKAccounts = AccountsDatabaseInterface.GetAccountsPKByUsername(User.Identity.Name);
            argumentModel.bIsAgree = bAgree;
            argumentModel.FKSparks = nSparkID;

            return PartialView("SparkArgumentCreate", argumentModel);
        }

        
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateArgument(Models.arguments argumentModel)
        {
            if(SparksDatabaseInterface.CreateArgument(argumentModel))
            {
                //We were successful in adding the argument
                //TODO: Go to the right spark
                return RedirectToAction("SparkContainer");
            }

            //We failed TODO:What do we do when we fail
            return RedirectToAction("SparkContainer");//remove this line
        }

        private SparkCreateModel ParseStringToModel(string strModelInfo)
        {
            SparkCreateModel scm = new SparkCreateModel();

            string[] strArray = strModelInfo.Split(',');
            if (strArray.Length < 9)
                return null;

            scm.Topic = strArray[0];
            scm.Description = strArray[1];
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
    }
}
