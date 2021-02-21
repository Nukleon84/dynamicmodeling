using ModelingFramework.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Expressions
{
    [DebuggerDisplay("{FullName} = {Val()}")]
    public class Variable : Expression
    {
        bool _isFixed = false;
        bool _isConstant = false;
        bool _isBound = false;

        Unit _internalUnit;
        Unit _displayUnit;

        string _description;

        PhysicalDimension _dimension;

        private double _upperBound = 1e20;
        private double _lowerBound = -1e20;
    
        public bool IsFixed { get => _isFixed; set => _isFixed = value; }
        public Unit InternalUnit { get => _internalUnit; }
        public Unit DisplayUnit { get => _displayUnit; set => _displayUnit = value; }
     
        public string FullName
        {
            get
            {
                return Name;
            }
        }

        public double DisplayValue
        {
            get
            {
                return Unit.Convert(_internalUnit, DisplayUnit, Val());
            }
            set
            {
                var newValueInInteral = Unit.Convert(DisplayUnit, _internalUnit, value);
                SetValue(newValueInInteral);
            }
        }

        public double InternalValue
        {
            get { return Val(); }
            set { SetValue(value); }
        }


        public double UpperBound { get => _upperBound; set => _upperBound = value; }
        public double LowerBound { get => _lowerBound; set => _lowerBound = value; }
        public string Description { get => _description; set => _description = value; }
        public PhysicalDimension Dimension { get => _dimension; set => _dimension = value; }
        public bool IsConstant { get => _isConstant; set => _isConstant = value; }
        public bool IsBound { get => _isBound; set => _isBound = value; }

        public Variable(string name, Func<double> valueFunc, Unit internalUnit = null) : base(name, valueFunc, (vari) => 1)
        {
            if (internalUnit == null)
                internalUnit = SI.none;

            _internalUnit = internalUnit;
            DisplayUnit = internalUnit;
            DiffFunc = (vari) => vari == this ? 1 : 0;
        }


        public Variable(string name, double value, double lower, double upper, Unit internalUnit, string description) : this(name, () => value, internalUnit)
        {
            LowerBound = lower;
            UpperBound = upper;
            Description = description;
        }


        public Variable(string name, double value, Unit internalUnit = null) : this(name, () => value, internalUnit)
        {

        }


        public override string Pretty()
        {
            return Name;
        }


        public override string ToString()
        {
            return FullName;
        }
        public void Unbind()
        {
            SetValue(Val());
            DiffFunc = (vari) => vari == this ? 1 : 0;
            IsBound = false;
        }
        public void BindTo(Expression exp)
        {
            Value = Double.NaN;
            ValueFunc = () => Math.Max(LowerBound, Math.Min(UpperBound, exp.Val()));
            DiffFunc = (vari) => exp.Diff(vari);
            this.Children.Clear();
            this.AddChildren(exp);
            IsBound = true;
        }
        
        public void AddDelta(double delta)
        {
            var newValue = Val() + delta;

            if (newValue > UpperBound)
                newValue = UpperBound;
            if (newValue < LowerBound)
                newValue = LowerBound;

            SetValue(newValue);
        }

        public void Unfix()
        {
            IsFixed = false;
        }

        public void Fix(double value)
        {
            SetValue(value);
            IsFixed = true;
        }

        public void Fix()
        {
            IsFixed = true;
        }

        public void Init(double value)
        {
            SetValue(value);
        }

        public void Init(double value, Unit newUnit)
        {
            if (Unit.AreSameDimension(newUnit, InternalUnit))
            {
                var convertedValue = Unit.Convert(newUnit, InternalUnit, value);
                Init(convertedValue);
            }
            else
                throw new Exception("Dimensions of assignment do not fit." + newUnit + " " + InternalUnit);
        }


        public void Fix(double value, Unit newUnit)
        {
            if (Unit.AreSameDimension(newUnit, InternalUnit))
            {
                var convertedValue = Unit.Convert(newUnit, InternalUnit, value);
                Fix(convertedValue);
            }
            else
                throw new Exception("Dimensions of assignment do not fit." + newUnit + " " + InternalUnit);
        }

        public string WriteReport()
        {
            return string.Format("{0,-25} = {1, 12} {2,-15} {3}", FullName, DisplayValue.ToString("G4"), DisplayUnit.Symbol,
                Description);
        }


    }
}
