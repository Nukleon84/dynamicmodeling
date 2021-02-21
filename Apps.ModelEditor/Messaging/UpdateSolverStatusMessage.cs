using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Messaging
{
    public enum SolverStatus { OK, Warning, Error, Fatal, Busy };
    public class UpdateSolverStatusMessage
    {
        string _solverStatus;
        SolverStatus _code;

        public string StatusMessage { get => _solverStatus; set => _solverStatus = value; }
        public SolverStatus StatusCode { get => _code; set => _code = value; }

        public UpdateSolverStatusMessage(string message, SolverStatus code)
        {
            StatusMessage = message;
            StatusCode = code;
        }
    }
}
