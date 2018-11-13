using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
namespace DynamicXMLParser
{
    class Program
    {
        private static readonly string CANCEL_MESSAGE = "\nScript Cancelled, press any key to exit...";
        private static readonly string NAMESPACE = "com/thomsonreuters/schemas/person-report";
        private static readonly string OUTPUT_FILE_NAME = "output.csv";
        private static readonly string CLEAR_SECTION_RESULTS = "ClearSectionResults";
        private static readonly string SECTION_DETAILS = "SectionDetails";
        private static readonly string FILE_NAME = "File Name";

        [STAThread]
        static void Main(string[] args)
        {
            XNamespace xmlns = NAMESPACE;
            try
            {
                Console.WriteLine("Select the folder to run this script on, press any key to continue...");
                Console.ReadKey();
                string folderPath = selectFolderToProcess();
                List<string> filesToProcess = getFilesToProcess(folderPath);

                Console.WriteLine("\nSelect the legend file to run this script with, press any key to continue...");
                Console.ReadKey();
                string legendFilePath = selectLegendFile();
                Legend legend = parseLegendFile(legendFilePath);

                Console.WriteLine("\nProvide a custom name for the output file, or press enter to skip...");
                Console.Write("Output File Name: ");
                string outputfileName = formatOutputFileName(Console.ReadLine());


                processFiles(filesToProcess, legend, xmlns, folderPath, outputfileName);
                Console.WriteLine("\nScript completed, press any key to exit...");

            } catch (CancelledException e)
            {
                Console.WriteLine(CANCEL_MESSAGE);
            } catch (NoFilesException e)
            {
                Console.WriteLine(e.Message);
            }

            if (Console.ReadKey() != null)
            {
                Environment.Exit(0);
            }

        }

        private static string formatOutputFileName(string fileName)
        {
            if(fileName == null || fileName.Length == 0)
            {
                return OUTPUT_FILE_NAME;
            }
            if (fileName.EndsWith(".csv"))
            {
                return fileName;
            }
            else
            {
                return fileName + ".csv";
            }
        }

        private static void processFiles(List<string> files, Legend legend, XNamespace name, string outputFileDirectory, string outputFileName)
        {
            List<List<string>> fileLines = new List<List<string>>();
            files.ForEach(file =>
            {
                fileLines.Add(processFile(file, legend.sectionHeaders, name));
            });

            writeCSV(outputFileName, outputFileDirectory, legend.columnHeaders, fileLines);
        }

        private static List<string> processFile(string file, List<List<SectionHeader>> headers, XNamespace name)
        {

            List<string> foundValues = new List<string>();
            foundValues.Add(file.Substring(file.LastIndexOf('\\') + 1));
            headers.ForEach(sectionHeaders =>
            {
                foundValues.Add(getSpecifiedColumn(file, sectionHeaders, name));

            });

            return foundValues;

        }

        private static string getSpecifiedColumn(string file, List<SectionHeader> sectionHeaders, XNamespace nameSpace)
        {
            XmlDocument document = new XmlDocument();
            document.Load(file);

            var currentDoc = XElement.Parse(document.OuterXml);
            var enumerator = sectionHeaders.GetEnumerator();
            
            try
            {
                enumerator.MoveNext();
                currentDoc = getFirstElement(currentDoc, enumerator.Current, nameSpace);

                while (enumerator.MoveNext())
                {
                    var currentSectionHeader = enumerator.Current;
                    int index = currentSectionHeader.headerIndex;
                    XName name = currentSectionHeader.hasNameSpace ? nameSpace + currentSectionHeader.headerName : currentSectionHeader.headerName;
                    currentDoc = currentDoc.Elements(name).ToList()[currentSectionHeader.headerIndex];
                }
                return currentDoc.Value;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nFailure to retrieve column for the file " + file + "\n\tColumn: " + formatSectionHeaders(sectionHeaders));
                return "Error!";
            }

        }

        private static XElement getFirstElement(XElement document, SectionHeader firstHeader, XNamespace nameSpace)
        {
                XName name = firstHeader.hasNameSpace ? nameSpace + firstHeader.headerName : firstHeader.headerName;
                var descendent = (from element in document.Descendants(name) select element).First();
            
                return descendent;
        }


        private static List<string> getFilesToProcess(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.xml");
            if (files.Length == 0)
            {
                throw new NoFilesException("There are no XML files in the folder " + folderPath + "\n" + CANCEL_MESSAGE);
            }
            return files.ToList();
        }

        private static void writeCSV(string outputFileName, string outputFileDirectory, List<string> columnHeaders, List<List<string>> fileLines)
        {
            string pathString = Path.Combine(outputFileDirectory, outputFileName);
            FileStream fs = File.Create(pathString);
            StreamWriter fileWriter = new StreamWriter(fs);

            fileWriter.WriteLine(string.Join(",", columnHeaders));
            fileWriter.Flush();

            fileLines.ForEach(list =>
            {
                fileWriter.WriteLine(string.Join(",", list));
                fileWriter.Flush();
            });
            fileWriter.Close();
            fs.Close();
            Console.WriteLine("\nOutput created here:\t" + pathString);
        }

        private static string selectFolderToProcess()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the folder to run the script on.";
                fbd.ShowNewFolderButton = false;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                } else
                {
                    throw new CancelledException();
                }
            }
        }

        private static string selectLegendFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files | *.txt";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            else
            {
                throw new CancelledException();
            }

        }

        private static Legend parseLegendFile(string file)
        {
            Legend legend = new Legend();
            List<string> columnHeaders = new List<string>();
            List<List<SectionHeader>> sectionHeaders = new List<List<SectionHeader>>();
            StreamReader reader = null;
            try {

                reader = File.OpenText(file);
                string currentLine;
                columnHeaders.Add(FILE_NAME);
                while ((currentLine = reader.ReadLine()) != null)
                {
                    var items = currentLine.Split(' ').ToList();
                    string curColumnHeader = items[0];
                    items.RemoveAt(0);
                    columnHeaders.Add(curColumnHeader);
                    sectionHeaders.Add(items.Select(element => convertStringToSectionHeader(element)).ToList());

                }
                legend.columnHeaders = columnHeaders;
                legend.sectionHeaders = sectionHeaders;
            } catch(Exception e)
            {
                Console.WriteLine("\nUnable to open the legend file located located here: " + file + "\nEnsure it is not being used by another resource.");
                throw new CancelledException();
            } finally {
                reader.Close();
            }

            return legend;
        }


        private static SectionHeader convertStringToSectionHeader(string item)
        {
            SectionHeader header = new SectionHeader(item);
            bool edited = false;
            if (item.StartsWith(":"))
            {
                header.hasNameSpace = true;
                item = item.TrimStart(':');
                edited = true;
            }

            if (item.EndsWith(">"))
            {
                string ending = item.Substring(item.IndexOf('<'));
                ending = ending.TrimStart('<');
                ending = ending.TrimEnd('>');
                // Transform to 0 based ing
                int index = Convert.ToInt32(ending) -1;
                header.headerIndex = index;
                item = item.Substring(0, item.IndexOf('<'));
                edited = true;
            }

            if (edited)
            {
                header.headerName = item;
            }

            return header;
        }

        private static string formatSectionHeaders(List<SectionHeader> sectionHeaders)
        {
            StringBuilder builder = new StringBuilder();
            sectionHeaders.ForEach(header =>
            {
                builder.Append(header.original).Append(' ');
            });
            return builder.ToString();
        }

    }
}
