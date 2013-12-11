using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;

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
    }
}