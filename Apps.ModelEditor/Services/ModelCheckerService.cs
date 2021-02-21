using Apps.ModelEditor.Interfaces;
using Apps.ModelEditor.Messaging;
using Caliburn.Micro;
using ModelingFramework.Core.Modelica;
using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Services
{
    public class ModelCheckerService: IModelChecker, IHandle<RequestModelAnalysisMessage>
    {
        ModelicaParser parser = new ModelicaParser();
        IEventAggregator _eventAggregator;

        public ModelCheckerService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public bool Check(string sourceCode)
        {
            var status = parser.TryParseProgram(sourceCode, out var prog, out var internalerror, out var position);


            if (!status)
            {
                _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage(internalerror, SolverStatus.Error));
                
            }
            else
            {
                if (prog.ClassDefinitions.Count == 1)
                {

                    var translator = new ModelTranslatorV1();
                    var printer = new ModelicaTreePrinter();
                    var pretty = new ModelicaPrettyPrinter();
                    var instancePrinter = new InstancePrinter();
                    var flattening = new Flattening();

                    var astText = printer.Transform(prog);
                    var prettyTest = pretty.Transform(prog);
                    DAEProblem model = null;

                    string prettyflat = "";

                    try
                    {
                        var flatModel = flattening.Transform(prog.ClassDefinitions.First());
                        prettyflat = instancePrinter.Transform(flatModel);
                        //model = translator.Translate(flatModel);
                    }
                    catch(Exception e)
                    {
                        _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage($"{e.Message}", SolverStatus.Error));
                        return false;
                    }


                    _eventAggregator.PublishOnUIThreadAsync(new UpdateModelAnalysisMessage() { SyntaxTree = astText, FlattenedModel = prettyflat, CalculationModel=model });
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage("Model OK", SolverStatus.OK));
                }
                else
                    throw new InvalidOperationException("Multiple class definitions detected. Flattening can only resolve one class.");
            }

            return status;


        }

        public Task HandleAsync(RequestModelAnalysisMessage message, CancellationToken cancellationToken)
        {
            Check(message.SourceCode);

            return Task.CompletedTask;
            //return Task.Run(() => );
        }
    }
}
