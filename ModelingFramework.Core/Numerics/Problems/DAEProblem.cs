using System;
using System.Collections.Generic;
using System.Text;
using ModelingFramework.Core.Expressions;
using ModelingFramework.Core.Interfaces;
using ModelingFramework.Core.Logging;
using System.Linq;

namespace ModelingFramework.Core.Numerics
{
    public class DAEProblem
    {
        public string Name { get; set; }

        public List<Variable> AlgebraicVariables { get; set; } = new List<Variable>();
        public List<Variable> DifferentialVariables { get; set; } = new List<Variable>();

        public List<Variable> Parameters { get; set; } = new List<Variable>();

        public List<Equation> Equations { get; set; } = new List<Equation>();

        public Dictionary<Variable, Variable> Mapping = new Dictionary<Variable, Variable>();


        public Variable Time { get; set; } = new Variable("time",0);
        public Variable TimeStep { get; set; } = new Variable("dt", 1);


        double[] StateValues;
        public AlgebraicSystem SystemToSolve;
      
        bool SolverStatus = false;
        ILogger _logger;


        public void Initialize(ILogger logger)
        {
            _logger = logger;

            StateValues = new double[DifferentialVariables.Count];
            SystemToSolve = new AlgebraicSystem(Name);
          

            foreach (var vari in AlgebraicVariables)
                SystemToSolve.AddVariables(vari);
            foreach (var vari in DifferentialVariables)
                SystemToSolve.AddVariable(vari);
            foreach (var eq in Equations)
                SystemToSolve.AddEquation(eq);

            SystemToSolve.CreateIndex();
            SystemToSolve.GenerateJacobian();
        }
        

        public void Step(IAlgebraicSolver solver)
        {
            SolverStatus = solver.Solve(SystemToSolve);

            if (!SolverStatus)
            {
                _logger.Error("Could not solve algebraic equation system.");
            }
        }
        

        public Variable ResolveReference(string id)
        {
            if (id == "time")
                return Time;

            var vari = AlgebraicVariables.FirstOrDefault(v => v.Name == id);
            if (vari != null)
                return vari;

            vari = Parameters.FirstOrDefault(v => v.Name == id);
            return vari;

        }

    }
}
