using ModelingFramework.Core.Expressions;
using ModelingFramework.Core.Logging;
using ModelingFramework.Core.Modelica;
using ModelingFramework.Core.Numerics;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PlaygroundConsole
{
    class Program
    {

        static void ElectricalCircuit()
        {

            string input =
        @"
class RLC1
  parameter Real Vb=24;
  parameter Real L = 1;
  parameter Real R = 100;
  parameter Real C = 1e-3;
  Real V;
        Real i_L;
        Real i_R;
        Real i_C;
        equation
          V = i_R * R;
        C* der(V) = i_C;
  L* der(i_L) = (Vb-V);
  i_L=i_R+i_C;
end RLC1;";
            Simulate(input, 0.1, 2.0);

        }

        static void VanDerPol()
        {

            string input =
        @"
class VanDerPol
  parameter Real mu=0;

  Real x=2;
  Real y=0;       
  equation
    der(x)=y;
    der(y)=mu*(1-x^2)*y-x;  
end VanDerPol;";
            Simulate(input, 0.1, 50.0);

        }



        static void Simulate(string input, double step, double end)
        {
            var parser = new ModelicaParser();
            var printer = new ModelicaTreePrinter();
            var translator = new ModelTranslatorV1();

            var status = parser.TryParseProgram(input, out var prog, out var error, out var position);
            if (status)
            {
                var ast = printer.Transform(prog);
                Console.WriteLine(ast);
                var logger = new ColoredConsoleLogger();
                var model = translator.Translate(prog.ClassDefinitions.Last());

                var integrator = new ImplicitEuler();
                integrator.Discretize(model);
                integrator.StepSize = step;
                integrator.EndTime = end;
                model.Initialize(new NoLogger());

                Stopwatch w = new Stopwatch();
                w.Start();
                var results = integrator.Integrate(model, logger);
                w.Stop();
                Console.WriteLine("Integration took " + w.ElapsedMilliseconds + "ms");

                using (var sw = new StreamWriter(Environment.CurrentDirectory + "\\results.csv"))
                {
                    sw.WriteLine("time;" + string.Join("; ", model.AlgebraicVariables.Select(v => v.Name)));

                    foreach (var result in results)
                    {
                        sw.WriteLine(result.ToString());
                    }
                }
            }
            else
                Console.WriteLine(error);
        }

        static void LotkaVolterraParsed()
        {
            string input =
            @"class LotkaVolterra
                    Real N1 = 10;
                    Real N2 = 5;
                    parameter Real e1 = 0.09;
                    parameter Real g1 = 0.01;
                    parameter Real e2 = 0.04;
                    parameter Real g2 = 0.01;
                    equation
                        der(N1) = N1*(e1-g1*N2);
                        der(N2) = -N2*(e2-g2*N1);                        
                 end LotkaVolterra;
                 ";

            var parser = new ModelicaParser();



            var printer = new ModelicaTreePrinter();
            var translator = new ModelTranslatorV1();

            var status = parser.TryParseProgram(input, out var prog, out var error, out var position);
            if (status)
            {
                var ast = printer.Transform(prog);
                // Console.WriteLine(ast);
                var logger = new NoLogger();
                var model = translator.Translate(prog.ClassDefinitions.Last());

                var integrator = new ExplicitEuler();
                integrator.Discretize(model);
                integrator.StepSize = 1;
                integrator.EndTime = 800;
                model.Initialize(new NoLogger());

                Stopwatch w = new Stopwatch();
                w.Start();
                var results = integrator.Integrate(model, logger);
                w.Stop();
                Console.WriteLine("Integration took " + w.ElapsedMilliseconds + "ms");

                using (var sw = new StreamWriter(Environment.CurrentDirectory + "\\results.csv"))
                {
                    sw.WriteLine("time;" + string.Join("; ", model.AlgebraicVariables.Select(v => v.Name)));

                    foreach (var result in results)
                    {
                        sw.WriteLine(result.ToString());
                        // Console.WriteLine(result.ToString());
                    }
                }
            }
            else
                Console.WriteLine(error);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }

        static void Parse(string input)
        {
            var parser = new ModelicaParser();
            var printer = new ModelicaTreePrinter();
            

            var status = parser.TryParseProgram(input, out var prog, out var error, out var position);
            if (status)
            {
                var ast = printer.Transform(prog);
                Console.WriteLine(ast);
            }
            else
                Console.WriteLine(error);
        }

        static void LoktaVolterraHardoded()
        {
            var logger = new NoLogger();
            var model = new DAEProblem();
            var integrator = new ExplicitEuler();
            integrator.StepSize = 0.1;
            integrator.EndTime = 200;

            var N1 = new Variable("N1", 5);
            var N2 = new Variable("N2", 10);
            var dN1 = new Variable("dN1dt", 0);
            var dN2 = new Variable("dN2dt", 0);
            var e1 = new Variable("e1", 0.09);
            var g1 = new Variable("g1", 0.01);
            var e2 = new Variable("e2", 0.04);
            var g2 = new Variable("g2", 0.01);

            var eq1 = new Equation(N1 * (e1 - g1 * N2) - dN1);
            var eq2 = new Equation(-N2 * (e2 - g2 * N1) - dN2);


            model.AlgebraicVariables.Add(N1);
            model.AlgebraicVariables.Add(N2);
            model.DifferentialVariables.Add(dN1);
            model.DifferentialVariables.Add(dN2);
            model.Equations.Add(eq1);
            model.Equations.Add(eq2);
            model.Mapping.Add(dN1, N1);
            model.Mapping.Add(dN2, N2);
            model.Initialize(new NoLogger());

            var results = integrator.Integrate(model, logger);

            using (var sw = new StreamWriter(Environment.CurrentDirectory + "\\results.csv"))
            {
                sw.WriteLine("time;" + string.Join("; ", model.AlgebraicVariables.Select(v => v.Name)));

                foreach (var result in results)
                {
                    sw.WriteLine(result.ToString());
                }
            }


        }
        static void Main(string[] args)
        {
            #region Old Stuff
            /* Console.WriteLine("Hello World!");

             string input = "True";


             var ast = Parser.Boolean.Parse(input);

             Console.WriteLine(ast.Value);


             string input2 = "\"a string\"";

             var ast2 = Parser.ModelicaString.Parse(input2);
             Console.WriteLine(ast2.Value);


             string input4 = "7";

             var ast4 = Parser.Number.Parse(input4);

             Console.WriteLine(ast4.Value);


             string input5 =
              "class Test" + System.Environment.NewLine +
              "   Complex a(re=14,im=2);" + System.Environment.NewLine +
              "end Test;";

             var ast5 = Parser.ClassDefinition.Parse(input5);

             Console.WriteLine(ast5.Elements.First().ToString());*/

            /* var input=@"class Ele1000 = Ele(Resistor.r=1000);
                   class Ele
                     class Resistor
                         Real r = 1;
                     end Resistor;
                     class Circuit
                         Resistor r1;
                         Ele.Resistor r2;
                     end Circuit;
                 end Ele;";
                 */
            #endregion

            //LoktaVolterraHardoded();

            // LotkaVolterraParsed();

            // VanDerPol();

            //ElectricalCircuit();

            Parse("type Voltage = Real(unit = \"V\");");
            Console.ReadLine();
        }
    }
}
