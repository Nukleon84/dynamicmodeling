using ModelingFramework.Core.Modelica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Modelica
{
    public class ModelicaTreePrinter
    {
        public string Transform(ModelicaProgram program)
        {
            var sb = new StringBuilder();
            Print(program, sb, 0);
            return sb.ToString();

        }
        void Print(object value, StringBuilder sb, int indent = 0)
        {
            switch (value)
            {
                case null:
                    Indent(indent, sb, "Null");
                    break;
                case true:
                    Indent(indent, sb, "True");
                    break;
                case false:
                    Indent(indent, sb, "False");
                    break;
                case double n:
                    Indent(indent, sb, $"Number: {n}");
                    break;
                case string s:
                    Indent(indent, sb, $"String: {s}");
                    break;
                case ModelicaProgram p:
                    Indent(indent, sb, $"Program: ");
                    foreach (var el in p.ClassDefinitions)
                        Print(el, sb, indent + 2);
                    break;
                case ClassDefinition c:
                    Indent(indent, sb, $"ClassDefinition: {c.ID} ");
                    if (c.Comment != null)
                        Indent(indent+2, sb, $"Comment: "+c.Comment);
                    foreach (var el in c.Elements)
                        Print(el, sb, indent + 2);
                    break;
                  case ComponentDeclaration c:
                      Indent(indent, sb, $"ComponentDeclaration: {c.ID}");
                      if (c.Comment != null)
                          Indent(indent + 2, sb, $"Comment: " + c.Comment);
                      Print(c.Modification, sb, indent + 2);
                      break;
                case ComponentClause c:
                    Indent(indent, sb, $"ComponentClause: {c.Type} ({c.ConnectionType}, {c.Variability}, {c.Direction})");
                    if (c.Comment != null)
                        Indent(indent + 2, sb, $"Comment: " + c.Comment);
                    foreach (var el in c.Declarations)
                        Print(el, sb, indent + 2);
                    break;
                case EquationSection c:
                    Indent(indent, sb, $"EquationSection:");
                    foreach (var el in c.Equations)
                        Print(el, sb, indent + 2);
                    break;
                case ConnectClause c:
                    Indent(indent, sb, $"ConnectClause:");
                    Print(c.From, sb, indent + 2);
                    Print(c.To, sb, indent + 2);
                    break;
                case Modification m:
                    Indent(indent, sb, $"Modification:");
                    if (m.Value != null)
                    {
                        Indent(indent + 2, sb, $"Value=");
                        Print(m.Value, sb, indent + 2);
                    }
                    foreach (var c in m.Modifications)
                        Print(c, sb, indent + 2);
                    break;
                case ElementModification m:
                    Indent(indent, sb, $"Modification:");
                    Indent(indent + 2, sb, $"Ref: {m.Reference}");
                    Print(m.Modification, sb, indent + 2);
                    break;
                case BinaryExpression e:
                    Indent(indent, sb, $"Binary: {e.Operator}");
                    Print(e.Left, sb, indent + 2);
                    Print(e.Right, sb, indent + 2);
                    break;
                case SimpleEquation e:
                    Indent(indent, sb, $"Equation:");
                    if (e.Comment != null)
                        Indent(indent + 2, sb, $"Comment: " + e.Comment);
                    Print(e.Left, sb, indent + 2);
                    Print(e.Right, sb, indent + 2);
                    break;
                case ModelicaEquation c:
                    Indent(indent, sb, $"{c.ToString()}");
                    break;
                default:
                    Indent(indent, sb, $"{value.ToString()}");
                    break;
            }
        }

        void Indent(int amount, StringBuilder sb, string text)
        {
            sb.AppendLine($"{new string(' ', amount)}{text}");
        }
    }
}
