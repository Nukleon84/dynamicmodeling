using ModelingFramework.Core.Expressions;
using ModelingFramework.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelingFramework.Core.Numerics
{
    public class ExplicitEuler: IIntegrator
    {

        public double StepSize { get; set; } = 0.1;
        public double MinStepSize { get; set; } = 1e-6;

        public double StartTime { get; set; } = 0;
        public double EndTime { get; set; } = 1;

        Dictionary<Variable, Variable> Map = new Dictionary<Variable, Variable>();
        public int TotalSteps { get; set; } = 1;
        public double Tolerance { get; set; }
        public Action<double> OnIteration { get; set; }
        public bool UseDulmageMendelsohnSolver { get; set; }
        public IAlgebraicSolver Solver { get; set; }

        public void Discretize(DAEProblem problem)
        {
            foreach (var dery in problem.DifferentialVariables)
            {
                var y = problem.Mapping[dery];
                var ynext = new Variable("" + y.Name + "_[t+1]", y.InternalValue);
                problem.AlgebraicVariables.Add(ynext);
                problem.AlgebraicVariables.Remove(y);
                problem.Parameters.Add(y);

                problem.Equations.Add(new Equation(Sym.Par(ynext - y) - problem.TimeStep * dery));
                Map.Add(y, ynext);
            }
        }

        public List<TimeStep> Integrate(DAEProblem problem, ILogger logger)
        {
            var results = new List<TimeStep>();
            int numSteps = (int)((EndTime - StartTime) / StepSize);

            double time = StartTime;
            problem.Time.SetValue(StartTime);
            problem.TimeStep.SetValue(StepSize);

            if (UseDulmageMendelsohnSolver)
                Solver = new DecompositionSolver(logger);
            else
                Solver = new BasicNewtonSolver(logger);


            var x = problem.AlgebraicVariables.Select(v => v.InternalValue).ToArray();
            var y = problem.DifferentialVariables.Select(v => v.InternalValue).ToArray();

            var currentStep = new TimeStep()
            {
                Time = time,
                AlgebraicStates = x,
                DifferentialStates = y
            };
            results.Add(currentStep);


            for (int i = 0; i < numSteps; i++)
            {
                problem.Step(Solver);

                foreach (var dery in problem.DifferentialVariables)
                {
                    var cury = problem.Mapping[dery];
                    cury.AddDelta(dery.InternalValue);
                }

                time += StepSize;
                problem.Time.AddDelta(StepSize);

                x = problem.AlgebraicVariables.Select(v => v.InternalValue).ToArray();
                y = problem.DifferentialVariables.Select(v => v.InternalValue).ToArray();

                currentStep = new TimeStep()
                {
                    Time = time,
                    AlgebraicStates = x,
                    DifferentialStates = y
                };
                results.Add(currentStep);
                logger.Log(currentStep.ToString());

            }

            return results;
        }
    }
}
