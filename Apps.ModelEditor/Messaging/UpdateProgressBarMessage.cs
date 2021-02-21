using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Messaging
{
    public class UpdateProgressBarMessage : BaseMessage
    {
        int _currentProgress;

        public int CurrentProgress { get => _currentProgress; set => _currentProgress = value; }

        public UpdateProgressBarMessage(int progress)
        {
            CurrentProgress = progress;
        }
    }
}
