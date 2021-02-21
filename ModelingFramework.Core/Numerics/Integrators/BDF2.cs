using ModelingFramework.Core.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ModelingFramework.Core.Expressions;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Numerics
{
    public class BDF2 : IIntegrator
    {
        public double StepSize { get; set; } = 0.1;
        public double MinStepSize { get; set; } = 1e-6;
        public double StartTime { get; set; } = 0;
        public double EndTime { get; set; } = 1;
        public int TotalSteps { get; set; } = 1;
        public double Tolerance { get; set; }
        public Action<double> OnIteration { get; set; }

        public bool UseDulmageMendelsohnSolver { get; set; }

        public IAlgebraicSolver Solver { get; set; }

        Dictionary<Variable, Variable> PreMap = new Dictionary<Variable, Variable>();
        public void Discretize(DAEProblem problem)
        {
            foreach (var dery in problem.DifferentialVariables)
            {
                var y = problem.Mapping[dery];
                var yt_1 = new Variable("" + y.Name + "_[t-1]", y.InternalValue);
                problem.Parameters.Add(yt_1);
                var yt_2 = new Variable("" + y.Name + "_[t-2]", y.InternalValue);
                problem.Parameters.Add(yt_2);

                problem.Equations.Add(new Equation(Sym.Par(3 * y - 4 * yt_1 + yt_2) - 2 * problem.TimeStep * dery));
                PreMap.Add(y, yt_1);
                PreMap.Add(yt_1, yt_2);
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

            var reportingInveral = TotalSteps / 100;
            for (int i = 0; i < TotalSteps; i++)
            {

                problem.Time.AddDelta(StepSize);

                problem.Step(Solver);

                foreach (var dery in problem.DifferentialVariables)
                {
                    var cury = problem.Mapping[dery];
                    var yt_1 = PreMap[cury];
                    var yt_2 = PreMap[yt_1];
                    yt_2.SetValue(yt_1.InternalValue);
                    yt_1.SetValue(cury.InternalValue);
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

                if (i % reportingInveral == 0)
                    OnIteration?.Invoke(problem.Time.InternalValue / EndTime);
                //System.Threading.Thread.Sleep(2);
            }
            return results;
        }

    }
}
