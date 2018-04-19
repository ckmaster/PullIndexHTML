using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RemoveExtraTags
{

    class Program
    {
        private static string sourceDir = @"C:\Users\Administrator.ESS-102466\Desktop\RawOutput";

        static void Main (string[] args)
        {
            List<string> files = Directory.GetFiles(sourceDir).ToList<string>();
            foreach (string s in files)
            {
                List<string> lines = ReadDocument(s);
                string newName = s.Replace(".html", "_new.html");
                WriteDocument(lines, newName);
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

        private static void WriteDocument (List<string> lines, string newName)
        {
            using (StreamWriter sw = new StreamWriter(newName))
            {
                for (int i = 6; i < lines.Count - 4; i++)
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }
    }
}
