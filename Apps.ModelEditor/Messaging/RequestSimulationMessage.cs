using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Messaging
{
    public class RequestSimulationMessage:BaseMessage
    {
        public string SourceCode { get; set; }
    }
}
