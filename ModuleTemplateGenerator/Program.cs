using System;
using System.Collections;
using YamlDotNet.Serialization;

namespace ModuleTemplateGenerator
{
    internal class Program
    {

        //
        // Create a quick and dirty scaffold for a module, with a set number of units
        //

        
        private static void Main(string[] args)
        {
            

            // Get bare minimum data

            Console.WriteLine("Welcome to the Microsoft Learn scaffold creation tool.");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine();
            Console.Write("Please enter a title for your module. (e.g. Get Started): ");
            string moduleTitle = "";

            do
            {
                moduleTitle = Console.ReadLine();

                if (moduleTitle == "")
                {
                    Console.Write("Please enter a module title: ");
                }
            } while (moduleTitle == "");

            string defaultFilename = ConvertTitleToFilename(moduleTitle);

            // Some sanity checking in case the user entered a char that would break a filename
            var badChars = System.IO.Path.GetInvalidFileNameChars();
            defaultFilename = String.Join("_", defaultFilename.Split(badChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            Console.Write("Module filename (lowercase, no spaces (just press return if '" + defaultFilename + "' is ok): ");
            string moduleFilename = Console.ReadLine();

            if (moduleFilename == "")
            {
                moduleFilename = defaultFilename;
            }

            Console.Write("Module root UID (lowercase, no spaces (just press return if 'learn-windows' is ok): ");
            string defaultRootUID = "learn-windows";

            string moduleUID = Console.ReadLine();

            if (moduleUID == "")
            {
                moduleUID = defaultRootUID + "." + moduleFilename;
            }
            else
            {
                moduleUID = moduleUID + "." + moduleFilename;
            }

            Console.Write("Number of units (1-10):");
            int.TryParse(Console.ReadLine(), out int numberOfUnits);

            if (numberOfUnits < 1 || numberOfUnits > 10)
            {
                Console.Write("Wrong number of units. Let's say there's 1");
                numberOfUnits = 1;
            }

            // Keep track of the units, needed when exporting them
            ArrayList unitslist = new ArrayList();
            ArrayList unitFilenames = new ArrayList();
            ArrayList unitUIDs = new ArrayList();

            Console.WriteLine();

            for (int i = 0; i < numberOfUnits; i++)
            {
                Console.WriteLine();
                Console.WriteLine("Unit {0}.", i + 1);
                Console.WriteLine("------------");
                Console.WriteLine();
                string unitTitle = "";
                do
                {
                    Console.Write("Unit title (e.g. Get Setup): ");
                    unitTitle = Console.ReadLine();
                } while (unitTitle == "");

                string defaultUnitFilename = ConvertTitleToFilename(unitTitle);
                Console.Write("Unit filename (lowercase, no spaces (just press return if '" + defaultUnitFilename + "' is ok): ");
                string unitFilename = Console.ReadLine();

                if (unitFilename == "")
                {
                    unitFilename = defaultUnitFilename;
                }

                unitFilename = (i + 1).ToString() + "-" + unitFilename;

                string unitUID = moduleUID + "." + unitFilename;



                Unit unit = new Unit
                {
                    title = unitTitle,
                    uid = unitUID,
                    metadata = new Unit.Metadata
                    {
                        title = unitTitle,
                        description = "TBD",
                        ms_date = DateTime.Now.ToShortDateString(),
                        displayType = "one-Column",
                        author = "TBD",
                        ms_author = "TBD",
                        ms_topic = "interactive-tutorial",
                        ms_prod = "learning-windows",
                        ROBOTS = "NOINDEX",
                    },


                    durationInMinutes = 10, // default
                    content = "[!include[](includes/" + unitFilename + ".md)]",
                };

                unitslist.Add(unit);
                unitFilenames.Add(unitFilename);
                unitUIDs.Add(unitUID);
            }


            // Create the data structures. These are used by the yaml serializer


            Module module = new Module
            {
                uid = moduleUID,
                title = moduleTitle,
                metadata = new Module.Metadata {
                    title = moduleTitle,
                    description = "TBD",
                    ms_date = DateTime.Now.ToShortDateString(),
                    author = "TBD",
                    ms_author = "TBD",
                    ms_topic = "interactive-tutorial",
                    ms_prod = "learning-windows"},
                summary = "TBD",
                abstract_ = "TBD",
                
                iconUrl = "/learn/achievements/TBD.svg",
                prerequisites = "TBD",
                levels = new string[] { "beginner" },
                roles = new string[] { "developer" },
                products = new string[] { "windows" },

                units = unitUIDs,

                achievement = moduleUID + ".badge"
            };

            
            

            // Create the serializer

            ISerializer serializer = new SerializerBuilder().Build();


            // Save out the module yaml

            string Module_index_yaml = serializer.Serialize(module);

            // Last minute processing, as the serializer doesn't like certain characters
            // and the fields with a | and newline need this processing too.
            Module_index_yaml = "### YamlMime:Module\n" + Module_index_yaml;
            Module_index_yaml = Module_index_yaml.Replace("abstract_: TBD", "abstract: |\n In this module you will:\n - Learn stuff");
            Module_index_yaml = Module_index_yaml.Replace("prerequisites: TBD", "prerequisites: |\n - A sense of wonder");

            Module_index_yaml = Module_index_yaml.Replace("ms_date", "ms.date");
            Module_index_yaml = Module_index_yaml.Replace("ms_prod", "ms.prod");
            Module_index_yaml = Module_index_yaml.Replace("ms_topic", "ms.topic");
            Module_index_yaml = Module_index_yaml.Replace("ms_author", "ms.author");

            // Set the path and create directories common for every module

            System.IO.Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            System.IO.Directory.CreateDirectory(moduleFilename);
            System.IO.Directory.CreateDirectory(moduleFilename + "\\media");
            System.IO.Directory.CreateDirectory(moduleFilename + "\\includes");
            System.IO.File.WriteAllText(moduleFilename + "\\index.yml", Module_index_yaml);

            // Save out the Unit yaml files, and create dummy markdown files.
            // Need to replace ms_date with ms.date, again, a C# syntax thing

            int filenameCounter = 0;

            foreach (Unit unit in unitslist)
            {

                string Unit_yaml = serializer.Serialize(unit);

                // Last minute processing, as the serializer doesn't like certain characters

                Unit_yaml = "### YamlMime:ModuleUnit\n" + Unit_yaml;

                Unit_yaml = Unit_yaml.Replace("ms_date", "ms.date");
                Unit_yaml = Unit_yaml.Replace("ms_prod", "ms.prod");
                Unit_yaml = Unit_yaml.Replace("ms_topic", "ms.topic");
                Unit_yaml = Unit_yaml.Replace("ms_author", "ms.author");
       
                Unit_yaml = Unit_yaml.Replace("[!include", "|\n    [!include");
                Unit_yaml = Unit_yaml.Replace("'|", "|");
                Unit_yaml = Unit_yaml.Replace("]'", "]");

                System.IO.File.WriteAllText(moduleFilename + "\\" + unitFilenames[filenameCounter] + ".yml", Unit_yaml);

                System.IO.File.WriteAllText(moduleFilename + "\\includes\\" + unitFilenames[filenameCounter++] + ".md", "TBD");

            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("All done. You'll find the files you need here: {0}", System.IO.Directory.GetCurrentDirectory() + moduleFilename);
            Console.ReadKey();
        }

        private static string ConvertTitleToFilename(string Title)
        {
            string filename = Title;
            filename = filename.Replace("'", "");
            filename = filename.Replace(" ", "-");
            filename = filename.ToLower();
            filename = filename.Replace("the-", "");
            filename = filename.Replace("a-", "");
            filename = filename.Replace("an-", "");
            filename = filename.Replace("for-", "");

            return filename;
        }

    }

    public class Module
    {

        public class Metadata
        {
            public string title { get; set; }
            public string description { get; set; }
            public string ms_date { get; set; }
            public string author { get; set; }
            public string ms_author { get; set; }
            public string ms_topic { get; set; }
            public string ms_prod { get; set; }
            public string ROBOTS { get; set; }
        }

        public string uid { get; set; }
        public Metadata metadata { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string abstract_ { get; set; }
        public string prerequisites { get; set; }
        //public string cardDescription { get; set; }
        public string iconUrl { get; set; }
        public string[] levels { get; set; }
        public string[] roles { get; set; }
        public string[] products { get; set; }
        public ArrayList units { get; set; }
        public string achievement { get; set; }
    }

    public class Unit
    {


        public class Metadata
        {
            public string title { get; set; }
            public string description { get; set; }
            public string ms_date { get; set; }
            public string displayType { get; set; }
            public string author { get; set; }
            public string ms_author { get; set; }
            public string ms_topic { get; set; }
            public string ms_prod { get; set; }
            public string ROBOTS { get; set; }
        }

        public string title { get; set; }
        public string uid { get; set; }
        public Metadata metadata { get; set; }
        public int durationInMinutes { get; set; }
        public string content { get; set; }
    }




}
