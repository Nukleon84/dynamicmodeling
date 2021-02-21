using ModelingFramework.Core.Modelica;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TestModelicaParser
{
    public class Compliance
    {
        ModelicaParser parser = new ModelicaParser();

        [SetUp]
        public void Setup()
        {
            parser = new ModelicaParser();
                               }


        [Test]
        public void CanParseIconsmo()
        {
            string input =
                @"within ModelicaCompliance;

package Icons
  model TestCase
  end TestCase;

  package TestPackage
  end TestPackage;

  annotation(Documentation(info = ""<html></html>""));
end Icons;
            ";
            //
            var ast = parser.ParseProgram(input);
            Assert.NotNull(ast);

            var icons = ast.ClassDefinitions.First();
            var testCase = icons.Elements[0] as ClassDefinition;
            var testPackage = icons.Elements[1] as ClassDefinition;
            var annot = icons.Elements[2] as Annotation;
            Assert.AreEqual("Icons", icons.ID);
            Assert.AreEqual("TestCase", testCase.ID);
            Assert.AreEqual("TestPackage", testPackage.ID);
            //Assert.AreEqual("TestPackage", annot.Modification);





        }

        [Test]
        public void CanParsePackagemo()
        {
            var input = @"package ModelicaCompliance ""A semantics compliance suite for the Modelica language""
  extends Icons.TestPackage;

            annotation(version = ""3.2"", Documentation(info = ""<html><p> ModelicaCompliance is a semantics compliance suite for the Modelica language version 3.2 revision 2.</ p >
<p> Licensed by the Modelica Association under the <a href =\""modelica://ModelicaCompliance.ModelicaLicense2\"">Modelica License 2</a>.</p>             
<p> The tools under <a href =\""modelica://ModelicaCompliance/Resources/tools\"">Resources/tools</a> are provided under the <a href=\""http://opensource.org/licenses/BSD-3-Clause\"">BSD 3-clause</a> license.</p></html> ""));
             end ModelicaCompliance;";

            var ast = parser.ParseProgram(input);
            Assert.NotNull(ast);

            var pack = ast.ClassDefinitions.First();
            Assert.AreEqual("ModelicaCompliance", pack.ID);

        }
    }


}
