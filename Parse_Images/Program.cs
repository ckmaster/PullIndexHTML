using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

namespace Parse_Images
{
    class Program
    {
        private static string sourceDir = @"C:\Users\Administrator.ESS-102466\Desktop\PureHTML\";
        private static string newDir = @"C:\Articles\";
        private static List<string> lines;

        static void Main (string[] args)
        {
            List<string> files = Directory.GetFiles(sourceDir).ToList<string>();
            foreach (string s in files)
            {
                lines = ReadDocument(s);
                string title = GetArticleTitle(lines[0]);
                MakeDirectory(title);
                List<string> urls = GetUrls();
                WriteDocument(title);
                if (urls.Count > 0)
                {
                    DownloadImages(urls, title);
                }
            }
        }

        private static void WriteDocument (string title)
        {
            string article = $"{newDir}\\{title}\\article.html";
            using (StreamWriter sw = new StreamWriter(article))
            {
                foreach (string s in lines)
                {
                    sw.WriteLine(s);
                }
            }
        }

        private static void DownloadImages(List<string> urls, string title)
        {
            List<string[]> headers = new List<string[]>();
            //headers.Add(new string[] { "Accept", "application/json" });
            //foreach (string url in urls)
            for (int i = 0; i < urls.Count; i++)
            {
                if (!String.Equals(urls[i], ""))
                {
                    HttpWebRequest request = BuildRequest(urls[i], "GET", headers);
                    HttpWebResponse response = ExecuteRequest(request);
                    Stream body = ParseResponseBody(response);
                    using (Stream output = File.OpenWrite($"{newDir}\\{title}\\image{i.ToString()}.png"))
                    {
                        body.CopyTo(output);
                    }
                }
            }
        }

        private static HttpWebRequest BuildRequest (string url, string method, List<string[]> headers)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            foreach (string[] s in headers)
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

        private static Stream ParseResponseBody (HttpWebResponse response)
        {
            Stream body = response.GetResponseStream();
            return body;
        }

        private static List<string> ReadDocument (string sourceFile)
        {
            List<string> tempLines = new List<string>();
            using (StreamReader sr = new StreamReader(sourceFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    tempLines.Add(line);
                }
            }
            return tempLines;
        }

        private static string GetArticleTitle(string firstLine)
        {
            Regex regex = new Regex("(?<=<h1>)([\\s\\S]*?)(?=<\\/h1>)");
            Match match = regex.Match(firstLine);
            string title = match.ToString();
            title = title.Replace("\\", "");
            title = title.Replace("/", "");
            title = title.Replace(":", "");
            title = title.Replace("*", "");
            title = title.Replace("?", "");
            title = title.Replace("\"", "");
            title = title.Replace("<", "");
            title = title.Replace(">", "");
            title = title.Replace("|", "");
            title = title.Left(220);
            return title;
        }

        private static List<string> GetUrls()
        {
            List<string> urls = new List<string>();
            //foreach(string s in lines)
            for (int i = 0; i < lines.Count; i++)
            {
                int index = lines[i].IndexOf("https://kfxadhna1.blob.core.windows.net");
                if (index != -1)
                {
                    //string regString = "(?<=src=\")([\\s\\S] *?)(?= \"><\\/img>)";
                    //regString = $@"{Regex.Escape(regString)}";
                    //Regex regex = new Regex(regString);
                    //Match match = regex.Match(s);
                    //urls.Add(match.ToString());

                    char[] chars = lines[i].ToCharArray();
                    List<char> lineChar = chars.ToList();
                    List<char> urlChar = new List<char>();
                    string url = "";

                    for (int j = index; j < chars.Length; j++)
                    {
                        urlChar.Add(chars[j]);
                        if (urlChar.Count > 4)
                        {
                            if (String.Equals($"{urlChar[urlChar.Count-4]}{urlChar[urlChar.Count - 3]}{urlChar[urlChar.Count - 2]}{urlChar[urlChar.Count - 1]}", ".png"))
                            {
                                url = string.Join("", urlChar.ToArray());
                                lines[i] = SetLocalImageReference(lineChar, urls.Count, index, j, ".png");
                                break;
                            }
                            else if (String.Equals($"{urlChar[urlChar.Count - 4]}{urlChar[urlChar.Count - 3]}{urlChar[urlChar.Count - 2]}{urlChar[urlChar.Count - 1]}", ".gif"))
                            {
                                url = string.Join("", urlChar.ToArray());
                                lines[i] = SetLocalImageReference(lineChar, urls.Count, index, j, ".gif");
                                break;
                            }
                        }
                    }
                    urls.Add(url);
                }
            }
            return urls;
        }

        private static string SetLocalImageReference(List<char> lineChar, int imageNumber, int beginUrl, int endUrl, string extension)
        {
            string newLine = "";
            lineChar.RemoveRange(beginUrl, (endUrl - (beginUrl -1)));
            string filename = $"image{imageNumber}{extension}";
            lineChar.InsertRange(beginUrl, filename);
            newLine = string.Join("", lineChar.ToArray());
            return newLine;
        }

        private static void MakeDirectory(string title)
        {
            //string title = GetArticleTitle(lines[0]);
            if (!Directory.Exists(newDir + title))
            {
                Directory.CreateDirectory(newDir + title);
            }
        }
    }

    public static class StringExtensions
    {
        public static string Left (this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
    }

}
