using ModelingFramework.Core.Modelica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Modelica
{
    public class ModelicaPrettyPrinter
    {
        public string Transform(ModelicaProgram program)
        {
            var sb = new StringBuilder();
            Print(program, sb, 0);
            return sb.ToString();

        }

        public string Transform(ClassDefinition classdef)
        {
            var sb = new StringBuilder();
            Print(classdef, sb, 0);
            return sb.ToString();

        }

        string printPrefixes(ComponentClause c)
        {
            List<string> prefixes = new List<string>(); ;

            if (c.ConnectionType == FlowPrefix.Flow)
                prefixes.Add("flow");

            if (c.Variability == VariabilityPrefix.Constant)
                prefixes.Add("constant");

            if (c.Variability == VariabilityPrefix.Discrete)
                prefixes.Add("discrete");
            if (c.Variability == VariabilityPrefix.Parameter)
                prefixes.Add("parameter");


            if (c.Direction == DirectionPrefix.Input)
                prefixes.Add("input");
            else if   (c.Direction == DirectionPrefix.Output)
                prefixes.Add("output");

            var result= String.Join(' ', prefixes);

            if (!String.IsNullOrWhiteSpace(result))
                result += " ";
            
            return result;

        }

        string printClassConstraint(ClassConstraint c)
        {
            switch(c)
            {
                case ClassConstraint.Connector:
                    return "connector";
                case ClassConstraint.Model:
                    return "model";
                case ClassConstraint.Package:
                    return "package";
                case ClassConstraint.Function:
                    return "function";
                case ClassConstraint.Type:
                    return "type";
                case ClassConstraint.Block:
                    return "block";
            }

            return "class";
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
                    Indent(indent, sb, $"{n}");
                    break;
                case string s:
                    Indent(indent, sb, $"{s}");
                    break;
                case ModelicaProgram p:
                    foreach (var el in p.ClassDefinitions)
                        Print(el, sb, indent);
                    break;
                case ClassDefinition c:
                    Indent(indent, sb, $"{printClassConstraint(c.ClassConstraint)} {c.ID} {(c.Comment != null ? "\""+c.Comment+"\"" : "")}");                    
                    foreach (var el in c.Elements)
                        Print(el, sb, indent + 2);
                    Indent(indent, sb, $"end {c.ID}; ");
                    break;
                case Extends e:
                    Indent(indent, sb, $"extends {e.Type} {(e.Modification!=null?e.Modification.ToString():"")};");
                    break;
                case ComponentClause c:
                    Indent(indent, sb, $"{printPrefixes(c)}{c.Type} {String.Join(',', c.Declarations)};");                                                            
                    break;
                case EquationSection c:
                    Indent(indent-2, sb, $"equation");
                    foreach (var el in c.Equations)
                        Print(el+";", sb, indent);
                    break;
                case ConnectClause c:
                    Indent(indent, sb, $"connect({c.From}, {c.To})");                   
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
