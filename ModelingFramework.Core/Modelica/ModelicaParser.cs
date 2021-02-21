using Superpower;
using Superpower.Display;
using Superpower.Parsers;
using Superpower.Tokenizers;
using Superpower.Model;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModelingFramework.Core.Modelica
{
    public enum ModelicaToken
    {
        None,
        Number,
        String,

        [Token(Category = "literal")]
        True,

        [Token(Category = "literal")]
        False,

        Identifier,

        [Token(Category = "keyword", Example = "der")]
        DerivativeOperator,

        [Token(Example = "(")]
        LParen,

        [Token(Example = ")")]
        RParen,


        [Token(Category = "built-in function")]
        Sin,
        [Token(Category = "built-in function")]
        Cos,
        [Token(Category = "built-in function")]
        Tan,
        [Token(Category = "built-in function")]
        Log,
        [Token(Category = "built-in function")]
        Exp,
        [Token(Category = "built-in function", Example = "time")]
        Time,
        [Token(Category = "built-in function", Example = "sqrt")]
        Sqrt,

        [Token(Category = "operator", Example = ".")]
        Accessor,

        [Token(Category = "operator", Example = "+")]
        Plus,

        [Token(Category = "operator", Example = "-")]
        Minus,

        [Token(Category = "operator", Example = "*")]
        Times,

        [Token(Category = "operator", Example = "/")]
        Divide,

        [Token(Category = "operator", Example = "/")]
        Power,


        [Token(Category = "operator", Example = "=")]
        EqualSign,

        [Token(Category = "puncutation", Example = ";")]
        Semicolon,
        [Token(Category = "puncutation", Example = ",")]
        Comma,


        [Token(Category = "relational-operator", Example = "==")]
        Equal,
        [Token(Category = "relational-operator", Example = "<>")]
        NotEqual,
        [Token(Category = "relational-operator", Example = ">")]
        GreaterThan,
        [Token(Category = "relational-operator", Example = "<")]
        LessThan,
        [Token(Category = "relational-operator", Example = ">=")]
        GreaterThanOrEqual,
        [Token(Category = "relational-operator", Example = "<=")]
        LessThanOrEqual,

        [Token(Category = "keyword", Example = "end")]
        End,

        [Token(Category = "keyword", Example = "equation")]
        Equation,

        [Token(Category = "keyword", Example = "class")]
        Class,

        [Token(Category = "keyword", Example = "model")]
        Model,

        [Token(Category = "keyword", Example = "operator")]
        Operator,

        [Token(Category = "keyword", Example = "record")]
        Record,

        [Token(Category = "keyword", Example = "block")]
        Block,

        [Token(Category = "keyword", Example = "connector")]
        Connector,

        [Token(Category = "keyword", Example = "function")]
        Function,

        [Token(Category = "keyword", Example = "type")]
        Type,

        [Token(Category = "keyword", Example = "package")]
        Package,

        [Token(Category = "keyword", Example = "extends")]
        Extends,

        [Token(Category = "keyword", Example = "redeclare")]
        Redeclare,

        [Token(Category = "keyword", Example = "inner")]
        Inner,

        [Token(Category = "keyword", Example = "outer")]
        Outer,

        [Token(Category = "keyword", Example = "public")]
        Public,

        [Token(Category = "keyword", Example = "protected")]
        Protected,

        [Token(Category = "keyword", Example = "external")]
        External,

        [Token(Category = "keyword", Example = "within")]
        Within,
        [Token(Category = "annotation", Example = "annotation")]
        Annotation,

        Import,
        Discrete,
        Parameter,
        Constant,
        Input,
        Output,


        [Token(Category = "keyword", Example = "flow")]
        Flow,

        [Token(Category = "keyword")]
        If,

        [Token(Category = "keyword")]
        Then,

        [Token(Category = "keyword")]
        Else,

        [Token(Category = "keyword")]
        ElseIf,

        [Token(Category = "keyword")]
        When,

        [Token(Category = "keyword")]
        For,

        [Token(Category = "keyword")]
        While,

        [Token(Category = "keyword")]
        Loop,

        [Token(Category = "keyword")]
        Connect
    }

    public class ModelicaParser
    {

        static TextParser<double> Number { get; } =
        //from sign in Character.EqualTo('-').Value(-1.0).OptionalOrDefault(1.0)
        from whole in Superpower.Parsers.Numerics.Natural.Select(n => double.Parse(n.ToStringValue()))
        from frac in Character.EqualTo('.')
            .IgnoreThen(Superpower.Parsers.Numerics.Natural)
            .Select(n => double.Parse(n.ToStringValue()) * Math.Pow(10, -n.Length))
            .OptionalOrDefault()
        from exp in Character.EqualToIgnoreCase('e')
            .IgnoreThen(Character.EqualTo('+').Value(1.0)
                .Or(Character.EqualTo('-').Value(-1.0))
                .OptionalOrDefault(1.0))
            .Then(expsign => Superpower.Parsers.Numerics.Natural.Select(n => double.Parse(n.ToStringValue()) * expsign))
            .OptionalOrDefault()
        select (whole + frac)  /**sign*/ * Math.Pow(10, exp);

        static TextParser<Unit> JsonStringToken { get; } =
      from open in Character.EqualTo('"')
      from content in Span.EqualTo("\\\"").Value(Unit.Value).Try()
          .Or(Character.Except('"').Value(Unit.Value))
          .IgnoreMany()
      from close in Character.EqualTo('"')
      select Unit.Value;

        static Tokenizer<ModelicaToken> Tokenizer = new TokenizerBuilder<ModelicaToken>()
        .Ignore(Span.WhiteSpace)
        .Ignore(Superpower.Parsers.Comment.CPlusPlusStyle)
        .Ignore(Superpower.Parsers.Comment.CStyle)
        .Match(Number, ModelicaToken.Number)
        .Match(JsonStringToken, ModelicaToken.String)
        .Match(Character.EqualTo('+'), ModelicaToken.Plus)
        .Match(Character.EqualTo('-'), ModelicaToken.Minus)
        .Match(Character.EqualTo('*'), ModelicaToken.Times)
        .Match(Character.EqualTo('/'), ModelicaToken.Divide)
        .Match(Character.EqualTo('^'), ModelicaToken.Power)
        .Match(Character.EqualTo('('), ModelicaToken.LParen)
        .Match(Character.EqualTo(')'), ModelicaToken.RParen)
        .Match(Character.EqualTo('.'), ModelicaToken.Accessor)
        .Match(Character.EqualTo('='), ModelicaToken.EqualSign)
        .Match(Character.EqualTo(';'), ModelicaToken.Semicolon)
        .Match(Character.EqualTo(','), ModelicaToken.Comma)
        .Match(Span.EqualTo("equation"), ModelicaToken.Equation, requireDelimiters: true)
        .Match(Span.EqualTo("end"), ModelicaToken.End, requireDelimiters: true)
        .Match(Span.EqualTo("model"), ModelicaToken.Model, requireDelimiters: true)
        .Match(Span.EqualTo("class"), ModelicaToken.Class, requireDelimiters: true)
        .Match(Span.EqualTo("package"), ModelicaToken.Package, requireDelimiters: true)
        .Match(Span.EqualTo("connector"), ModelicaToken.Connector, requireDelimiters: true)
        .Match(Span.EqualTo("function"), ModelicaToken.Function, requireDelimiters: true)
            .Match(Span.EqualTo("type"), ModelicaToken.Type, requireDelimiters: true)
        .Match(Span.EqualTo("block"), ModelicaToken.Block, requireDelimiters: true)
        .Match(Span.EqualTo("within"), ModelicaToken.Within, requireDelimiters: true)
        .Match(Span.EqualTo("annotation"), ModelicaToken.Annotation, requireDelimiters: true)
        .Match(Span.EqualTo("connect"), ModelicaToken.Connect, requireDelimiters: true)
        .Match(Span.EqualTo("parameter"), ModelicaToken.Parameter, requireDelimiters: true)
        .Match(Span.EqualTo("discrete"), ModelicaToken.Discrete, requireDelimiters: true)
        .Match(Span.EqualTo("constant"), ModelicaToken.Constant, requireDelimiters: true)
        .Match(Span.EqualTo("extends"), ModelicaToken.Extends, requireDelimiters: true)
        .Match(Span.EqualTo("flow"), ModelicaToken.Flow, requireDelimiters: true)
        .Match(Span.EqualToIgnoreCase("true"), ModelicaToken.True)
        .Match(Span.EqualToIgnoreCase("false"), ModelicaToken.False)
        .Match(Span.EqualTo("der"), ModelicaToken.DerivativeOperator, requireDelimiters: true)
        .Match(Span.EqualTo("sin"), ModelicaToken.Sin, requireDelimiters: true)
        .Match(Span.EqualTo("cos"), ModelicaToken.Cos, requireDelimiters: true)
        .Match(Span.EqualTo("tan"), ModelicaToken.Tan, requireDelimiters: true)
        .Match(Span.EqualTo("log"), ModelicaToken.Log, requireDelimiters: true)
        .Match(Span.EqualTo("exp"), ModelicaToken.Exp, requireDelimiters: true)
        .Match(Span.EqualTo("sqrt"), ModelicaToken.Sqrt, requireDelimiters: true)
        .Match(Identifier.CStyle, ModelicaToken.Identifier, requireDelimiters: true)        
        .Build();




        public static TextParser<string> String { get; } =
          from open in Character.EqualTo('"')
          from chars in Character.ExceptIn('"', '\\')
              .Or(Character.EqualTo('\\')
                  .IgnoreThen(
                      Character.EqualTo('\\')
                      .Or(Character.EqualTo('"'))
                      .Or(Character.EqualTo('/'))
                      .Or(Character.EqualTo('b').Value('\b'))
                      .Or(Character.EqualTo('f').Value('\f'))
                      .Or(Character.EqualTo('n').Value('\n'))
                      .Or(Character.EqualTo('r').Value('\r'))
                      .Or(Character.EqualTo('t').Value('\t'))
                      .Or(Character.EqualTo('u').IgnoreThen(
                              Span.MatchedBy(Character.HexDigit.Repeat(4))
                                  .Apply(Superpower.Parsers.Numerics.HexDigitsUInt32)
                                  .Select(cc => (char)cc)))
                      .Named("escape sequence")))
              .Many()
          from close in Character.EqualTo('"')
          select new string(chars);



        static readonly TokenListParser<ModelicaToken, ModelicaExpression> UnsignedNumberRule =
          Token.EqualTo(ModelicaToken.Number)
             .Apply(Number)
             .Select(n => (ModelicaExpression)new DoubleLiteral(n));


        static readonly TokenListParser<ModelicaToken, ModelicaExpression> StringLiteralRule =
            Token.EqualTo(ModelicaToken.String)
             .Apply(String)
            .Select(n => (ModelicaExpression)new StringLiteral(n.ToString()));

        static readonly TokenListParser<ModelicaToken, ModelicaExpression> BooleanLiteralRule =
            Token.EqualTo(ModelicaToken.True).Select(n => (ModelicaExpression)new BooleanLiteral(true))
            .Or(Token.EqualTo(ModelicaToken.False).Select(n => (ModelicaExpression)new BooleanLiteral(false)));


        static readonly TokenListParser<ModelicaToken, string> StringCommentRule =
            from comment in StringLiteralRule
            select ((StringLiteral)comment).Value;




        static readonly TokenListParser<ModelicaToken, string> Ident =
        Token.EqualTo(ModelicaToken.Identifier)
         .Apply(Identifier.CStyle)
        .Select(n => n.ToString());


        public static readonly TokenListParser<ModelicaToken, Reference> ReferenceRule =
             from id in Ident
             from next in (
                from _ in Token.EqualTo(ModelicaToken.Accessor)
                from nextref in ReferenceRule
                select nextref).OptionalOrDefault()
             select new Reference() { ID = id, Next = next };


        static readonly TokenListParser<ModelicaToken, ModelicaExpression> ReferenceGeneric =
             from refr in ReferenceRule select (ModelicaExpression)refr;

        static readonly TokenListParser<ModelicaToken, ModelicaExpression> ParenthesesExpressionGeneric =
          from lparen in Token.EqualTo(ModelicaToken.LParen)
          from expr in Expression
          from rparen in Token.EqualTo(ModelicaToken.RParen)
          select (ModelicaExpression)new UnaryExpression() { Child = expr, Operator = UnaryOperator.Parentheses };

        static readonly TokenListParser<ModelicaToken, ModelicaExpression> DerivativeRule =
            from der in Token.EqualTo(ModelicaToken.DerivativeOperator)
            from lparen in Token.EqualTo(ModelicaToken.LParen)
            from expr in Expression
            from rparen in Token.EqualTo(ModelicaToken.RParen)
            select (ModelicaExpression)new UnaryExpression() { Operator = UnaryOperator.TimeDerivative, Child = expr };


        static Dictionary<ModelicaToken, BuiltinFunctionType> TokenToBuiltInMapping = new Dictionary<ModelicaToken, BuiltinFunctionType>
        {
            {ModelicaToken.Sin, BuiltinFunctionType.Sin },
            {ModelicaToken.Cos, BuiltinFunctionType.Cos },
            {ModelicaToken.Tan, BuiltinFunctionType.Tan },
            {ModelicaToken.Log, BuiltinFunctionType.Log },
            {ModelicaToken.Exp, BuiltinFunctionType.Exp },
            {ModelicaToken.Sqrt, BuiltinFunctionType.Sqrt },
        };


        static readonly TokenListParser<ModelicaToken, ModelicaExpression> BuiltinRule =
         from der in Token.EqualTo(ModelicaToken.Sin)
            .Or(Token.EqualTo(ModelicaToken.Cos))
            .Or(Token.EqualTo(ModelicaToken.Tan))
            .Or(Token.EqualTo(ModelicaToken.Log))
            .Or(Token.EqualTo(ModelicaToken.Exp))
             .Or(Token.EqualTo(ModelicaToken.Sqrt))
         from lparen in Token.EqualTo(ModelicaToken.LParen)
         from expr in Expression
         from rparen in Token.EqualTo(ModelicaToken.RParen)
         select (ModelicaExpression)new BuiltinFunction() { Type = TokenToBuiltInMapping[der.Kind], Child = expr };

        static readonly TokenListParser<ModelicaToken, ModelicaExpression> FunctionRule =
            BuiltinRule;



        static readonly TokenListParser<ModelicaToken, ModelicaExpression> Primary =
            UnsignedNumberRule
             .Or(StringLiteralRule)
            .Or(ReferenceGeneric)
            .Or(BooleanLiteralRule)
            .Or(ParenthesesExpressionGeneric)
            .Or(DerivativeRule)
            .Or(FunctionRule);

        #region Expressions     

        static readonly TokenListParser<ModelicaToken, BinaryOperator> Add = from t in Token.EqualTo(ModelicaToken.Plus) select BinaryOperator.Add;
        static readonly TokenListParser<ModelicaToken, BinaryOperator> Subtract = from t in Token.EqualTo(ModelicaToken.Minus) select BinaryOperator.Subtract;
        static readonly TokenListParser<ModelicaToken, BinaryOperator> Multiply = from t in Token.EqualTo(ModelicaToken.Times) select BinaryOperator.Multiply;
        static readonly TokenListParser<ModelicaToken, BinaryOperator> Divide = from t in Token.EqualTo(ModelicaToken.Divide) select BinaryOperator.Divide;
        static readonly TokenListParser<ModelicaToken, BinaryOperator> Power = from t in Token.EqualTo(ModelicaToken.Power) select BinaryOperator.Power;

        static ModelicaExpression MakeBinary(BinaryOperator op, ModelicaExpression left, ModelicaExpression right)
        {
            return new BinaryExpression() { Operator = op, Left = left, Right = right };
        }

        static readonly TokenListParser<ModelicaToken, ModelicaExpression> NegationRule =
           from neg in Token.EqualTo(ModelicaToken.Minus).Optional()
           from prim in Primary
           select neg.HasValue ? new UnaryExpression() { Child = prim, Operator = UnaryOperator.Negate } : prim;


        static readonly TokenListParser<ModelicaToken, ModelicaExpression> Factor = Parse.Chain(Power, NegationRule, MakeBinary);
        static readonly TokenListParser<ModelicaToken, ModelicaExpression> Term = Parse.Chain(Multiply.Or(Divide), Factor, MakeBinary);
        static readonly TokenListParser<ModelicaToken, ModelicaExpression> ArithmeticExpression = Parse.Chain(Add.Or(Subtract), Term, MakeBinary);

        public static readonly TokenListParser<ModelicaToken, ModelicaExpression> SimpleExpression =
            ArithmeticExpression;

        public static readonly TokenListParser<ModelicaToken, ModelicaExpression> Expression =
            SimpleExpression;
        #endregion Expressions

        #region Equations            

        public static readonly TokenListParser<ModelicaToken, ModelicaEquation> ConnectClauseRule =
              from keyword in Token.EqualTo(ModelicaToken.Connect)
              from lparen in Token.EqualTo(ModelicaToken.LParen)
              from source in ReferenceRule
              from comma in Token.EqualTo(ModelicaToken.Comma)
              from sink in ReferenceRule
              from rparen in Token.EqualTo(ModelicaToken.RParen)
              from semi in Token.EqualTo(ModelicaToken.Semicolon)
              select (ModelicaEquation)new ConnectClause() { From = source, To = sink };

        public static readonly TokenListParser<ModelicaToken, ModelicaEquation> SimpleEquationRule =
            from left in SimpleExpression
            from _ in Token.EqualTo(ModelicaToken.EqualSign)
            from right in Expression
            from comment in StringCommentRule.OptionalOrDefault()
            from semi in Token.EqualTo(ModelicaToken.Semicolon)
            select (ModelicaEquation)new SimpleEquation() { Left = left, Right = right, Comment = comment };

        public static readonly TokenListParser<ModelicaToken, ModelicaEquation> EquationRule =
            SimpleEquationRule
            .Or(ConnectClauseRule);

        public static readonly TokenListParser<ModelicaToken, Element> EquationSectionRule =
             from keyword in Token.EqualTo(ModelicaToken.Equation)
             from eqs in EquationRule.Many()
             select (Element)new EquationSection() { Equations = eqs.ToList() };
        #endregion


        #region Classes

        static readonly TokenListParser<ModelicaToken, ModelicaExpression> ValueAssignment =
          from _ in Token.EqualTo(ModelicaToken.EqualSign)
          from value in Expression
          select value;

        static readonly TokenListParser<ModelicaToken, ElementModification> ElementModification =
           from name in ReferenceRule
           from mod in Modification.OptionalOrDefault()
           from comment in StringCommentRule.OptionalOrDefault()
           select new ElementModification() { Reference = name, Modification = mod };

        static readonly TokenListParser<ModelicaToken, ElementModification> Argument =
           ElementModification;

        static readonly TokenListParser<ModelicaToken, List<ElementModification>> ClassModificationRule =
            from _ in Token.EqualTo(ModelicaToken.LParen)
            from args in Argument.ManyDelimitedBy(Token.EqualTo(ModelicaToken.Comma))
            from __ in Token.EqualTo(ModelicaToken.RParen)
            select new List<ElementModification>(args);

        static readonly TokenListParser<ModelicaToken, Modification> Modification =
            from mods in ClassModificationRule.OptionalOrDefault(defaultValue: new List<ElementModification>())
            from valueassign in ValueAssignment.OptionalOrDefault()
            select new Modification() { Modifications = new List<ElementModification>(mods), Value = valueassign };


        static readonly TokenListParser<ModelicaToken, ComponentDeclaration> ComponentDeclarationRule =            
            from id in Ident
            from mod in Modification.OptionalOrDefault()
            from comment in StringCommentRule.OptionalOrDefault()
            
            select new ComponentDeclaration() { ID = id,  Modification = mod,  Comment = comment };

        static FlowPrefix mapTokenToFlowPrefix(ModelicaToken token)
        {
            switch(token)
            {
                case ModelicaToken.Flow:
                    return FlowPrefix.Flow;
                default:
                    return FlowPrefix.None;
            }
        }
        static VariabilityPrefix mapTokenToVariabilityPrefix(ModelicaToken token)
        {
            switch (token)
            {
                case ModelicaToken.Constant:
                    return VariabilityPrefix.Constant;
                case ModelicaToken.Parameter:
                    return VariabilityPrefix.Parameter;
                case ModelicaToken.Discrete:
                    return VariabilityPrefix.Discrete;
                default:
                    return VariabilityPrefix.None;
            }
        }

        static DirectionPrefix mapTokenToDirectionPrefix(ModelicaToken token)
        {
            switch (token)
            {
                case ModelicaToken.Input:
                    return DirectionPrefix.Input;
                case ModelicaToken.Output:
                    return DirectionPrefix.Output;                
                default:
                    return DirectionPrefix.None;
            }
        }
        static readonly TokenListParser<ModelicaToken, Tuple<FlowPrefix, VariabilityPrefix, DirectionPrefix>> TypePrefix =
            from flow in Token.EqualTo(ModelicaToken.Flow).OptionalOrDefault()
            from variability in Token.EqualTo(ModelicaToken.Discrete).
                Or(Token.EqualTo(ModelicaToken.Parameter)).
                Or(Token.EqualTo(ModelicaToken.Constant)).OptionalOrDefault()
            from direction in Token.EqualTo(ModelicaToken.Input).
                Or(Token.EqualTo(ModelicaToken.Output)).OptionalOrDefault()
            select new Tuple<FlowPrefix, VariabilityPrefix, DirectionPrefix>(mapTokenToFlowPrefix(flow.Kind), mapTokenToVariabilityPrefix(variability.Kind), mapTokenToDirectionPrefix(direction.Kind));
                

        static readonly TokenListParser<ModelicaToken, Element> ComponentClauseRule =
            from prefixes in TypePrefix
            from typespecifier in ReferenceRule
            from decl in ComponentDeclarationRule.ManyDelimitedBy(Token.EqualTo(ModelicaToken.Comma))
            from __ in Token.EqualTo(ModelicaToken.Semicolon)
            select (Element)new ComponentClause() {Type=typespecifier, ConnectionType= prefixes.Item1, Variability=prefixes.Item2, Direction=prefixes.Item3, Declarations=decl.ToList() };



        static readonly TokenListParser<ModelicaToken, Element> AnnotationRule =
       from keyword in Token.EqualTo(ModelicaToken.Annotation)
       from classmod in ClassModificationRule
       from __ in Token.EqualTo(ModelicaToken.Semicolon)
       select (Element)new Annotation() { Modification = new Modification() { Modifications = classmod } };

        static readonly TokenListParser<ModelicaToken, Element> ExtendsClauseRule =
            from keyword in Token.EqualTo(ModelicaToken.Extends)
            from name in ReferenceRule
            from classmod in ClassModificationRule.OptionalOrDefault()
            from annot in AnnotationRule.OptionalOrDefault()
            from __ in Token.EqualTo(ModelicaToken.Semicolon)
            select (Element)new Extends() {Type=name, Modification = new Modification() { Modifications = classmod }, Annotation= annot as Annotation };


        static List<Element> elementConcatenator(IList<Element> classBody, Extends extendsClause)
        {
            var elements = new List<Element>();

            if (extendsClause != null)
                elements.Add(extendsClause);
            if (classBody.Count() > 0)
                elements.AddRange(classBody);

            return elements;
        }

        static ClassConstraint mapTokenToConstraint(ModelicaToken token)
        {
            switch(token)
            {
                case ModelicaToken.Connector:
                    return ClassConstraint.Connector;
                case ModelicaToken.Block:
                    return ClassConstraint.Block;
                case ModelicaToken.Model:
                    return ClassConstraint.Model;
                case ModelicaToken.Function:
                    return ClassConstraint.Connector;
                case ModelicaToken.Package:
                    return ClassConstraint.Package;
                case ModelicaToken.Type:
                    return ClassConstraint.Type;
                case ModelicaToken.Class:
                default:
                    return ClassConstraint.Class;
            }
        }

        public static readonly TokenListParser<ModelicaToken, ClassDefinition> ClassDefinitionRule =
             from prefix in Token.EqualTo(ModelicaToken.Class).Or(Token.EqualTo(ModelicaToken.Model)).Or(Token.EqualTo(ModelicaToken.Package)).Or(Token.EqualTo(ModelicaToken.Connector)).Or(Token.EqualTo(ModelicaToken.Type))
             from ident in Ident
             from comment in StringCommentRule.OptionalOrDefault()
             from body in ClassBody.OptionalOrDefault(defaultValue: new List<Element>())
             from extends in ShortClassDef.OptionalOrDefault()
             select new ClassDefinition() { ID = ident, ClassConstraint= mapTokenToConstraint(prefix.Kind), Elements = elementConcatenator(body, extends), Comment = comment };

        public static readonly TokenListParser<ModelicaToken, Element> ClassDefinitionElementRule =
            from classDef in ClassDefinitionRule select (Element)classDef;


        static readonly TokenListParser<ModelicaToken, Element> Element =
            ClassDefinitionElementRule
            .Or<ModelicaToken, Element>(ComponentClauseRule)
            .Or<ModelicaToken, Element>(EquationSectionRule)
            .Or<ModelicaToken, Element>(AnnotationRule)
            .Or<ModelicaToken, Element>(ExtendsClauseRule)
            ;

        public static readonly TokenListParser<ModelicaToken, List<Element>> ClassBody =
                 from elements in Element.Many()
                 from end in Token.EqualTo(ModelicaToken.End)
                 from ident2 in Ident
                 from ___ in Token.EqualTo(ModelicaToken.Semicolon)
                 select elements.ToList();

        public static readonly TokenListParser<ModelicaToken, Extends> ShortClassDef =
                  from _ in Token.EqualTo(ModelicaToken.EqualSign)
                  from ident in ReferenceRule
                  from mod in ClassModificationRule.OptionalOrDefault(defaultValue: new List<ElementModification>())
                  from ___ in Token.EqualTo(ModelicaToken.Semicolon)
                  select new Extends() { Type = ident, Modification = new Modification() { Modifications = mod.ToList() } };


        static readonly TokenListParser<ModelicaToken, Reference> WithInRule =
            from keyword in Token.EqualTo(ModelicaToken.Within)
            from name in ReferenceRule
            from _ in Token.EqualTo(ModelicaToken.Semicolon)
            select name;

        public static readonly TokenListParser<ModelicaToken, ModelicaProgram> ProgramRule =
            from within in WithInRule.OptionalOrDefault()
            from defs in ClassDefinitionRule.AtLeastOnce()
            select new ModelicaProgram() { ClassDefinitions = defs.ToList(), Within = within };


        #endregion


        public ModelicaExpression ParseExpression(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            return Expression.Parse(tokens);

        }

        public Reference ParseReference(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            return ReferenceRule.Parse(tokens);

        }


        public ClassDefinition ParseClassDefinition(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            return ClassDefinitionRule.Parse(tokens);

        }
        public ModelicaProgram ParseProgram(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            return ProgramRule.Parse(tokens);

        }

        public bool TryParseProgram(string input, out ModelicaProgram prog, out string error, out Position errorPosition)
        {
            var tokens = Tokenizer.TryTokenize(input);
            if (!tokens.HasValue)
            {
                prog = null;
                error = tokens.ToString();
                errorPosition = tokens.ErrorPosition;
                return false;
            }

            var parsed = ProgramRule.TryParse(tokens.Value);
            if (!parsed.HasValue)
            {
                prog = null;
                error = parsed.ToString();
                errorPosition = parsed.ErrorPosition;
                return false;
            }

            prog = parsed.Value;
            error = null;
            errorPosition = Position.Empty;
            return true;
        }



    }
}
