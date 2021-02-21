using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Interfaces
{
    public interface ISyntaxChecker
    {
        bool Check(string sourceCode, out string error, out int line, out int column);
    }
}
