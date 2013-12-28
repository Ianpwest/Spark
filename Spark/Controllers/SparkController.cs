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

            string strMessageReturn = Utilities.GenerateImageString(strFileName, ImageLocation.Category, Server);
            return Json(new { success = true, message = strMessageReturn });
        }

        //GET
        public ActionResult SparkContainer()
        {
            //TODO: get spark information such as topic...etc...
            List<List<SparkArgumentModel>> lstReturn = new List<List<SparkArgumentModel>>();

            List<SparkArgumentModel> lstArgumentsAgree = new List<SparkArgumentModel>();
            List<SparkArgumentModel> lstArgumentsDisagree = new List<SparkArgumentModel>();

            //Begin Testing code
            List<Models.arguments> lstArguments = SparksDatabaseInterface.GetAllArgumentsForSpark(1); //hard coded id, controller method should take an id


            //No arguments were found
            if (lstArguments == null || lstArguments.Count == 0)
            {
                lstReturn.Add(lstArgumentsAgree);
                lstReturn.Add(lstArgumentsDisagree);
                ViewBag.Arguments = lstReturn;
                return View(//TODO: no arguments found
                            );
            }
                

            //Run analytics and create sparkArgumentModels to return
            foreach(Models.arguments argument in lstArguments)
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

                if (sam.bIsAgree)
                    lstArgumentsAgree.Add(sam);
                else
                    lstArgumentsDisagree.Add(sam);
            }

            //Prepare the lists to be returned
            lstReturn.Add(lstArgumentsAgree);
            lstReturn.Add(lstArgumentsDisagree);
            ViewBag.Arguments = lstReturn;

            //End testing code

            return View();
        }

        public ActionResult GetExpandedArgumentView(int id)
        {
            //using the id build the expanded argument view to return.



            return PartialView("SparkArgumentExpanded");
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
    }
}
