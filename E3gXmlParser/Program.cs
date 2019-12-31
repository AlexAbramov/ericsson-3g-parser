using Geomethod.Etl;
using Geomethod.Etl.E3g;
using System;
using System.Configuration;
using System.IO;

namespace E3gXmlParser
{
    enum ParserMode { None=0, Pm, Cm}
    enum ErrorCode { None = 0, WrongArgs=1, FileNotFound, Exception, InvalidFilename, UnknownError }

    class Test
    {
        public static string[] pmArgs = { "/pm", "e3gPm_input.xml" };
        public static string[] cmArgs = { "/cm", "e3gCm_input.xml" };
    }

    class Program
    {
        static int Main(string[] args)
        {
            var program = new Program();

            if (args.Length==0 && System.Diagnostics.Debugger.IsAttached)
            {
//                args = Test.pmArgs;
                args = Test.cmArgs;
            }

            try
            {
                if(program.ParseArgs(args))
                {
                    program.ParseFile();
                }
                else
                {
                    program.SetError(ErrorCode.WrongArgs, "Incorrect program arguments.");
                    Console.WriteLine("Usage: E3gXmlParser.exe [/cm | /pm] source");
                    Console.WriteLine("  source - input xml file path");
                }
            }
            catch (Exception ex)
            {
                program.SetError(ex);
            }

            if(program.errorCode!=ErrorCode.None && program.errorCode != ErrorCode.WrongArgs)
            {
                Console.WriteLine(string.Format("Failed with error code {0:d} ({0}): {1}", program.errorCode, program.errorMessage));
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            return (int) program.errorCode;
        }

        #region Fields
        // Args
        ParserMode parserMode;
        string fileName;
        // Config
        int consoleOutputRowCount = int.Parse(ConfigurationManager.AppSettings.Get("consoleOutputRowCount"));
        // Status
        ErrorCode errorCode;
        string errorMessage;

        public bool HasError { get { return errorCode != ErrorCode.None; } }
        #endregion

        Program()
        {
        }

        bool ParseArgs(string[] args)
        {
            foreach (var p in args)
            {
                if (p.StartsWith("/"))
                {
                    switch (p)
                    {
                        case "/pm": parserMode = ParserMode.Pm; break;
                        case "/cm": parserMode = ParserMode.Cm;  break;
                        default: return false;
                    }
                }
                else
                {
                    if (fileName != null) return false;
                    fileName = p;
                }
            }
            return parserMode!=ParserMode.None && !string.IsNullOrWhiteSpace(fileName);
        }

        void ParseFile()
        {
            var startTime = DateTime.Now;
            var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var filePath = baseDir + fileName;

            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found: " + filePath);

            var csvConverter = new CsvConverter();

            var parser = CreateParser(parserMode);
            parser.Parse(filePath);
            Console.WriteLine("Done in {0} sec", (DateTime.Now - startTime).TotalSeconds);

            foreach (var t in parser.ResultSet.Tables)
            {
                Console.WriteLine();
                Console.WriteLine(string.Format("Table: {0} RowCount: {1}", t.Name, t.RowCount));
                Console.WriteLine(csvConverter.ToCsvLine(t.Columns));
                for (var i = 0; i < Math.Min(consoleOutputRowCount, t.RowCount); i++) Console.WriteLine(csvConverter.ToCsvLine(t.GetRow(i)));

                filePath = baseDir + string.Format("{0}-{1}.csv", parser.Name, t.Name);
                using (var sw= new StreamWriter(filePath))
                {
                    t.ToCsv(sw);
                }
            }
            Console.WriteLine();

            /*                while(parser.StartTable())
                            {
                                Console.WriteLine("--TABLE--");
                                var s = csvConverter.ToCsvLine(parser.Columns);
                                Console.WriteLine(s);
                                var outputFileName = "pm_output" + parser.TableCount + ".csv";
                                using (var streamWriter = new StreamWriter(baseDir+ outputFileName))
                                {
                                    streamWriter.WriteLine(s);
                                    while (parser.ReadRow())
                                    {
                                        s = csvConverter.ToCsvLine(parser.Row);
                                        if (parser.RowCount <= consoleOutputRowCount)
                                        {
                                            Console.WriteLine("--ROW--");
                                            Console.WriteLine(s);
                                        }
                                        streamWriter.WriteLine(s);
                                    }
                                }
                                Console.WriteLine("RowCount: " + parser.RowCount);
                            }
                            Console.WriteLine("TableCount: " + parser.TableCount);
                            Console.WriteLine("TotalRowCount: " + parser.TotalRowCount);
                        }*/
        }

        private XmlParser CreateParser(ParserMode parserMode)
        {
            switch(parserMode)
            {
                case ParserMode.Pm:
                    {
                        var columns = ConfigurationManager.AppSettings.Get("pm_columns");
                        var counters = ConfigurationManager.AppSettings.Get("pm_counters");
                        return new E3gPmXmlParser(columns, counters);
                    }
                case ParserMode.Cm:
                    {
                        var columns = ConfigurationManager.AppSettings.Get("cm_columns");
                        var parameters = ConfigurationManager.AppSettings.Get("cm_parameters");
                        return new E3gCmXmlParser(columns, parameters);
                    }
                default:
                    throw new NotImplementedException("CreateParser parserMode: "+ parserMode);
            }
        }

        void SetError(Exception ex) {
            var ec = ErrorCode.Exception;
            SetError(ec, ex.GetType().Name + ": " + ex.Message);
        }

        void SetError(ErrorCode errorCode, string errorMessage)
        {
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
        }

        /*       private void TestCmParser()
               {
                   Console.WriteLine("--CM--");
                   var startTime = DateTime.Now;
                   var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
                   var filePath = baseDir + ConfigurationManager.AppSettings.Get("cm_input");
                   var columns = ConfigurationManager.AppSettings.Get("cm_columns");
                   var parameters = ConfigurationManager.AppSettings.Get("cm_parameters");
                   var csvConverter = new CsvConverter();
                   using (var parser = new E3gCmXmlParser(filePath, columns, parameters))
                   {
                       while (parser.StartTable())
                       {
                           Console.WriteLine("--TABLE--");
                           var s = csvConverter.ToCsvLine(parser.Columns);
                           Console.WriteLine(s);
                           var outputFileName = "pm_output" + parser.TableCount + ".csv";
                           using (var streamWriter = new StreamWriter(baseDir + outputFileName))
                           {
                               streamWriter.WriteLine(s);
                               while (parser.ReadRow())
                               {
                                   s = csvConverter.ToCsvLine(parser.Row);
                                   if (parser.RowCount <= consoleOutputRowCount)
                                   {
                                       Console.WriteLine("--ROW--");
                                       Console.WriteLine(s);
                                   }
                                   streamWriter.WriteLine(s);
                               }
                           }
                           Console.WriteLine("RowCount: " + parser.RowCount);
                       }
                       Console.WriteLine("TableCount: " + parser.TableCount);
                       Console.WriteLine("TotalRowCount: " + parser.TotalRowCount);
                   }
                   Console.WriteLine("Done in {0} sec", (DateTime.Now - startTime).TotalSeconds);
               }
       */
    }
}
