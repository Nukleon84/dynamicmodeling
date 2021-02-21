using ModelingFramework.Core.Logging;
using ModelingFramework.Core.Modelica;
using ModelingFramework.Core.Numerics;
using System;
using System.IO;
using System.Linq;
namespace Apps.ModelicaCompliance
{
    class Program
    {
        string searchPath = @"G:\Modelica\Modelica-Compliance\ModelicaCompliance";
        string logFile = "output.log";

        void Explore(ModelicaTestElement node)
        {
            var subdirs = Directory.EnumerateDirectories(node.Path);
            foreach (var subdir in subdirs)
            {
                var di = new DirectoryInfo(subdir);
                var child = new ModelicaTestElement() { Name = di.Name, Path = subdir, ElementType = ModelicaTestElementType.Folder };
                Explore(child);
                node.Children.Add(child);
            }

            var models = Directory.EnumerateFiles(node.Path, "*.mo");
            foreach (var model in models)
            {
                var child = new ModelicaTestElement() { Name = Path.GetFileName(model), Path = model, ElementType = ModelicaTestElementType.Model };
                node.Children.Add(child);
            }
        }


        void WriteInColor(string text, ConsoleColor color, StreamWriter sw)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            sw.Write(text);
            Console.ForegroundColor = oldColor;
        }

        void Print(ModelicaTestElement node, StreamWriter sw, int depth = 0)
        {


            var parsedString = $"{node.PercentParsed :P2}";
            var translatedString = $"{node.PercentTranslated:P2}";
            var solvedString = $"{node.PercentSolved :P2}";

            //Console.WriteLine($"{path,-60} {parsedString,10} {translatedString,10} {solvedString,10} ");

            if (node.ElementType == ModelicaTestElementType.Folder)
            {
                var path = $"{new string(' ', depth)}[+] {node.Name}";
                WriteInColor($"{path,-60}", ConsoleColor.DarkYellow, sw);
                WriteInColor($"{node.TotalSubelements,-10}", ConsoleColor.DarkYellow, sw);
                if (node.PercentParsed > 0.95)
                    WriteInColor($"{parsedString,10}", ConsoleColor.Green, sw);
                else if (node.PercentParsed > 0.50)
                    WriteInColor($"{parsedString,10}", ConsoleColor.Yellow, sw);
                else
                    WriteInColor($"{parsedString,10}", ConsoleColor.Red, sw);

                if (node.PercentTranslated > 0.95)
                    WriteInColor($"{translatedString,10}", ConsoleColor.Green, sw);
                else if (node.PercentTranslated > 0.5)
                    WriteInColor($"{translatedString,10}", ConsoleColor.Yellow, sw);
                else
                    WriteInColor($"{translatedString,10}", ConsoleColor.Red, sw);


                if (node.PercentSolved > 0.95)
                    WriteInColor($"{solvedString,10}", ConsoleColor.Green, sw);
                else if (node.PercentSolved > 0.5)
                    WriteInColor($"{solvedString,10}", ConsoleColor.Yellow, sw);
                else
                    WriteInColor($"{solvedString,10}", ConsoleColor.Red, sw);


                Console.WriteLine();
                sw.WriteLine();

            }
            if (node.ElementType == ModelicaTestElementType.Model)
            {
                var path = $"{new string(' ', depth)}{node.Name}";
                WriteInColor($"{path,-60}", ConsoleColor.White, sw);
                WriteInColor($"{"",-10}", ConsoleColor.White, sw);
                if (node.PercentParsed > 0.95)
                    WriteInColor($"{parsedString,10}", ConsoleColor.Green, sw);
                else if (node.PercentParsed > 0.50)
                    WriteInColor($"{parsedString,10}", ConsoleColor.Yellow, sw);
                else
                    WriteInColor($"{parsedString,10}", ConsoleColor.Red, sw);

                if (node.PercentTranslated > 0.95)
                    WriteInColor($"{translatedString,10}", ConsoleColor.Green, sw);
                else if (node.PercentTranslated > 0.5)
                    WriteInColor($"{translatedString,10}", ConsoleColor.Yellow, sw);
                else
                    WriteInColor($"{translatedString,10}", ConsoleColor.Red, sw);


                if (node.PercentSolved > 0.95)
                    WriteInColor($"{solvedString,10}", ConsoleColor.Green, sw);
                else if (node.PercentSolved > 0.5)
                    WriteInColor($"{solvedString,10}", ConsoleColor.Yellow, sw);
                else
                    WriteInColor($"{solvedString,10}", ConsoleColor.Red, sw);

                Console.WriteLine();
                sw.WriteLine();
            }

          
            

            foreach (var child in node.Children)
                Print(child, sw, depth + 4);
        }


        void Test(ModelicaTestElement node)
        {
            if (node.ElementType == ModelicaTestElementType.Folder)
            {
                foreach (var child in node.Children)
                    Test(child);

                node.TotalSubelements = node.Children.Sum(n => n.TotalSubelements);

                if (node.Children.Count > 0 && node.TotalSubelements>0)
                {
                    node.PercentParsed = node.Children.Sum(n => n.PercentParsed*n.TotalSubelements)/node.TotalSubelements;
                    node.PercentTranslated = node.Children.Sum(n => n.PercentTranslated * n.TotalSubelements) / node.TotalSubelements;
                    node.PercentSolved = node.Children.Sum(n => n.PercentSolved * n.TotalSubelements) / node.TotalSubelements;
                }
                else
                {
                    node.PercentParsed = 1.0;
                    node.PercentTranslated = 1.0;
                    node.PercentSolved = 1.0;
                }
                
            }
            else
            {
                node.TotalSubelements = 1;
                var input = File.ReadAllText(node.Path);
                var parser = new ModelicaParser();
                try
                {
                    var status = parser.TryParseProgram(input, out var prog, out var error, out var position);
                    if (status)
                    {
                        node.PercentParsed = 1.0;
                        var translator = new ModelTranslatorV1();
                                               
                        var model = translator.Translate(prog.ClassDefinitions.Last());
                        node.PercentTranslated = 1.0;

                        var integrator = new ImplicitEuler();
                        integrator.StepSize = 0.1;
                        integrator.EndTime = 0.1;
                        var logger = new NoLogger();
                        integrator.Discretize(model);                                                
                        model.Initialize(logger);
                        var results = integrator.Integrate(model, logger);
                        node.PercentSolved = 1.0;

                    }
                    else
                    {
                        Console.WriteLine($"Error parsing file {node.Path}");
                        Console.WriteLine(error);
                        Console.WriteLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        void Run()
        {
            var root = new ModelicaTestElement() { Name = "root", Path = searchPath, ElementType = ModelicaTestElementType.Folder };

            Explore(root);
            Test(root);
            using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "\\" + logFile))
            {
                Console.WriteLine($"{"Path",-60}{"Elements",-10}{"Parsed",-10}{"Translated",-10}{"Solved",-10} ");
                sw.WriteLine($"{"Path",-60}{"Elements",-10}{"Parsed",-10}{"Translated",-10}{"Solved",-10} ");


                Print(root, sw);
            }

        }

        static void Main(string[] args)
        {

            var app = new Program();

            app.Run();

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}
