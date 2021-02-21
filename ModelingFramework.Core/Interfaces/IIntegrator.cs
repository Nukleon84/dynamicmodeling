using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelingFramework.Core.Interfaces
{
    public interface IIntegrator
    {
        public List<TimeStep> Integrate(DAEProblem problem, ILogger logger);
        public void Discretize(DAEProblem problem);
        
        public double StepSize { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public int TotalSteps { get; set; }

        public double MinStepSize { get; set; }

        public double Tolerance { get; set; }
        public Action<double> OnIteration { get; set; }

        public bool UseDulmageMendelsohnSolver { get; set; }

        public IAlgebraicSolver Solver { get; set; }

    }
}
