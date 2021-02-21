using ModelingFramework.Core.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ModelingFramework.Core.Expressions;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Numerics
{
    public class BDF2A : IIntegrator
    {
        public double StepSize { get; set; } = 0.1;
        public double MinStepSize { get; set; } = 1e-6;

        public double StepSizeMax { get; set; } = 0.1;
        public double StartTime { get; set; } = 0;

        public double Tolerance { get; set; } = 1e-6;
        public double EndTime { get; set; } = 1;
        public int TotalSteps { get; set; } = 1;

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


        void TakeStep(DAEProblem problem, double dt)
        {
            problem.TimeStep.SetValue(dt);
            problem.Time.AddDelta(dt);
            problem.Step(Solver);

            foreach (var dery in problem.DifferentialVariables)
            {
                var cury = problem.Mapping[dery];
                var yt_1 = PreMap[cury];
                var yt_2 = PreMap[yt_1];
                yt_2.SetValue(yt_1.InternalValue);
                yt_1.SetValue(cury.InternalValue);
            }

        }

        void fillVector(Vector y, Vector ym1, Vector ym2, DAEProblem problem)
        {
            for (int i = 0; i < problem.DifferentialVariables.Count; i++)
            {
                var yt = problem.Mapping[problem.DifferentialVariables[i]];
                var yt_1 = PreMap[yt];
                var yt_2 = PreMap[yt_1];

                y[i] = yt.InternalValue;


                ym1[i] = yt_1.InternalValue;
                ym2[i] = yt_2.InternalValue;
            }
        }

        void resetVariables(Vector y, Vector ym1, Vector ym2, DAEProblem problem)
        {
            for (int i = 0; i < problem.DifferentialVariables.Count; i++)
            {
                var yt = problem.Mapping[problem.DifferentialVariables[i]];
                var yt_1 = PreMap[yt];
                var yt_2 = PreMap[yt_1];

                yt.SetValue(y[i]);
                yt_1.SetValue(ym1[i]);
                yt_2.SetValue(ym2[i]);
            }
        }


        public List<TimeStep> Integrate(DAEProblem problem, ILogger logger)
        {
            StepSizeMax = Math.Abs(EndTime - StartTime) / 50.0;

            var results = new List<TimeStep>();

            problem.Time.SetValue(StartTime);
            problem.TimeStep.SetValue(StepSize);

            if (UseDulmageMendelsohnSolver)
                Solver = new DecompositionSolver(logger);
            else
                Solver = new BasicNewtonSolver(logger);

            double rho = 0;
            double eps = 0;


            Vector y1h = new Vector(problem.AlgebraicVariables.Count);
            Vector y2h = new Vector(problem.AlgebraicVariables.Count);

            Vector y0 = new Vector(problem.AlgebraicVariables.Count);
            Vector yt_1 = new Vector(problem.AlgebraicVariables.Count);
            Vector yt_2 = new Vector(problem.AlgebraicVariables.Count);

            Vector dummy = new Vector(problem.AlgebraicVariables.Count);
            double lastProgress = 0;
            double currentProgress = 0;

            while (problem.Time.InternalValue <= EndTime)
            {

                var x = problem.AlgebraicVariables.Select(v => v.InternalValue).ToArray();
                var y = problem.DifferentialVariables.Select(v => v.InternalValue).ToArray();
                var t0 = problem.Time.InternalValue;
                var currentStep = new TimeStep()
                {
                    Time = t0,
                    AlgebraicStates = x,
                    DifferentialStates = y
                };
                results.Add(currentStep);
                rho = 0;

                fillVector(y0, yt_1, yt_2, problem);
                var dt = 0.0;

                int iter = 0;


                while (rho < 1)
                {
                    dt = 2 * StepSize;
                    resetVariables(y0, yt_1, yt_2, problem);
                    problem.Time.SetValue(t0);
                    TakeStep(problem, 2 * StepSize);
                    fillVector(y2h, dummy, dummy, problem);

                    resetVariables(y0, yt_1, yt_2, problem);
                    problem.Time.SetValue(t0);
                    TakeStep(problem, StepSize);
                    TakeStep(problem, StepSize);
                    fillVector(y1h, dummy, dummy, problem);

                    eps = (y1h - y2h).GetNorm() / 6.0;
                    rho = StepSize * eps / Tolerance;

                    StepSize = Math.Min(StepSize * Math.Pow(1.0 / rho, 1.0 / 3.0), 2 * StepSize);
                  
                    if (problem.Time.InternalValue + 2 * StepSize > EndTime)
                    {
                        StepSize = (EndTime - problem.Time.InternalValue) /2.0;
                    }
                    if (StepSize < MinStepSize)
                        StepSize = MinStepSize;


                    problem.Time.SetValue(t0);
                    if (iter > 10)
                        break;
                    iter++;
                }

           
                problem.Time.AddDelta(dt);
                currentProgress = problem.Time.InternalValue / EndTime;                
                if (currentProgress - lastProgress > 0.01)
                {
                    OnIteration?.Invoke(problem.Time.InternalValue / EndTime);
                    lastProgress = currentProgress;
                }
            }

           


            return results;
        }

    }
}
