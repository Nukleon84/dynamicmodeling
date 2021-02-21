
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace ModelingFramework.Core.Modelica
{
    public enum LiteralType { Number, String, Boolean }
    public abstract class Literal : ModelicaExpression
    {
        public LiteralType LiteralType { get; protected set; }

        protected object _value;
        public virtual object Value { get { return _value; } set { _value = value; } }

        public override string ToString()
        {
            return _value.ToString();
        }


    }

    public class BooleanLiteral : Literal
    {

        public new bool Value { get { return (bool)_value; } set { _value = value; } }
        public BooleanLiteral()
        {
            LiteralType = LiteralType.Boolean;
        }
        public BooleanLiteral(bool value) : this()
        {
            _value = value;
        }

        public override double Evaluate()
        {
            return Value ? 1 : 0;
        }
    }

    public class StringLiteral : Literal
    {

        public new string Value { get { return _value.ToString(); } set { _value = value; } }
        public StringLiteral()
        {
            LiteralType = LiteralType.String;
        }
        public StringLiteral(string value) : this()
        {
            _value = value;
        }

        public override string ToString()
        {
            return "\"" + _value.ToString() + "\"";
        }


    }
    public class DoubleLiteral : Literal
    {

        public new double Value { get { return (double)_value; } set { _value = value; } }

        public DoubleLiteral()
        {
            LiteralType = LiteralType.Number;
        }
        public DoubleLiteral(double value) : this()
        {
            _value = value;
        }

        public DoubleLiteral(string value) : this()
        {
            var parsed = Double.Parse(value, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo);
            _value = parsed;
        }

        public override string ToString()
        {
            return Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public override double Evaluate()
        {
            return Value;
        }
    }

    public abstract class Element
    {
        public Modification Modification { get; set; }
        public string Comment { get; set; }
    }

    public abstract class NamedElement : Element
    {
        public string ID { get; set; }
    }

    public abstract class Class : NamedElement
    {
        public Class Parent { get; set; }
        public ClassDefinition Definition { get; set; }
    }
    public class ImplicitClass : Class
    {

    }


      

    public enum ClassConstraint { Class, Model, Connector, Block, Record, Function, Type, Package }
    public class ClassDefinition : Class
    {
        public ClassConstraint ClassConstraint { get; set; }
        public IList<Element> Elements { get; set; } = new List<Element>();
        public IList<Import> Imports { get; set; } = new List<Import>();

        public override string ToString()
        {
            return ClassConstraint.ToString() + " " + ID;
        }
    }




    public class Extends : Element
    {
        public Reference Type { get; set; }
        public Annotation Annotation { get; set; }

        public override string ToString()
        {
            return Type.ToString();
        }

    }

    public class ModelicaEquation
    {
        public string Comment { get; set; }
    }

    public class SimpleEquation : ModelicaEquation
    {
        public ModelicaExpression Left { get; set; }
        public ModelicaExpression Right { get; set; }
        public override string ToString()
        {
            return Left.ToString() + " = " + Right.ToString();
        }

    }

    public class ConnectClause : ModelicaEquation
    {
        public Reference From { get; set; }
        public Reference To { get; set; }
        public override string ToString()
        {
            return "connect(" + From.ToString() + " , " + To.ToString() + ")";
        }

    }

    public class Annotation : Element
    {

    }

    public class EquationSection : Element
    {
        public IList<ModelicaEquation> Equations { get; set; }
    }
    public class Modification
    {
        public IList<ComponentDeclaration> Redeclarations { get; set; } = new List<ComponentDeclaration>();
        public IList<ElementModification> Modifications { get; set; } = new List<ElementModification>();

        public ModelicaExpression Value { get; set; }

        public override string ToString()
        {
            var result = "";
            if (Modifications != null && Modifications.Any())
            {
                result += "(" + String.Join(',', Modifications) + ")";
            }

            if (Value != null)
                result += "=" + Value.ToString();

            return result;
        }

        public Modification Copy()
        {

            var copy = new Modification();
            copy.Value = Value;


            foreach (var mod in Redeclarations)
                copy.Redeclarations.Add(mod.Copy());



            foreach (var mod in Modifications)
                copy.Modifications.Add(mod.Copy());

            return copy;

        }
    }


    public class ElementModification
    {
        public Modification Modification { get; set; }
        public Reference Reference { get; set; }

        public override string ToString()
        {
            return Reference + Modification?.ToString();
        }

        public ElementModification Copy()
        {
            var copy = new ElementModification();
            copy.Reference = Reference.Copy();
            copy.Modification = Modification.Copy();
            return copy;

        }

    }

    public enum FlowPrefix { None, Flow, Stream };
    public enum VariabilityPrefix { None, Discrete, Parameter, Constant };
    public enum DirectionPrefix { None, Input, Output };

    public class ComponentClause : Element
    {
        public Reference Type { get; set; }
        public FlowPrefix ConnectionType { get; set; }
        public VariabilityPrefix Variability { get; set; }
        public DirectionPrefix Direction { get; set; }


        public List<ComponentDeclaration> Declarations { get; set; } = new List<ComponentDeclaration>();
    }

    public class ComponentDeclaration : NamedElement
    {
        public ComponentClause Clause { get; set; }

        public override string ToString()
        {
            return ID + this.Modification.ToString();
        }

        public ComponentDeclaration Copy()
        {
            var copy = new ComponentDeclaration();
            copy.ID = ID;
            copy.Comment = Comment;
            copy.Clause = Clause;
            return copy;
        }
    }



    public class Reference : ModelicaExpression
    {
        public Reference Next { get; set; }
        public string ID { get; set; }

        public override string ToString()
        {
            return ID + (Next != null ? ("." + Next.ToString()) : "");
        }

        public Reference Copy()
        {
            var copy = new Reference();
            copy.ID = ID;
            
            if (Next != null)
                copy.Next = Next.Copy();

            return copy;
        }
    }

    public class ModelicaExpression
    {
        public virtual double Evaluate()
        {
            throw new NotSupportedException("Cannot evaluate " + this.ToString() + " as a number.");
        }


    }


    public enum BuiltinFunctionType { Sin, Cos, Tan, Log, Exp, Sqrt }
    public class BuiltinFunction : ModelicaExpression
    {
        public ModelicaExpression Child { get; set; }
        public BuiltinFunctionType Type { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case BuiltinFunctionType.Sin:
                    return "sin( " + Child.ToString() + " )";
                case BuiltinFunctionType.Cos:
                    return "cos( " + Child.ToString() + " )";
                case BuiltinFunctionType.Tan:
                    return "sin( " + Child.ToString() + " )";
                case BuiltinFunctionType.Log:
                    return "log( " + Child.ToString() + " )";
                case BuiltinFunctionType.Exp:
                    return "exp( " + Child.ToString() + " )";
                case BuiltinFunctionType.Sqrt:
                    return "sqrt( " + Child.ToString() + " )";
                default:
                    return Child.ToString();
            }
        }



    }



    public enum UnaryOperator { Negate, Parentheses, TimeDerivative }
    public class UnaryExpression : ModelicaExpression
    {
        public ModelicaExpression Child { get; set; }
        public UnaryOperator Operator { get; set; }

        public override string ToString()
        {
            switch (Operator)
            {
                case UnaryOperator.Negate:
                    return "-" + Child.ToString();
                case UnaryOperator.Parentheses:
                    return "( " + Child.ToString() + " )";
                case UnaryOperator.TimeDerivative:
                    return "der( " + Child.ToString() + " )";
                default:
                    return Child.ToString();
            }

        }

        public override double Evaluate()
        {
            switch (Operator)
            {
                case UnaryOperator.Negate:
                    return -Child.Evaluate();
                case UnaryOperator.Parentheses:
                    return Child.Evaluate();
                case UnaryOperator.TimeDerivative:
                    throw new NotSupportedException("Cannot evaluate derivative operator at compile time");
                default:
                    return base.Evaluate();
            }
        }


    }

    public enum BinaryOperator { Multiply, Divide, Add, Subtract, Power, GreaterThan, LessThan, GreaterThanOrEqual, LessThenOrEqual, Equal, NotEqual };


    public class BinaryExpression : ModelicaExpression
    {
        public ModelicaExpression Left { get; set; }
        public ModelicaExpression Right { get; set; }

        public BinaryOperator Operator { get; set; }


        public override string ToString()
        {
            string opsym = "#";

            switch (Operator)
            {
                case BinaryOperator.Add:
                    opsym = "+";
                    break;
                case BinaryOperator.Subtract:
                    opsym = "-";
                    break;
                case BinaryOperator.Multiply:
                    opsym = "*";
                    break;
                case BinaryOperator.Divide:
                    opsym = "/";
                    break;
                case BinaryOperator.Power:
                    opsym = "^";
                    break;
            }
            return Left.ToString() + " " + opsym + " " + Right.ToString();
        }


        public override double Evaluate()
        {
            switch (Operator)
            {
                case BinaryOperator.Add:
                    return Left.Evaluate() + Right.Evaluate();
                case BinaryOperator.Subtract:
                    return Left.Evaluate() - Right.Evaluate();
                case BinaryOperator.Multiply:
                    return Left.Evaluate() * Right.Evaluate();
                case BinaryOperator.Divide:
                    return Left.Evaluate() / Right.Evaluate();
                case BinaryOperator.Power:
                    return Math.Pow(Left.Evaluate(), Right.Evaluate());
                default:
                    return base.Evaluate();
            }
        }


    }



    public class Component
    {

    }

    public class Instance : Component
    {
        public IList<Instance> Parts { get; set; } = new List<Instance>();
        public IList<ModelicaEquation> Equations { get; set; } = new List<ModelicaEquation>();
        public string ID { get; set; }
        public ModelicaExpression Value { get; set; }

        public string Type { get; set; }
        public Element Source { get; set; }

    }

    public class QualifiedReference : Component
    {
        public Reference Reference { get; set; }
        public Instance Host { get; set; }
    }

    public class Import
    {
        public string ID { get; set; }
        public Reference Name { get; set; }
    }

    public class ModelicaProgram
    {
        public Reference Within { get; set; }
        public IList<ClassDefinition> ClassDefinitions { get; set; }
    }
}
