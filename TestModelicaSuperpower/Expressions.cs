using ModelingFramework.Core.Modelica;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace TestModelicaParser
{
    public class Tests
    {
        ModelicaParser parser = new ModelicaParser();

        [SetUp]
        public void Setup()
        {
            parser = new ModelicaParser();


        }


        [Test]
        public void CanParseNegNumbers([Values("-1.23", "-12.3", "-1.2e3")]  string input)
        {

            var result = parser.ParseExpression(input) as UnaryExpression;

            Assert.NotNull(result);

            var child = result.Child as DoubleLiteral;

            Assert.AreEqual(Double.Parse(input, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo), -child.Value);
        }


        [Test]
        public void CanParseNumbers([Values("1.23", "12.3", "1.2e3")]  string input)
        {

            var result = parser.ParseExpression(input) as DoubleLiteral;

            Assert.NotNull(result);
            Assert.AreEqual(Double.Parse(input, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo), result.Value);
        }


        [Test]
        public void CanParseTrue([Values("true", "True", "TRUE")]  string input)
        {
            
            var result = parser.ParseExpression(input) as BooleanLiteral;

            Assert.NotNull(result);
            Assert.AreEqual(true, result.Value);
        }
        [Test]
        public void CanParseFalse([Values("false", "False", "FALSE")]  string input)
        {

            var result = parser.ParseExpression(input) as BooleanLiteral;

            Assert.NotNull(result);
            Assert.AreEqual(false, result.Value);
        }


        [Test]
        public void CanParseStrings()
        {
            var input="\"Test\"";
            var result = parser.ParseExpression(input) as StringLiteral;

            Assert.NotNull(result);
            Assert.AreEqual("Test", result.Value);

        }
        [Test]
        public void CanParseIdentifiers()
        {
            var input = "Real";
            var result = parser.ParseExpression(input) as Reference;

            Assert.NotNull(result);
            Assert.AreEqual("Real", result.ID);
        }

        [Test]
        public void CanParseNestedIdentifiers()
        {
            var input = "Complex.re";
            var result = parser.ParseExpression(input) as Reference;

            Assert.NotNull(result);
            Assert.AreEqual("Complex", result.ID);
            Assert.NotNull(result.Next);
            Assert.AreEqual("re", result.Next.ID);
        }

        [Test]
        public void CanParseNested3Identifiers()
        {
            var input = "Complex.re.val";
            var result = parser.ParseExpression(input) as Reference;

            Assert.NotNull(result);
            Assert.AreEqual("Complex", result.ID);
            Assert.NotNull(result.Next);
            Assert.AreEqual("re", result.Next.ID);
            Assert.NotNull(result.Next.Next);
            Assert.AreEqual("val", result.Next.Next.ID);
        }



        [Test]
        public void CanAddTwoLiterals([Values("2+1", "2 +1", "2+ 1", "2 + 1")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual("2", ast.Left.ToString());
            Assert.AreEqual("1", ast.Right.ToString());
        }

        [Test]
        public void CanAddTwoLiteralsNBeg([Values("2+ -1", "2 +-1", "2+ -1", "2 + -1")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual("2", ast.Left.ToString());
            Assert.AreEqual("-1", ast.Right.ToString());
        }


        [Test]
        public void CanAddLiteralToReference([Values("a+1", "a +1", "a+ 1", "a + 1")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual(BinaryOperator.Add, ast.Operator);
            Assert.AreEqual("a", ast.Left.ToString());
            Assert.AreEqual("1", ast.Right.ToString());
        }

        [Test]
        public void CanAddReferenceToLiteral([Values("1+a", "1 +a", "1+ a", "1 + a")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual(BinaryOperator.Add, ast.Operator);
            Assert.AreEqual("1", ast.Left.ToString());
            Assert.AreEqual("a", ast.Right.ToString());
        }


        [Test]
        public void CanMultiply([Values("1*a", "1 *a", "1* a", "1 * a")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual(BinaryOperator.Multiply, ast.Operator);
            Assert.AreEqual("1", ast.Left.ToString());
            Assert.AreEqual("a", ast.Right.ToString());
        }

        [Test]
        public void CanDivide([Values("1/a", "1 /a", "1/ a", "1 / a")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual(BinaryOperator.Divide, ast.Operator);
            Assert.AreEqual("1", ast.Left.ToString());
            Assert.AreEqual("a", ast.Right.ToString());
        }

        [Test]
        public void CanAddMultiply([Values("a+2*b")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual(BinaryOperator.Add, ast.Operator);
            Assert.AreEqual("a", ast.Left.ToString());
            var ast2 = ast.Right as BinaryExpression;
            Assert.NotNull(ast2);
            Assert.AreEqual(BinaryOperator.Multiply, ast2.Operator);
            Assert.AreEqual("2", ast2.Left.ToString());
            Assert.AreEqual("b", ast2.Right.ToString());
        }

        [Test]
        public void CanAddMultiplyPowr([Values("a+2*b^4")]  string input)
        {
            var ast = parser.ParseExpression(input) as BinaryExpression;
            Assert.NotNull(ast);
            Assert.AreEqual(BinaryOperator.Add, ast.Operator);
            Assert.AreEqual("a", ast.Left.ToString());

            var ast2 = ast.Right as BinaryExpression;
            Assert.NotNull(ast2);
            Assert.AreEqual(BinaryOperator.Multiply, ast2.Operator);
            Assert.AreEqual("2", ast2.Left.ToString());

            var ast3 = ast2.Right as BinaryExpression;
            Assert.NotNull(ast3);
            Assert.AreEqual(BinaryOperator.Power, ast3.Operator);
            Assert.AreEqual("b", ast3.Left.ToString());
            Assert.AreEqual("4", ast3.Right.ToString());


        }

    }
}