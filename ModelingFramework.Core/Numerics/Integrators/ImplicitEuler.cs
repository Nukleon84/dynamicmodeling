using ModelingFramework.Core.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ModelingFramework.Core.Expressions;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Numerics
{
    public class ImplicitEuler : IIntegrator
    {
        public double StepSize { get; set; } = 0.1;
        public double MinStepSize { get; set; } = 1e-6;
        public double StartTime { get; set; } = 0;
        public double EndTime { get; set; } = 1;
        public int TotalSteps { get; set; } = 1;
        public bool UseDulmageMendelsohnSolver { get; set; }
        public IAlgebraicSolver Solver { get; set; }
        public double Tolerance { get; set; }
        public Action<double> OnIteration { get; set; }

        Dictionary<Variable, Variable> PreMap = new Dictionary<Variable, Variable>();
        public void Discretize(DAEProblem problem)
        {
            foreach (var dery in problem.DifferentialVariables)
            {
                var y = problem.Mapping[dery];
                var yprev = new Variable("" + y.Name + "_[t-1]", y.InternalValue);
                problem.Parameters.Add(yprev);
                problem.Equations.Add(new Equation(Sym.Par(y - yprev) - problem.TimeStep * dery));
                PreMap.Add(y, yprev);
            }

        }

        public List<TimeStep> Integrate(DAEProblem problem, ILogger logger)
        {
            var results = new List<TimeStep>();
            TotalSteps = (int)((EndTime - StartTime) / StepSize);

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
                Time = StartTime,
                AlgebraicStates = x,
                DifferentialStates = y
            };
            results.Add(currentStep);


            for (int i = 0; i < TotalSteps; i++)
            {

                problem.Time.AddDelta(StepSize);

                problem.Step(Solver);

                foreach (var dery in problem.DifferentialVariables)
                {
                    var cury = problem.Mapping[dery];
                    var prey = PreMap[cury];
                    prey.SetValue(cury.InternalValue);
                }

                x = problem.AlgebraicVariables.Select(v => v.InternalValue).ToArray();
                y = problem.DifferentialVariables.Select(v => v.InternalValue).ToArray();

                currentStep = new TimeStep()
                {
                    Time = problem.Time.InternalValue,
                    AlgebraicStates = x,
                    DifferentialStates = y
                };
                results.Add(currentStep);
                logger.Log(currentStep.ToString());


                OnIteration?.Invoke((i + 1) / (double)TotalSteps);
                //System.Threading.Thread.Sleep(2);
            }
            return results;
        }

    }
}
