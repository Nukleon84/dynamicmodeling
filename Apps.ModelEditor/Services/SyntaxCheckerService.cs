using Apps.ModelEditor.Interfaces;
using Apps.ModelEditor.Messaging;
using Caliburn.Micro;
using ModelingFramework.Core.Modelica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Services
{
    public class SyntaxCheckerService : ISyntaxChecker, IHandle<RequestSyntaxCheckMessage>
    {
        ModelicaParser parser = new ModelicaParser();
        IEventAggregator _eventAggregator;

        public SyntaxCheckerService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

        }

        public bool Check(string sourceCode, out string error, out int line, out int column)
        {
            var status = parser.TryParseProgram(sourceCode, out var prog, out var internalerror, out var position);

            error = "";
            line = -1;
            column = -1;
            if (!status)
            {
                error = internalerror;
                line = position.Line;
                column = position.Column;
                _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage(error, SolverStatus.Error));
            }
            else
            {
                _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage("OK", SolverStatus.OK));
            }

            return status;


        }

        public Task HandleAsync(RequestSyntaxCheckMessage message, CancellationToken cancellationToken)
        {
            var status = Check(message.SourceCode, out var error, out int line, out int column);

            return Task.Run(() =>
            {
               

            });
        }

    }
}
