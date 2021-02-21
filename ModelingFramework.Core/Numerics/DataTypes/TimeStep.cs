using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelingFramework.Core.Numerics
{
    public class TimeStep
    {
        public double Time { get; set; }
        public double[] DifferentialStates { get; set; }
        public double[] AlgebraicStates { get; set; }

        public override string ToString()
        {
            return $"{Time:G6}; {(string.Join("; ", AlgebraicStates))}";
        }
    }
}
