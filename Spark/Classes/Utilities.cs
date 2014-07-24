using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Spark.Classes
{
    public static class Utilities
    {
        /// <summary>
        /// Method to encrypt a given string
        /// </summary>
        /// <param name="strToEncrypt">String to encrypt</param>
        /// <returns>Encrypted string</returns>
        public static String Encrypt(string strToEncrypt)
        {
            string strEncryptedPassword = string.Empty;
            MD5 md5Encryption = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5Encryption.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strToEncrypt));

            //get hash result after compute it
            byte[] result = md5Encryption.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        /// <summary>
        /// Method to get a random salt
        /// </summary>
        /// <returns>Salt string</returns>
        public static String GetSalt()
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[16];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// Method to find the encrypted string to compare against given a
        /// password string and a salt string
        /// </summary>
        /// <param name="strPassword">Password String</param>
        /// <param name="strSalt">Salt String</param>
        /// <returns>Encrypted combined value</returns>
        public static string GetHashPassword(string strPassword, string strSalt)
        {
            string strPasswordToCompare = strSalt + strPassword;

            return Encrypt(strPasswordToCompare);
        }

        /// <summary>
        /// Method to send an email from our gmail account. Credentials are inside this method for 
        /// the account.
        /// </summary>
        /// <param name="strEmail">E-mail to send to</param>
        /// <param name="strActivationGUID">Guid to send the user to activate against</param>
        public static bool SendEmail(string strEmailTo, string strSubject, string strMessage)
        {
            //Successfully registered
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                //TODO: Push these values to an xml file or database. Encrypt them
                Credentials = new NetworkCredential("SparkItEmail@gmail.com", "Margaritas!"),
                EnableSsl = true
            };

            try
            {
                client.Send("SparkItEmail@gmail.com", strEmailTo, strSubject, strMessage);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes an image to a file. Attempts to write to the filename specified in the parameter.
        /// Requires an HttpRequestBase and HttpServerUtilityBase class given from the controller to find the appropriate file.
        /// Uses the first file found in the HttpRequestBase object.
        /// Returns true if no exceptions were encountered, else returns false.
        /// </summary>
        /// <param name="strFileName">Full filepath to save file on the server.</param>
        /// <param name="Request">Http request object.</param>
        /// <param name="Server">Http server utilities object.</param>
        /// <returns></returns>
        public static bool WriteImageToFile(string strFileName, HttpRequestBase Request, HttpServerUtilityBase Server)
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
            catch(Exception ex)
            {
                UtilitiesDatabaseInterface.LogError(string.Empty, "Failed to write image to file.", ex.ToString(), ex.StackTrace);
                return false;
            }
            return true;
        }

        public static bool WriteImageToFile(string strFileName, string strImgString, ImageLocation imgLoc, HttpServerUtilityBase Server)
        {
            string strBytes = strImgString.Substring(strImgString.IndexOf(',') + 1);
            byte[] bytes = Convert.FromBase64String(strBytes);

            XDocument xDoc = XDocument.Load(Server.MapPath("~/App_Data/Config.xml"));
            IEnumerable<XElement> configuration = xDoc.Elements();
            string strRootFolder = Enum.GetName(typeof(ImageLocation), (int)imgLoc);

            // Extracts the server location of the image files from the config doc.
            // TODO - can't search through multiple child tags with this method.
            var strFilePath = (from r in configuration.FirstOrDefault().Elements()
                               where r.Attribute("name").Value == strRootFolder
                               select r.Attribute("value").Value).FirstOrDefault();

            if (string.IsNullOrEmpty(strFilePath))
                // TODO - perform error logging here.
                return false;

            // Adds .jpg to the end of the filename if it does not already contain .jpg or .jpeg.
            if (!strFileName.ToLower().EndsWith(".jpg"))
                strFileName += ".jpg";

            strFilePath += strFileName;
            try
            {
                FileStream fs = new FileStream(strFilePath, FileMode.CreateNew, FileAccess.Write);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                UtilitiesDatabaseInterface.LogNonUserError("Filestream write error.", ex.ToString(), ex.StackTrace, "Utilities", "WriteImageToFile", "fs");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Takes an image byte array and resizes it to a set height, width
        /// </summary>
        /// <param name="arryImage">byte array containing the image data</param>
        /// <returns>Resized byte array data</returns>
        public static byte[] ResizeImage(byte[] arryImage, int fileLength)
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

        /// <summary>
        /// Generates a base 64 string that can be used to place into an image tag's "src" attribute. This searches the given directory from the imgLoc parameter for the file that
        /// uses the file name specified in the strFileName parameter. The Server parameter is the "Server" property used in the controllers.
        /// </summary>
        /// <param name="strFileName">Simple file name inside the root image directory.</param>
        /// <param name="imgLoc">Enumeration to indicate which directory to check.</param>
        /// <param name="Server">Server property given from the controller. Used to map to the config file.</param>
        /// <returns></returns>
        public static string GenerateImageString(string strFileName, ImageLocation imgLoc, HttpServerUtilityBase Server)
        {
            XDocument xDoc = XDocument.Load(Server.MapPath("~/App_Data/Config.xml"));
            IEnumerable<XElement> configuration = xDoc.Elements();
            string strRootFolder = Enum.GetName(typeof(ImageLocation), (int)imgLoc);
            
            // Extracts the server location of the image files from the config doc.
            // TODO - can't search through multiple child tags with this method.
            var strFilePath = (from r in configuration.FirstOrDefault().Elements()
                               where r.Attribute("name").Value == strRootFolder
                               select r.Attribute("value").Value).FirstOrDefault();

            if (string.IsNullOrEmpty(strFilePath) || string.IsNullOrEmpty(strFileName))
                return string.Empty;

            byte[] byArray = new byte[0];
            FileStream fs;
            BinaryReader br;
            if (!strFileName.ToLower().EndsWith(".png"))
                strFileName += ".png";
            try
            {
                fs = new FileStream(strFilePath + strFileName, FileMode.Open, FileAccess.Read);
                byArray = new byte[fs.Length];
                br = new BinaryReader(fs);

                byArray = br.ReadBytes((int)fs.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                UtilitiesDatabaseInterface.LogNonUserError("Filestream read error.", ex.ToString(), ex.StackTrace, "Utilities", "GenerateImageString", "fs");
                return string.Empty;
            }

            return String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(byArray)); ;
        }

        public static void UpdateDatabaseTagsFromFolder(HttpServerUtilityBase Server)
        {
            XDocument xDoc = XDocument.Load(Server.MapPath("~/App_Data/Config.xml"));
            IEnumerable<XElement> configuration = xDoc.Elements();

            var strFilePath = (from r in xDoc.Elements().FirstOrDefault().Elements()
                               where r.Attribute("name").Value == "Tag"
                               select r.Attribute("value").Value).FirstOrDefault();

            if (string.IsNullOrEmpty(strFilePath))
                return;

            string[] filePaths = Directory.GetFiles(strFilePath);

            foreach(string strFile in filePaths)
            {
                string strShortFileName = Path.GetFileName(strFile);
                UtilitiesDatabaseInterface.AddTag(Regex.Replace(strShortFileName.Substring(0, strShortFileName.IndexOf('.')), "(\\B[A-Z])", " $1"), strShortFileName);
            }
        }

        /// <summary>
        /// Method to generate tile partials given a list of sparks. Handles setting max length for the topics and the current vote counts.
        /// </summary>
        /// <param name="lstSparks">List of sparks to convert</param>
        /// <returns>List of tiles.</returns>
        public static List<Models.SparkTileModel> GetSparkTiles(List<Models.sparks> lstSparks, System.Security.Principal.IPrincipal User, HttpServerUtilityBase Server)
        {
            List<Models.SparkTileModel> lstReturn = new List<Models.SparkTileModel>();
            int CONST_MAX_TOPIC_CHARACTERS = 325;

            //Make sure the topics aren't over the max allowed characters and initialize the voting counts.
            foreach (Models.sparks spark in lstSparks)
            {
                Models.SparkTileModel tile = new Models.SparkTileModel();
                tile.PK = spark.PK;
                tile.Topic = spark.strTopic;
                tile.Title = spark.strTitle;

                if (spark.strTopic.Length > CONST_MAX_TOPIC_CHARACTERS)
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

                tile.Tag1 = ""; tile.Tag2 = ""; tile.Tag3 = ""; // initialize all of the tags to empty strings
                List<int> lstTagPks = new List<int>() { spark.FKCategories1, spark.FKCategories2, spark.FKCategories3 };
                List<Models.tags> lstTags = SparksDatabaseInterface.GetTagFileName(lstTagPks);
                int nCount = 1;
                foreach (Models.tags tag in lstTags)
                {
                    switch (nCount)
                    {
                        case 1:
                            tile.Tag1 = Utilities.GenerateImageString(tag.strImageName, ImageLocation.Tag, Server);
                            break;
                        case 2:
                            tile.Tag2 = Utilities.GenerateImageString(tag.strImageName, ImageLocation.Tag, Server);
                            break;
                        case 3:
                            tile.Tag3 = Utilities.GenerateImageString(tag.strImageName, ImageLocation.Tag, Server);
                            break;
                        default:
                            break;
                    }
                    nCount++;
                }

                lstReturn.Add(tile);
            }

            return lstReturn;
        }
    }

    public enum ImageLocation
    {
        Spark = 0,
        Category = 1,
        Tag = 2
    }
}