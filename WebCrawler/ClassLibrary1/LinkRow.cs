using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class LinkRow : TableEntity
    {
        public LinkRow(string key, string url, string title, DateTime published, string body, string img, string sgt)
        {
            this.PartitionKey = key;
            this.RowKey = CreateMD5(url);
            this.DatePublished = published;
            this.Title = title;
            this.URL = url;
            this.Body = body;
            this.Img = img;
            this.Suggestions = sgt;
        }

        public LinkRow() { }

        public string Title { get; set; }
        public string Suggestions { get; set; }
        public string Body { get; set; }
        public string Img { get; set; }
        public DateTime DatePublished { get; set; }
        public string URL { get; set; }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }


}
