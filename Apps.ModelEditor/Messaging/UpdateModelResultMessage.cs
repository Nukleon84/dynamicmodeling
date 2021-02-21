using ModelingFramework.Core.Expressions;
using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apps.ModelEditor.Messaging
{
    public class UpdateModelResultMessage
    {
        public string ModelName { get; set; }

        public List<Variable> AlgebraicStates { get; set; }
        public List<Variable>DifferentialStates { get; set; }
        public List<TimeStep> TimeSteps { get; set; }
    }
}
