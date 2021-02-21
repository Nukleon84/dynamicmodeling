using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apps.ModelEditor.Messaging
{
   public  class UpdateModelAnalysisMessage:BaseMessage
    {
        public string SyntaxTree { get; set; }

        public string InstanceTree { get; set; }

        public string FlattenedModel { get; set; }
        public DAEProblem CalculationModel { get; set; }
    }
}
