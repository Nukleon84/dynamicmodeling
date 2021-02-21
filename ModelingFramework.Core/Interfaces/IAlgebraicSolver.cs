using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelingFramework.Core.Interfaces
{
    public interface IAlgebraicSolver
    {
        public bool Solve(AlgebraicSystem system);
    }
}
