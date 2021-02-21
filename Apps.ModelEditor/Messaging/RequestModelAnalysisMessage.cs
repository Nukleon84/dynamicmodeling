using System;
using System.Collections.Generic;
using System.Text;

namespace Apps.ModelEditor.Messaging
{
    public class RequestModelAnalysisMessage : BaseMessage
    {
        public string SourceCode { get; set; }
    }
}
