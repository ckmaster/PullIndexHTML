/*
   Copyright (c) 1988-2017, Perceptive Software. All rights reserved.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
   WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
   DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
   ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
   LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
   ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Perceptive.DocumentFilters;
using System.Linq;

namespace PerceptiveDocumentFilters
{

    class Program
    {
        private DocumentFilters m_filters;
        private string m_outputFolder;
        private TextWriter m_stdout;
        private TextWriter m_stderr;

        public Program ()
        {
            m_stdout = System.Console.Out;
            m_stderr = System.Console.Error;
            m_outputFolder = ".";
        }

        public void Run (string[] args)
        {
            //if (args.Length == 0)
            //{
            //    ShowHelp();
            //    return;
            //}

            m_filters = new DocumentFilters();
            m_filters.Initialize(PerceptiveDocumentFiltersLicense.LICENSE_KEY, ".");
            string sourceDir = @"C:\Users\Administrator.ESS-102466\Desktop\PureHTML";

            //List<string> fileList = new List<string>();
            List<string> fileList = Directory.GetFiles(sourceDir).ToList<string>();
            for (int i = 0; i < args.Length; i++)
            {
                String arg = args[i];

                if (String.Compare(arg, "--output", true) == 0 || String.Compare(arg, "-o", true) == 0)
                {
                    m_outputFolder = args[++i];
                }
                else if (String.Compare(arg, "-h", true) == 0 || String.Compare(arg, "--help", true) == 0)
                {
                    ShowHelp();
                    return;
                }
                //else
                //{
                //    fileList.Add(arg);
                //}
            }

            foreach (string filename in fileList)
            {
                ProcessFile(filename, m_filters.GetExtractor(filename));
            }

            m_stdout.Close();
        }

        private void ProcessFile (string filename, Extractor item)
        {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);
            string outputFolder = m_outputFolder + "\\" + fileNameWithoutExtension;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            string destination = System.IO.Path.Combine(outputFolder, fileNameWithoutExtension + ".html");

            m_stderr.WriteLine("Processing " + filename);
            try
            {
                item.Open(isys_docfilters.IGR_BODY_AND_META | isys_docfilters.IGR_FORMAT_IMAGE, "HTTP_EXTERNAL_IMAGES=On");

                Perceptive.DocumentFilters.Canvas canvas = m_filters.MakeOutputCanvas(destination, isys_docfilters.IGR_DEVICE_HTML, "HTTP_EXTERNAL_IMAGES=On");
                try
                {
                    for (int pageIndex = 0; pageIndex < item.GetPageCount(); pageIndex++)
                    {
                        Perceptive.DocumentFilters.Page page = item.GetPage(pageIndex);
                        try
                        {
                            canvas.RenderPage(page);

                            // Embedded Images
                            SubFile image = page.GetFirstImage();
                            while (image != null)
                            {
                                image.CopyTo(System.IO.Path.Combine(outputFolder, image.getName()));
                                image = page.GetNextImage();
                            }
                        }
                        finally
                        {
                            page.Close();
                        }
                    }
                }
                finally
                {
                    canvas.Close();
                }
            }
            catch (Exception e)
            {
                m_stderr.WriteLine("Error Processing " + filename);
                m_stderr.WriteLine("   - " + e.ToString());
            }
            finally
            {
                item.Close();
            }
        }

        private void ShowHelp ()
        {
            System.Console.WriteLine("Perceptive Document Filters 11: ConvertDocumentToHDHTML C# Example");
            System.Console.WriteLine("(c)1988-2017 Perceptive Software");
            System.Console.WriteLine("");
            System.Console.WriteLine("ConvertDocumentToHDHTML [options] filename");
            System.Console.WriteLine("");
            System.Console.WriteLine("options");
            System.Console.WriteLine(" -h, --help                this help");
            System.Console.WriteLine(" -o, --output [folder]     the folder to save the output files, defaults to current directory");
        }

        static void Main (string[] args)
        {
            Program prog = new Program();
            prog.Run(args);
        }
    }

}
