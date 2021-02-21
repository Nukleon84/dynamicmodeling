using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModelingFramework.Core.Modelica
{
    public class InstancePrinter
    {
        public string Transform(Instance instance)
        {
            var sb = new StringBuilder();
            Print(instance, sb, 0);
            return sb.ToString();

        }


        void Print(object value, StringBuilder sb, int indent = 0)
        {
            switch (value)
            {              
                case Instance i:
                    Indent(indent, sb, $"Instance {i.Type} {i.ID} {(i.Value!=null? "="+i.Value.ToString():"")}");
                    foreach (var el in i.Parts)
                        Print(el, sb, indent + 2);

                    if (i.Equations.Any())
                    {
                        Indent(indent, sb, $"equation");
                        foreach (var el in i.Equations)
                            Print(el, sb, indent + 2);
                    }
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
