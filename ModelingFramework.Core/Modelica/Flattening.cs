using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModelingFramework.Core.Modelica
{
    public class Flattening
    {

        string globalsource = @"
class root
class RealType
end RealType;

class StringType
end StringType;

class BooleanType
end BooleanType;

class Real    
    StringType unit;
    RealType min;
    RealType max;
    RealType start;
    BooleanType fixed;
end Real;
end root;";




        public Instance Transform(ClassDefinition source)
        {
            var inst = new Instance() { ID = source.ID };

            var parser = new ModelicaParser();
            var global = parser.ParseClassDefinition(globalsource);

            CreateReverseHierarchy(global, null);

            CreateReverseHierarchy(source, global);

            inst = Elaborate(source, inst);

            inst = flatten(inst);
            return inst;
        }


        bool isSimpleType(string type)
        {
            switch (type)
            {
                case "Real":
                case "String":
                case "Integer":
                    return true;
                default:
                    return false;
            }
        }
        Instance flatten(Instance inst)
        {
            bool changed = true;

            while (changed)
            {
                changed = false;
                foreach (var child in inst.Parts.ToArray())
                {
                    if (!isSimpleType(child.Type))
                    {
                        mergeInstance(child, inst);
                        inst.Parts.Remove(child);
                        changed = true;
                    }
                }
            }

            return inst;
        }

        void mergeInstance(Instance from, Instance to)
        {
            foreach (var part in from.Parts)
            {
                part.ID = from.ID + "." + part.ID;
                to.Parts.Add(part);
            }
            foreach (var eq in from.Equations)
            {
                renameRefsInEquation(eq, from.ID);

                to.Equations.Add(eq);
            }

        }


        void renameRefsInEquation(object eq, string id)
        {
            switch (eq)
            {
                case SimpleEquation e:
                    e.Left = appendIDinExpression(e.Left, id);
                    e.Right = appendIDinExpression(e.Right, id);
                    break;
                default:
                    return;
            }
        }

        ModelicaExpression appendIDinExpression(ModelicaExpression e, string ID)
        {
            switch (e)
            {
                case Literal l:
                    return l;
                case Reference r:
                    return new Reference() { ID = ID, Next = r };
                case UnaryExpression u:
                    u.Child = appendIDinExpression(u.Child, ID);
                    return u;
                case BinaryExpression b:
                    b.Left = appendIDinExpression(b.Left, ID);
                    b.Right = appendIDinExpression(b.Right, ID);
                    return b;
                default:
                    return e;

            }
        }




        void CreateReverseHierarchy(ClassDefinition node, ClassDefinition parent)
        {
            if (parent != null)
                node.Parent = parent;

            node.Definition = node;

            foreach (var classdef in node.Elements.OfType<ClassDefinition>())
            {
                classdef.Definition = classdef;
                CreateReverseHierarchy(classdef, node);
            }
        }


        Class lookupClass(Class c, Reference name, Instance host)
        {
            var x = lookup(c, name, true, host);
            if (x is Class)
                return (Class)x;
            else
                throw new InvalidOperationException($"{x} is not a class.");
        }

        object lookup(Class c, Reference refr, bool isFirst, Instance host)
        {
            var x = lookup(c, refr.ID, host);
            if (x != null)
            {
                if (refr.Next == null)
                    return x;
                else
                {
                    return x;
                }
            }
            else if (isFirst)
            {
                if (c.Parent != null)
                    return lookup(c.Parent, refr, true, null);

            }

            throw new InvalidOperationException($"{refr} could not be found in the search tree.");
        }

        object lookup(Class c, string id, Instance host)
        {
            var e = c.Definition.Elements.OfType<NamedElement>().FirstOrDefault(e => e.ID == id);
            if (e is ClassDefinition)
            {
                return createClass(e as ClassDefinition, c.Modification);
            }

            return null;

        }
        Class createClass(Class c, Modification qm)
        {
            var qmod = Merge(qm, c.Modification);

            if (qmod == c.Modification)
                return c;
            else
                return new ImplicitClass() { ID= c.ID, Definition = c.Definition, Modification = qmod, Parent = c.Parent };
            
        }

        Modification qualify(Class c, Modification mod, Instance host)
        {
            if (mod == null)
                return null;

            foreach(var em in mod.Modifications)
            {

            }
            return mod;
        }

        Class getClass(Class c, ComponentDeclaration e, Modification qm, Instance host)
        {
            var type = lookupClass(c, e.Clause.Type, host);
            var qmod = qualify(c, e.Modification, host);
            qmod = Merge(qm, qmod);
            return createClass(type, qmod);
        }
        Modification Merge(Modification env, Modification mod)
        {
            if (env == null)
                return mod;
            else if( mod==null)
                return env;
            else
            {
                var result = env.Copy();
                foreach(var em in mod.Modifications)
                {
                    if (Select(env, em.Reference.ID) == null)
                        result.Modifications.Add(em);
                }
                foreach(var e in mod.Redeclarations)
                {
                    //if(redeclare(env, e.ID)==null)
                    //result.Redeclarations.Add(e);
                }

                if (env.Value == null)
                    result.Value = mod.Value;

                return result;
            }
        }
        Modification Select(Modification env, string ID)
        {
            if (env==null || env.Modifications == null)
                return null;

            var x = env.Modifications.FirstOrDefault(x => x.Reference.ID == ID);
            if (x != null)
            {
                if (x.Reference.Next == null)
                    return x.Modification;
                else
                {
                    var mod = new Modification();
                    var em = new ElementModification { Reference = x.Reference.Next, Modification = x.Modification };
                    mod.Modifications.Add(em);
                    return mod;
                }
            }
            else
                return null;

        }

        Instance Elaborate(Class c, Instance host)
        {
            if (c.Definition == null)
            {

            }
            foreach (var ext in c.Definition.Elements.OfType<Extends>())
            {

            }

            foreach (var clause in c.Definition.Elements.OfType<ComponentClause>())
            {
                foreach (var decl in clause.Declarations)
                {
                    decl.Clause = clause;

                    var qmod = Select(c.Modification, decl.ID);

                    var type = getClass(c, decl, qmod, host);
                    var comp = Elaborate(type, new Instance());
                    comp.ID = decl.ID;
                    comp.Type = type.ID;
                    if (isSimpleType(type.ID))
                        comp.Source = decl.Clause;
                    host.Parts.Add(comp);
                }

            }

            foreach (var eqsec in c.Definition.Elements.OfType<EquationSection>())
            {
                foreach (var eq in eqsec.Equations)
                    host.Equations.Add(eq);
            }


            if (c.Modification != null)
                host.Value = c.Modification.Value;
            return host;
        }

    }
}
