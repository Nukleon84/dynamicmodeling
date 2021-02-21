using Apps.ModelEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Messaging
{
    public class RequestModelEditorMessage
    {
        public ModelicaModel Owner { get; set; }
    }
}
