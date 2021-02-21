using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelingFramework.Core.Modelica;
using NUnit.Framework;


namespace TestModelicaParser
{
    public class Classes
    {
        ModelicaParser parser = new ModelicaParser();

        [SetUp]
        public void Setup()
        {
            parser = new ModelicaParser();


        }



        [Test]
        public void CanPassLevel1Reference()
        {
            string input = "Test";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test", ast.ID);
            Assert.AreEqual(null, ast.Next);
        }

        [Test]
        public void CanPassLevel1ReferenceNumber()
        {
            string input = "Test1";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test1", ast.ID);
            Assert.AreEqual(null, ast.Next);
        }
        [Test]
        public void CanPassLevel1ReferenceNumberUnderscore()
        {
            string input = "Test_1";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test_1", ast.ID);
            Assert.AreEqual(null, ast.Next);
        }

        [Test]
        public void CanPassLevel2Reference()
        {
            string input = "Test.Demo";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test", ast.ID);
            Assert.NotNull(ast.Next);
            Assert.AreEqual("Demo", ast.Next.ID);
        }
        [Test]
        public void CanPassLevel2ReferenceNumberUnderscore()
        {
            string input = "Test_1.Demo2";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test_1", ast.ID);
            Assert.NotNull(ast.Next);
            Assert.AreEqual("Demo2", ast.Next.ID);
        }
        [Test]
        public void CanPassLevel3Reference()
        {
            string input = "Test.Demo.TryThat";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test", ast.ID);
            Assert.NotNull(ast.Next);
            Assert.AreEqual("Demo", ast.Next.ID);
            Assert.NotNull(ast.Next.Next);
            Assert.AreEqual("TryThat", ast.Next.Next.ID);
        }
        [Test]
        public void CanPassLevel4Reference()
        {
            string input = "Test.Demo.TryThat.AndThat";

            var ast = parser.ParseReference(input);
            Assert.AreEqual("Test", ast.ID);
            Assert.NotNull(ast.Next);
            Assert.AreEqual("Demo", ast.Next.ID);
            Assert.NotNull(ast.Next.Next);
            Assert.AreEqual("TryThat", ast.Next.Next.ID);
            Assert.NotNull(ast.Next.Next.Next);
            Assert.AreEqual("AndThat", ast.Next.Next.Next.ID);

        }

        [Test]
        public void CanParseEmptyClass()
        {
            string input =
                "class Test" + System.Environment.NewLine +
                "end Test;";

            var ast = parser.ParseClassDefinition(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Test", ast.ID);
        }

        [Test]
        public void CanParseNestedClass()
        {
            string input =
                "class Test" + System.Environment.NewLine +
                "   class a" + System.Environment.NewLine +
                "   end a;" + System.Environment.NewLine +
                "end Test;";

            var ast = parser.ParseClassDefinition(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Test", ast.ID);

            var cd = ast.Elements.First() as ClassDefinition;
            Assert.NotNull(cd);
            Assert.AreEqual("a", cd.ID);

        }
        [Test]
        public void CanParseSimpleClass()
        {
            string input =
                "class Test" + System.Environment.NewLine +
                "   Real a;" + System.Environment.NewLine +
                "end Test;";

            var ast = parser.ParseClassDefinition(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Test", ast.ID);

            var cd = ast.Elements.First() as ComponentClause;
            Assert.NotNull(cd);
            Assert.AreEqual("a", cd.Declarations.First().ID);
            Assert.AreEqual("Real", cd.Type.ID);
        }

        [Test]
        public void CanParseSimple2Class()
        {
            string input =
                "class Test" + System.Environment.NewLine +
                "   Real a;" + System.Environment.NewLine +
                "   Real b;" + System.Environment.NewLine +
                "end Test;";

            var ast = parser.ParseClassDefinition(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Test", ast.ID);

            var cd = ast.Elements.First() as ComponentClause;
            Assert.NotNull(cd);
            Assert.AreEqual("a", cd.Declarations.First().ID);
            Assert.AreEqual("Real", cd.Type.ID);

            var cd2 = ast.Elements.Last() as ComponentClause;
            Assert.NotNull(cd2);
            Assert.AreEqual("b", cd2.Declarations.First().ID);
            Assert.AreEqual("Real", cd2.Type.ID);

        }

        [Test]
        public void CanParseSimpleClassWithModificationAssignedValue([Values("a=42", "a =42", "a= 42", "a = 42")]  string variableAssignment)
        {
            string input =
                "class Test" + System.Environment.NewLine +
                "   Real " + variableAssignment + ";" + System.Environment.NewLine +
                "end Test;";

            var ast = parser.ParseClassDefinition(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Test", ast.ID);

            var cd = ast.Elements.First() as ComponentClause;
            Assert.NotNull(cd);
            Assert.AreEqual("a", cd.Declarations.First().ID);
            Assert.AreEqual("Real", cd.Type.ID);
            var assignedVal = cd.Declarations.First().Modification.Value as DoubleLiteral;
            Assert.NotNull(assignedVal);
            Assert.AreEqual(42, assignedVal.Value);
        }

        [Test]
        public void CanParseComplexClass()
        {
            string input =
                "class Test" + System.Environment.NewLine +
                "   Complex a(re=14, im=0)=1;" + System.Environment.NewLine +
                "end Test;";

            var ast = parser.ParseClassDefinition(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Test", ast.ID);

            var cd = ast.Elements.First() as ComponentClause;
            Assert.NotNull(cd);
            Assert.AreEqual("a", cd.Declarations.First().ID);
            Assert.AreEqual("Complex", cd.Type.ID);
        }

        [Test]
        public void CanExampleProblemClass()
        {
            string input =
                @"class Ele1000 = Ele(Resistor.r=1000);
                  class Ele
                    class Resistor
                        Real r = 1;
                    end Resistor;
                    class Circuit
                        Resistor r1;
                        Ele.Resistor r2;
                    end Circuit;
                end Ele;";

            var ast = parser.ParseProgram(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Ele1000", ast.ClassDefinitions.First().ID);
            Assert.AreEqual("Ele", ast.ClassDefinitions.Last().ID);


        }


        [Test]
        public void CanParseTypeDefinition()
        {
            string input =
                @"type Voltage = Real(unit = ""V"");";

            var ast = parser.ParseProgram(input);
            Assert.NotNull(ast);
            Assert.AreEqual("Voltage", ast.ClassDefinitions.First().ID);
            


        }


       
    }
}
