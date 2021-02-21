using CSparse.Double.Factorization;
using CSparse.Storage;
using ModelingFramework.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingFramework.Core.Numerics
{

    /// <summary>
    /// Wrapper class around CSparse.NET LU solve
    /// </summary>
    public class CSparseWrapper
    {

        public static Vector SolveLinearSystem(CompressedColumnStorage<double> A, Vector x, Vector b, out bool status, out string error)
        {
            status = false;
            error = null;

            var result = SolveLU(A, x, b, out status, out error);


            return result;
        }


        public static Vector SolveLU(CompressedColumnStorage<double> A, Vector x, Vector b, out bool status, out string error)
        {
            var orderings = new[] { CSparse.ColumnOrdering.MinimumDegreeAtPlusA, CSparse.ColumnOrdering.MinimumDegreeAtA, CSparse.ColumnOrdering.MinimumDegreeStS, CSparse.ColumnOrdering.Natural };

            status = false;
            error = null;
            try
            {
                status = true;
                if (A.RowCount == A.ColumnCount)
                {
                    var lu = SparseLU.Create(A, CSparse.ColumnOrdering.MinimumDegreeAtPlusA, 1.0);
                    lu.Solve(b.ToDouble(), x.ToDouble());
                    status = true;
                    return x;
                }
            }
            catch (Exception e)
            {

                error = e.Message;
                status = false;
            }
            return x;
        }

        public static CompressedColumnStorage<double> FillResidualAndJacobian(AlgebraicSystem system, Vector b)
        {
            var sparseJacobian= new CoordinateStorage<double>(system.NumberOfEquations, system.NumberOfVariables, system.Jacobian.Count);

            int j = -1;
            double grad = -1;
            for (int i = 0; i < system.Equations.Count; i++)
            {
                system.Equations[i].Reset();
                b[i] = system.Equations[i].Residual();

                foreach (var variable in system.Equations[i].Variables)
                {                                        
                    if (system.VariableIndex.TryGetValue(variable, out j))
                    {
                        grad = system.Equations[i].Expression.Diff(variable);
                        sparseJacobian.At(i, j, grad);
                    }
                }
            }
          /*  int j = -1;
            double grad = -1;

            Parallel.For(0, system.Equations.Count, i =>
            {
                system.Equations[i].Reset();
                b[i] = system.Equations[i].Residual();
              
                foreach (var variable in system.Equations[i].Variables)
                {
                    if (system.VariableIndex.TryGetValue(variable, out j))
                    {
                        grad = system.Equations[i].Expression.Diff(variable);
                        sparseJacobian.At(i, j, grad);
                    }
                }
            });*/


            return CSparse.Converter.ToCompressedColumnStorage(sparseJacobian);
        }

        public static CompressedColumnStorage<double> ConvertSparsityJacobian(AlgebraicSystem problem)
        {
            var sparseJacobian = new CoordinateStorage<double>(problem.NumberOfEquations, problem.NumberOfVariables, problem.Jacobian.Count);

            foreach (var entry in problem.Jacobian)
            {
                sparseJacobian.At(entry.EquationIndex, entry.VariableIndex, 1);
            }

            var compJacobian = CSparse.Converter.ToCompressedColumnStorage(sparseJacobian);
            return compJacobian;
        }


        public static CompressedColumnStorage<double> CreateIdentity(int dim, double diagonal)
        {
            var identityMatrix = new CoordinateStorage<double>(dim, dim, dim);

            for (int i = 0; i < dim; i++)
            {
                identityMatrix.At(i, i, diagonal);
            }
            var compidentityMatrix = CSparse.Converter.ToCompressedColumnStorage(identityMatrix);
            return compidentityMatrix;
        }


        public static CompressedColumnStorage<double> CreateDiagonal(int dim, double[] diagonal)
        {
            var identityMatrix = new CoordinateStorage<double>(dim, dim, dim);

            for (int i = 0; i < dim; i++)
            {
                identityMatrix.At(i, i, diagonal[i]);
            }
            var compidentityMatrix = CSparse.Converter.ToCompressedColumnStorage(identityMatrix);
            return compidentityMatrix;
        }


        public static CompressedColumnStorage<double> CreateDiagonalInverse(int dim, double[] diagonal)
        {
            var identityMatrix = new CoordinateStorage<double>(dim, dim, dim);

            for (int i = 0; i < dim; i++)
            {
                identityMatrix.At(i, i, 1.0 / diagonal[i]);
            }
            var compidentityMatrix = CSparse.Converter.ToCompressedColumnStorage(identityMatrix);
            return compidentityMatrix;
        }

    }

}
