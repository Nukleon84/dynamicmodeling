using System;
using System.Collections.Generic;
using System.Text;

namespace Apps.ModelEditor.Messaging
{
    public class RequestSaveModelMessage
    {
        public string Path { get; set; }
        public string SourceCode { get; set; }
    }
}
