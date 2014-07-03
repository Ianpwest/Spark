using Spark.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.IO;

namespace Spark.Controllers
{
    public class HomeController : Controller
    {
        private const int CONST_MAX_TOPIC_CHARACTERS = 325;

        public ActionResult Homepage()
        {
            //Get the most popular sparks. TODO: Need to have a method that only returns first 20 or next 20 or something so that we can click next to go to the second page.
            List<Models.sparks> lstSparks = Calculations.GetSparksByPopularityRange(UtilitiesDatabaseInterface.GetDatabaseInstance(), 0, 20);
            List<Models.SparkTileModel> lstSparkTiles = new List<Models.SparkTileModel>();

            //Get the categories and tags to filter on.
            ViewBag.Categories = SetupCategoryFilter();
            ViewBag.Tags = SetupTagFilter();

            //Get the tiles.
            lstSparkTiles = GetSparkTiles(lstSparks);
           
            //TODO: Make a real error page. We have no sparks to display.
            if (lstSparkTiles == null)
                return View("Error");

            //Put the sparks in the viewbag so that the list can be consumed by the view.
            ViewBag.SparksTiles = lstSparkTiles;

            return View("Homepage");
        }

        public ActionResult GetNextSparks(string strSparkIds)
        {
            string[] strArray = strSparkIds.Split(',');
            List<int> lstPKs = new List<int>();

            foreach (string strPK in strArray)
            {
                int nTest = 0;
                if(int.TryParse(strPK, out nTest))
                    lstPKs.Add(nTest);
            }
            
            List<Models.sparks> lstSparks = Calculations.GetNextSetSparks(UtilitiesDatabaseInterface.GetDatabaseInstance(), 20, lstPKs);
            foreach(Models.sparks spark in lstSparks)
            {
                lstPKs.Add(spark.PK);
            }
            int nRemainingSparks = Calculations.GetRemainingSparkCount(UtilitiesDatabaseInterface.GetDatabaseInstance(), lstPKs);
            
            List<Models.SparkTileModel> lstTiles = GetSparkTiles(lstSparks);

            //TODO: Make a real error page. We have no sparks to display.
            if (lstTiles == null)
                return View("Error");

            // TODO - do not use the serializer here to make the JSON using the list of tiles.
            // Instead, generate all of the HTML (partial views) here then package as Json with some sorta location and send back to client to load into view.
            //JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            //string strJson = jsSerializer.Serialize(lstTiles);

            string strPartial = string.Empty;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult result = ViewEngines.Engines.FindPartialView(ControllerContext, "SparkTileContainerPartial");
                ViewContext context = new ViewContext(ControllerContext, result.View, ViewData, TempData, sw);
                context.ViewBag.SparksTiles = lstTiles;

                result.View.Render(context, sw);
                result.ViewEngine.ReleaseView(ControllerContext, result.View);

                strPartial = sw.GetStringBuilder().ToString();

            }

            bool bIsRemainingSparks = false; // indicates if more sparks are remaining.
            if (nRemainingSparks > 0)
                bIsRemainingSparks = true;

            return Json(new { success = true, strTiles = strPartial, bIsMore = bIsRemainingSparks });
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
               return Homepage();
            else
                return WelcomeScreen();
        }

        public ActionResult WelcomeScreen()
        {
            return View("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetFilterResults(string strCategory, string strTag, string strSearchText)
        {
            int nCategory = int.MinValue;
            int nTag = int.MinValue;

            //Parse out the integer values of the key.
            if(!string.IsNullOrEmpty(strCategory)){Int32.TryParse(strCategory, out nCategory);}
            if(!string.IsNullOrEmpty(strTag)){Int32.TryParse(strTag, out nTag);}

            List<Models.sparks> lstSparks = Calculations.FilterSparksByHomeParameters(UtilitiesDatabaseInterface.GetDatabaseInstance(), nCategory, nTag, strSearchText);

            List<Models.SparkTileModel> lstTiles = GetSparkTiles(lstSparks);

            //Put the sparks in the viewbag so that the list can be consumed by the view.
            ViewBag.SparksTiles = lstTiles;

            //Get the categories and tags to filter on.
            ViewBag.Categories = SetupCategoryFilter();
            ViewBag.Tags = SetupTagFilter();

            return PartialView("SparkTileContainerPartial");
        }

        [Authorize]
        public ActionResult CastSparkVote(string strDataConcat)
        {
            string strId = strDataConcat.Split(',')[0].ToString(); // SparkId
            string strBool = strDataConcat.Split(',')[1].ToString(); // Upvote or downvote

            int nSparkId = -1;
            if (!int.TryParse(strId, out nSparkId))
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

            int nStatus = SparksDatabaseInterface.CastSparkVote(nSparkId, bConvert, User.Identity.Name);
            bool bSuccess = (nStatus >= 0) ? true : false; // Non-negative statuses are successful.
            bool bNewVote = (nStatus == 0) ? true : false; // a zero status is a new vote
            bool bReverseVote = (nStatus == 3) ? true : false; // a 3 indicates the vote has been deleted.

            return Json(new { success = bSuccess, bIsNewVote = bNewVote, bReverseVote = bReverseVote });
        }

        private SelectList SetupCategoryFilter()
        {
            Dictionary<int, string> dictCategories = SparksDatabaseInterface.GetAllCategories();

            List<SelectListItem> listCategories = new List<SelectListItem>();
            foreach (KeyValuePair<int, string> kvp in dictCategories)
            {
                listCategories.Add(new SelectListItem { Text = kvp.Value, Value = kvp.Key.ToString() });
            }

            return new SelectList(listCategories, "Value", "Text");
        }

        private SelectList SetupTagFilter()
        {
            Dictionary<int, string> dictTags = SparksDatabaseInterface.GetAllTags();

            List<SelectListItem> listTags = new List<SelectListItem>();
            foreach (KeyValuePair<int, string> kvp2 in dictTags)
            {
                listTags.Add(new SelectListItem { Text = kvp2.Value, Value = kvp2.Key.ToString() });
            }

            return new SelectList(listTags, "Value", "Text");
        }

        /// <summary>
        /// Method to generate tile partials given a list of sparks. Handles setting max length for the topics and the current vote counts.
        /// </summary>
        /// <param name="lstSparks">List of sparks to convert</param>
        /// <returns>List of tiles.</returns>
        private List<Models.SparkTileModel> GetSparkTiles(List<Models.sparks> lstSparks)
        {
            List<Models.SparkTileModel> lstReturn = new List<Models.SparkTileModel>();

            //Make sure the topics aren't over the max allowed characters and initialize the voting counts.
            foreach(Models.sparks spark in lstSparks)
            {
                Models.SparkTileModel tile = new Models.SparkTileModel();
                tile.PK = spark.PK;
                tile.Topic = spark.strTopic;

                if(spark.strTopic.Length > CONST_MAX_TOPIC_CHARACTERS)
                {
                    tile.Topic = spark.strTopic.Substring(0, CONST_MAX_TOPIC_CHARACTERS - 3) + "...";
                }

                //Set the vote counts appropriately.
                tile.UpvoteCount = SparksDatabaseInterface.GetSparkVoteCount(spark.PK, true);
                tile.DownvoteCount = SparksDatabaseInterface.GetSparkVoteCount(spark.PK, false);

                // Attempts to find the current user id.
                int nCurrentUserId = AccountsDatabaseInterface.GetUserId(AccountsDatabaseInterface.GetDatabaseInstance(), User.Identity.Name);

                int nVoteStatus = SparksDatabaseInterface.GetUserSparkVoteStatus(nCurrentUserId, spark.PK);
                
                if (nVoteStatus == 1) // indicates the user has made an upvote for this spark.
                {
                    tile.UserVoted = true;
                    tile.VoteIsUpvote = true;
                }
                else if (nVoteStatus == 2) // indicates the user has made a downvote for this spark.
                {
                    tile.UserVoted = true;
                    tile.VoteIsUpvote = false;
                }
                else
                    tile.UserVoted = false; // indicates the user has not yet voted on this spark or there was an error in finding the user vote.

                lstReturn.Add(tile);
            }

            return lstReturn;
        }
    }
}
