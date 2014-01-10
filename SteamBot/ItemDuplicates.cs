using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Text;
using System.IO;


namespace SteamTrade
{
    public class ItemDuplicates
    {
        public static ItemDuplicates FetchDuplicates()
        {
            var url = "http://backpack.tf/api/IGetDupes/v1/";

            string cachefile = "backpacktf_duplicates.cache";
            int cachethreshold = 24;     // 24 Hours of caching duplicates
            string result;

            HttpWebResponse response = Request(url, "GET");

            DateTime DupesLastModified = DateTime.Parse(response.Headers["Date"]);

            if (!System.IO.File.Exists(cachefile) || (!isCacheCurrent(cachefile, cachethreshold)))
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
                File.WriteAllText(cachefile, result);
                System.IO.File.SetCreationTime(cachefile, DupesLastModified);
            }
            else
            {
                TextReader reader = new StreamReader(cachefile);
                result = reader.ReadToEnd();
                reader.Close();
            }
            response.Close();

            ItemDuplicatesResult dupeResult = JsonConvert.DeserializeObject<ItemDuplicatesResult>(result);
            return dupeResult.Response ?? null;
        }

        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("current_time")]
        public long CurrentTime { get; set; }

        [JsonProperty("original_ids")]
        public ulong[] DuplicateIds { get; set; }

       /// <summary>
        /// Find an if an item appears in a known duplicate item list
        /// </summary>
        public bool isDuplicate(ulong originalid)
        {
            int pos = Array.IndexOf(DuplicateIds, originalid);
            if (pos > -1)
            {
                return true;
            }
            return false;
        }


        private static bool isCacheCurrent(string cachefile, int hours)
        {
            return DateTime.Now <= System.IO.File.GetCreationTime(cachefile).AddHours(hours);
        }

        public static HttpWebResponse Request(string url, string method, NameValueCollection data = null, CookieContainer cookies = null, bool ajax = true)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = method;

            request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Host = "backpack.tf";
            request.UserAgent = "DuplicateItemDetectionBot";
            request.Referer = "https://github.com/FunkyLoveCow/CodeSnippets";

            if (ajax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("X-Prototype-Version", "1.7");
            }

            // Cookies
            request.CookieContainer = cookies ?? new CookieContainer();

            // Request data
            if (data != null)
            {
                string dataString = String.Join("&", Array.ConvertAll(data.AllKeys, key =>
                    String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))
                )
                );

                byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
                request.ContentLength = dataBytes.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            // Get the response
            return request.GetResponse() as HttpWebResponse;
        }

        public class ItemDuplicatesResult
        {
            [JsonProperty("response")]
            public ItemDuplicates Response { get; set; }
        }
    }
}