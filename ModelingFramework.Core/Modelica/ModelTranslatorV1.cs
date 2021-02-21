using ModelingFramework.Core.Expressions;
using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModelingFramework.Core.Modelica
{
    public class ModelTranslatorV1
    {


        public DAEProblem Translate(ClassDefinition model)
        {
            var problem = new DAEProblem();
            problem.Name = model.ID;

            foreach (var element in model.Elements)
                Handle(problem, element);

            return problem;
        }

        public DAEProblem Translate(Instance model)
        {
            var problem = new DAEProblem();
            problem.Name = model.ID;

            problem.Parameters.Add(new Variable("pi", Math.PI));

            foreach (var element in model.Parts)
                Handle(problem, element);
            foreach (var element in model.Equations)
                Handle(problem, element);

            return problem;
        }



        void Handle(DAEProblem problem, object value)
        {
            switch (value)
            {
                case Instance i:
                    {
                        var vari = new Variable(i.ID, 0.0);

                        var compClause = i.Source as ComponentClause;

                        bool isParameter = false;
                        if (compClause != null)
                        {
                            if (compClause.Variability == VariabilityPrefix.Parameter)
                                isParameter = true;
                        }

                        if (i.Value != null)
                        {
                            //problem.Equations.Add(new Equation(vari - i.Value.Evaluate()));
                            vari.SetValue(i.Value.Evaluate());
                            isParameter = true;
                        }


                        var initial = i.Parts.FirstOrDefault(p => p.ID == "start");
                        if (initial != null && initial.Value != null)
                        {
                            vari.SetValue(initial.Value.Evaluate());
                        }

                        if (isParameter)
                            problem.Parameters.Add(vari);
                        else
                            problem.AlgebraicVariables.Add(vari);
                    }
                    break;

                case ComponentClause c:
                    foreach (var decl in c.Declarations)
                    {
                        decl.Clause = c;
                        Handle(problem, decl);
                    }
                    break;

                case ComponentDeclaration e:
                    {
                        var vari = new Variable(e.ID, 0.0);

                        if (e.Modification != null)
                        {
                            foreach (var mod in e.Modification.Modifications)
                            {
                                if (mod.Reference.ToString().ToLower() == "start")
                                {
                                    vari.SetValue(mod.Modification.Value.Evaluate());
                                }
                            }
                        }

                        if (e.Modification != null && e.Modification.Value != null)
                        {
                            vari.SetValue(e.Modification.Value.Evaluate());
                            e.Clause.Variability = VariabilityPrefix.Parameter;
                        }

                        if (e.Clause.Variability == VariabilityPrefix.Parameter)
                            problem.Parameters.Add(vari);
                        else
                            problem.AlgebraicVariables.Add(vari);
                    }
                    break;

                case EquationSection e:
                    foreach (var eq in e.Equations)
                        Handle(problem, eq);
                    break;
                case SimpleEquation e:
                    var left = HandleExpression(problem, e.Left);
                    var right = HandleExpression(problem, e.Right);
                    var residual = right - left;
                    problem.Equations.Add(new Equation(residual));
                    break;
            }
        }

        Expression HandleExpression(DAEProblem problem, ModelicaExpression expr)
        {
            switch (expr)
            {
                case BinaryExpression b:
                    {
                        switch (b.Operator)
                        {
                            case BinaryOperator.Add:
                                return HandleExpression(problem, b.Left) + HandleExpression(problem, b.Right);
                            case BinaryOperator.Subtract:
                                return HandleExpression(problem, b.Left) - HandleExpression(problem, b.Right);
                            case BinaryOperator.Multiply:
                                return HandleExpression(problem, b.Left) * HandleExpression(problem, b.Right);
                            case BinaryOperator.Divide:
                                return HandleExpression(problem, b.Left) / HandleExpression(problem, b.Right);
                            case BinaryOperator.Power:
                                return Sym.Pow(HandleExpression(problem, b.Left), HandleExpression(problem, b.Right));
                        }
                    }
                    break;
                case UnaryExpression u:
                    {
                        switch (u.Operator)
                        {
                            case UnaryOperator.Negate:
                                return -HandleExpression(problem, u.Child);
                            case UnaryOperator.Parentheses:
                                return Sym.Par(HandleExpression(problem, u.Child));
                            case UnaryOperator.TimeDerivative:
                                var y = HandleExpression(problem, u.Child) as Variable;
                                if (y == null)
                                    throw new InvalidOperationException("Cannot handle expressions inside der operator yet.");

                                var dery = new Variable("der(" + y.Name + ")", 0);

                                var existingVar = problem.DifferentialVariables.FirstOrDefault(v => v.Name == dery.Name);
                                if (existingVar == null)
                                {

                                    problem.DifferentialVariables.Add(dery);
                                    problem.Mapping.Add(dery, y);
                                    return dery;
                                }
                                else
                                    return existingVar;

                            default:
                                throw new InvalidOperationException("Unknown unary operator " + u.Operator.ToString());
                        }
                    }

                case BuiltinFunction u:
                    {
                        switch (u.Type)
                        {
                            case BuiltinFunctionType.Sin:
                                return Sym.Sin(HandleExpression(problem, u.Child));
                            case BuiltinFunctionType.Cos:
                                return Sym.Cos(HandleExpression(problem, u.Child));
                            case BuiltinFunctionType.Tan:
                                return Sym.Tan(HandleExpression(problem, u.Child));
                            case BuiltinFunctionType.Log:
                                return Sym.Ln(HandleExpression(problem, u.Child));
                            case BuiltinFunctionType.Exp:
                                return Sym.Exp(HandleExpression(problem, u.Child));
                            case BuiltinFunctionType.Sqrt:
                                return Sym.Sqrt(HandleExpression(problem, u.Child));
                            default:
                                throw new InvalidOperationException("Unknown built-in fucntion " + u.Type.ToString());
                        }

                    }

                case DoubleLiteral l:
                    return new Variable(l.Value.ToString(), l.Value);
                case Reference r:
                    {
                        var vari = problem.ResolveReference(r.ToString());
                        if (vari != null)
                            return vari;
                        else
                            throw new NullReferenceException("Variable " + r.ToString() + " not defined.");
                    }
                default:
                    throw new InvalidOperationException("Unknown expression " + expr.ToString());
            }
            return null;
        }
    }
}
