using Spark.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spark.Controllers
{
    public class HomeController : Controller
    {
        private const int CONST_MAX_TOPIC_CHARACTERS = 325;

        public ActionResult Homepage()
        {
            //Get the most popular sparks. TODO: Need to have a method that only returns first 20 or next 20 or something so that we can click next to go to the second page.
            List<Models.sparks> lstSparks = Calculations.SortSparksByPopularity(UtilitiesDatabaseInterface.GetDatabaseInstance());
            List<Models.SparkTileModel> lstSparkTiles = new List<Models.SparkTileModel>();

            // Attempts to find the current user id.
            int nCurrentUserId = AccountsDatabaseInterface.GetUserId(AccountsDatabaseInterface.GetDatabaseInstance(), User.Identity.Name);
            //Make sure the topics aren't over the max allowed characters
            foreach(Models.sparks spark in lstSparks)
            {
                Models.SparkTileModel tile = new Models.SparkTileModel();
                tile.PK = spark.PK;
                
                if(spark.strTopic.Length > CONST_MAX_TOPIC_CHARACTERS)
                {
                    tile.Topic = spark.strTopic.Substring(0, CONST_MAX_TOPIC_CHARACTERS - 3) + "...";
                }

                tile.UpvoteCount = SparksDatabaseInterface.GetSparkVoteCount(spark.PK, true);
                tile.DownvoteCount = SparksDatabaseInterface.GetSparkVoteCount(spark.PK, false);

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

                lstSparkTiles.Add(tile);
            }

            //TODO: Make a real error page. We have no sparks to display.
            if (lstSparkTiles == null)
                return View("Error");

            //Put the sparks in the viewbag so that the list can be consumed by the view.
            ViewBag.SparksTiles = lstSparkTiles;

            return View("Homepage");
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
    }
}
