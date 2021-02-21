using Apps.ModelEditor.Interfaces;
using Apps.ModelEditor.Messaging;
using Caliburn.Micro;
using ModelingFramework.Core.Interfaces;
using ModelingFramework.Core.Logging;
using ModelingFramework.Core.Modelica;
using ModelingFramework.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Services
{
    public class ModelRunnerService : IModelRunner, IHandle<RequestSimulationMessage>
    {
        ModelicaParser parser = new ModelicaParser();
        IEventAggregator _eventAggregator;

        public ModelRunnerService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }


        void OnIteration(IIntegrator integrator, double progress)
        {
            _eventAggregator.PublishOnUIThreadAsync(new UpdateProgressBarMessage((int)((progress) * 100)));

        }

        void updateIntegratorSettings(ClassDefinition ast, IIntegrator integrator)
        {

            var annot = ast.Elements.FirstOrDefault(e => e is Annotation) as Annotation;
            if (annot != null)
            {
                var experiment = annot.Modification.Modifications.FirstOrDefault(m => m.Reference.ID == "experiment");
                if (experiment != null && experiment.Modification != null)
                {
                    foreach (var mod in experiment.Modification.Modifications)
                    {
                        if (mod.Reference.ID == "StopTime")
                            integrator.EndTime = mod.Modification.Value.Evaluate();
                        if (mod.Reference.ID == "Interval")
                            integrator.StepSize = mod.Modification.Value.Evaluate();
                        if (mod.Reference.ID == "Tolerance")
                            integrator.Tolerance = mod.Modification.Value.Evaluate();
                        if (mod.Reference.ID == "MinInterval")
                            integrator.MinStepSize = mod.Modification.Value.Evaluate();
                    }
                }
            }
        }

        IIntegrator createIntegratorFromAnnotation(ClassDefinition ast)
        {
            IIntegrator integrator = new BDF2();

            var annot = ast.Elements.FirstOrDefault(e => e is Annotation) as Annotation;
            if (annot != null)
            {
                var experiment = annot.Modification.Modifications.FirstOrDefault(m => m.Reference.ID == "experiment");
                if (experiment != null && experiment.Modification != null)
                {
                    foreach (var mod in experiment.Modification.Modifications)
                    {
                        if (mod.Reference.ID == "Integrator")
                            switch (mod.Modification.Value.ToString().Replace("\"", ""))
                            {
                                case "BDF1":
                                    integrator = new ImplicitEuler();
                                    break;
                                case "BDF2":
                                    integrator = new BDF2();
                                    break;
                                case "BDF2A":
                                    integrator = new BDF2A();
                                    break;
                            }

                        if (mod.Reference.ID == "Solver")
                        {
                            integrator.UseDulmageMendelsohnSolver = mod.Modification.Value.ToString() == "DMD";
                        }
                    }
                }
            }

            return integrator;
        }

        public bool Simulate(string sourceCode)
        {
            var status = parser.TryParseProgram(sourceCode, out var prog, out var error, out var position);
            if (status)
            {
                var translator = new ModelTranslatorV1();
                var flattening = new Flattening();

                var ast = prog.ClassDefinitions.Last();

                var instance = flattening.Transform(ast);

                DAEProblem model = null;

                try
                {
                    model = translator.Translate(instance);
                }
                catch (Exception e)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage($"{e.Message}", SolverStatus.Error));
                }



                var integrator = createIntegratorFromAnnotation(ast);

                integrator.StepSize = 0.1;
                integrator.EndTime = 1;
                integrator.OnIteration += (i) => OnIteration(integrator, i);

                updateIntegratorSettings(ast, integrator);

                var logger = new NoLogger();

                try
                {

                    integrator.Discretize(model);
                    model.Initialize(logger);
                }
                catch (Exception e)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage($"{e.Message}", SolverStatus.Error));
                    return false;
                }


                if (model.SystemToSolve.NumberOfEquations != model.SystemToSolve.NumberOfVariables)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage($"System not square! V={model.SystemToSolve.NumberOfVariables}, E={model.SystemToSolve.NumberOfEquations}", SolverStatus.Error));
                    return false;
                }

                Stopwatch w = new Stopwatch();
                _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage("Integration started", SolverStatus.Busy));

                try
                {
                    w.Start();
                    var results = integrator.Integrate(model, logger);
                    w.Stop();
                    Console.WriteLine("Integration took " + w.ElapsedMilliseconds + "ms");
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateModelResultMessage() { ModelName = model.Name, AlgebraicStates = model.AlgebraicVariables, DifferentialStates = model.DifferentialVariables, TimeSteps = results });
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage($"Integration finished ({w.ElapsedMilliseconds} ms)", SolverStatus.OK));
                }
                catch (Exception e)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage($"{e.Message}", SolverStatus.Error));
                    return false;
                }
            }
            else
            {
                _eventAggregator.PublishOnUIThreadAsync(new UpdateSolverStatusMessage(error, SolverStatus.Error));
            }


            return true;
        }

        public Task HandleAsync(RequestSimulationMessage message, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var status = Simulate(message.SourceCode);

            });


        }

    }
}
