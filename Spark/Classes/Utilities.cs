﻿using System;
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
        public static bool WriteImageToFile(String strFileName, HttpRequestBase Request, HttpServerUtilityBase Server)
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
    }
}