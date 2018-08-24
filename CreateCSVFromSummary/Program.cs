using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CreateCSVFromSummary
{
    class Program
    {
        private static string sourceDir = @"C:\Users\Administrator.ESS-102466\Desktop\Willis_Knighton_Export";

        static void Main (string[] args)
        {
            List<string> files = Directory.GetFiles(sourceDir).ToList<string>();
            foreach (string s in files)
            {
                List<string> lines = ReadDocument(s);
                WriteDocument(lines);
            }
        }

        private static List<string> ReadDocument (string sourceFile)
        {
            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(sourceFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        private static void WriteDocument (List<string> lines)
        {
            string[] tokens = new[] { "Case Number:", "Date Opened:", "Date Closed:", "Case Owner:", "Status:", "Origin:", "Temperature:", "Reason:", "Subject:", "Description:", "Defect ID:", "Internal Summary:", "Outcome:"};
            using (StreamWriter sw = new StreamWriter(@"C:\Users\Administrator.ESS-102466\Desktop\Willis_Knighton_Export\Willis_Knighton.csv", append: true))
            {
                bool lookForBreak = false;
                foreach (string s in lines)
                {
                    if (lookForBreak)
                    {
                        if (s.Equals("<br/>"))
                        {
                            sw.Write(",");
                            lookForBreak = false;
                        }
                        else if (!String.IsNullOrWhiteSpace(s))
                        {
                            sw.Write(s);
                        }
                    }
                    if (!lookForBreak)
                    {
                        foreach (string t in tokens)
                        {
                            if (s.Contains(t))
                            {
                                lookForBreak = true;
                            }
                        }
                    }
                }
                sw.WriteLine();
            }
        }
    }
}
