using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace PullCRMImages
{
    class Program
    {
        private static string startUrl = "http://10.208.146.251:8083";
        //private static string startUrl = "http://localhost:8083";
        private static string fullUrl;
        //private static string apiKey = "Lx5%Eg2@";
        private static string logFile = @"C:\Users\Administrator.ESS-102466\Desktop\KB_Test\download.log";
        private static bool illegalCharResult;
        private static int documentNumber = 1;
        private static bool xmlFail;

        static void Main (string[] args)
        {
            string urlParameters = "/search?IW_FIELD_TEXT=Willis%20Knighton&IW_SORT=RELEVANCE&IW_BATCHSIZE=10&IW_INDEX=Cases";
            List<string[]> headers = new List<string[]>();
            headers.Add(new string[] { "Accept", "application/json" });
            HttpWebRequest request = BuildRequest(urlParameters, "GET", headers);
            HttpWebResponse response = ExecuteRequest(request);
            string nextPage = "";

            while (!String.Equals(nextPage, "#"))
            {
                string responseBody = ParseResponseBody(response);
                JObject jsonObject = DeserializeJson(responseBody);
                List<string> documentUrls = ParseDocumentUrls(jsonObject);
                DownloadDocuments(documentUrls);
                nextPage = GetNextPageUrl(jsonObject);
                request = BuildRequest(nextPage, "GET", headers);
                response = ExecuteRequest(request);
            }
        }

        private static HttpWebRequest BuildRequest(string urlParameters, string method, List<string[]> headers)
        {
            fullUrl = $"{startUrl}{urlParameters}";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(fullUrl);
            LogSomething(fullUrl);
            request.Method = method;
            //WebHeaderCollection requestHeaders = request.Headers;
            foreach(string[] s in headers)
            {
                if (!WebHeaderCollection.IsRestricted(s[0]))
                {
                    request.Headers.Add(s[0], s[1]);
                }
                else if (String.Equals("Accept", s[0]))
                {
                    request.Accept = "application/json";
                }
            }
            return request;
        }

        private static HttpWebResponse ExecuteRequest (HttpWebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        private static string ParseResponseBody(HttpWebResponse response)
        {
            var encoding = ASCIIEncoding.ASCII;
            string responseBody = "";
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
            {
                responseBody = reader.ReadToEnd();
            }
            //LogSomething(responseBody);
            //responseBody = ReplaceNonASCII(responseBody);
            return responseBody;
        }

        private static JObject DeserializeJson(string responseBody)
        {
            JObject jsonObject = new JObject();
            try
            {
                //LogSomething(responseBody);
                jsonObject = JsonConvert.DeserializeObject<JObject>(responseBody);
                illegalCharResult = false;
                return jsonObject;
            }
            catch (JsonReaderException jre)
            {
                //LogSomething(responseBody, jre);
                LogSomething("An illegal character was encountered, handling");
                //Environment.Exit(1);
                //return null;
                responseBody = ReplaceNonASCII(responseBody);
                responseBody = XmlToJson(responseBody);
                jsonObject = JsonConvert.DeserializeObject<JObject>(responseBody);
                illegalCharResult = true;
                return jsonObject;
            }
        }

        private static string ReplaceNonASCII(string responseBody)
        {
            //https://stackoverflow.com/questions/123336/how-can-you-strip-non-ascii-characters-from-a-string-in-c
            string asAscii = Encoding.ASCII.GetString(
            Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding(
                    Encoding.ASCII.EncodingName,
                    new EncoderReplacementFallback(string.Empty),
                    new DecoderExceptionFallback()
                    ),
                Encoding.UTF8.GetBytes(responseBody)
                )
            );
            return asAscii;
        }

        private static string HandleOtherChars(XmlException xe, string responseBody)
        {
            string badChar = xe.Message.Substring(0, xe.Message.IndexOf(" "));
            badChar = badChar.Replace("'", "");
            badChar = badChar.Replace(",", "");
            //LogSomething(badChar);
            string newString = responseBody.Replace(badChar, "");
            return newString;
        }

        private static string XmlToJson (string xml)
        {
            XmlDocument doc = new XmlDocument();
            string json = "";
            xmlFail = true;

            while (xmlFail)
            {
                try
                {
                    doc.LoadXml(xml);
                    xmlFail = false;
                }
                catch (XmlException xe)
                {
                    xmlFail = true;
                    //LogSomething("Failure converting XML to JSON, Handling");
                    xml = HandleOtherChars(xe, xml);
                }
            }

            doc.LoadXml(xml);
            json = JsonConvert.SerializeXmlNode(doc);
            return json;
        }

        private static List<string> ParseDocumentUrls(JObject jsonObject)
        {
            //LogSomething(jsonObject.ToString());
            List<string> documentUrls = new List<string>();
            if (!illegalCharResult)
            {
                documentUrls =
                (from p in jsonObject["resultlist"]["result"]
                 select (string)p["textview"]).ToList();
            }
            else
            {
                try
                {
                    documentUrls =
                    (from p in jsonObject["response"]["resultlist"]["result"]
                        select (string)p["textview"]).ToList();
                }
                catch (NullReferenceException nre)
                {
                    LogSomething(jsonObject.ToString(), nre);
                }
            }
            return documentUrls;
        }

        private static string GetNextPageUrl (JObject jsonObject)
        {
            string nextPage = "";
            if (!illegalCharResult)
            {
                nextPage = (string)jsonObject.SelectToken("nextpage");
            }
            else
            {
                nextPage = (string)jsonObject.SelectToken("$.response.nextpage");
            }
            return nextPage;
        }

        private static void DownloadDocuments (List<string> documentUrls)
        {
            List<string[]> headers = new List<string[]>();
            if (documentUrls != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    //The below line will throw an out of bounds exception on the last page if it does not also have 10 entries
                    HttpWebRequest request = BuildRequest(documentUrls[i], "GET", headers);
                    HttpWebResponse response = ExecuteRequest(request);
                    string responseBody = ParseResponseBody(response);
                    documentNumber++;
                    System.IO.File.WriteAllText(@"C:\Users\Administrator.ESS-102466\Desktop\KB_Test\test" + documentNumber.ToString() + ".html", responseBody.ToString());
                }
            }          
        }

        private static void LogSomething(string stringToLog, Exception e)
        {
            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(stringToLog);
                    sw.WriteLine();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine(e.ToString());
                    sw.WriteLine(stringToLog);
                    sw.WriteLine();
                }
            }
        }

        private static void LogSomething (string stringToLog)
        {
            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine(stringToLog);
                    sw.WriteLine();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine(stringToLog);
                    sw.WriteLine();
                }
            }
        }
    }
}
