using Apps.ModelEditor.Interfaces;
using Apps.ModelEditor.Messaging;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apps.ModelEditor.ViewModels
{
    public class ShellViewModel : Screen, IShell,
        IHandle<UpdateSolverStatusMessage>,
        IHandle<UpdateProgressBarMessage>      
    {

        IEventAggregator _eventAggregator;
        ISyntaxChecker _syntaxCheckerService;
        IModelRunner _modelRunnerService;
        IModelChecker _modelCheckerService;

        int _currentProgress = 20;
        string _simulationStatusColor = "YellowGreen";
        string _simulationStatusMessage = "OK";

        public string SimulationStatusMessage
        {
            get
            {
                return _simulationStatusMessage;
            }

            set
            {
                _simulationStatusMessage = value;
                NotifyOfPropertyChange(() => SimulationStatusMessage);
            }
        }
        public string SimulationStatusColor
        {
            get
            {
                return _simulationStatusColor;
            }

            set
            {
                _simulationStatusColor = value;
                NotifyOfPropertyChange(() => SimulationStatusColor);
            }
        }
        public int CurrentProgress
        {
            get
            {
                return _currentProgress;
            }

            set
            {
                _currentProgress = value;
                NotifyOfPropertyChange(() => CurrentProgress);
            }
        }

        public IModelEditor ModelEditor { get; private set; }
        public ISimulationResultEditor SimulationResultEditor { get; private set; }
        public IProjectManager ProjectManager { get; private set; }

        public ShellViewModel(IEventAggregator eventAggregator,
            ISyntaxChecker syntaxChecker,
             IModelRunner modelRunnerService,
             IModelChecker modelCheckerService)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
            _syntaxCheckerService = syntaxChecker;
            _modelRunnerService = modelRunnerService;
            _modelCheckerService = modelCheckerService;

            ModelEditor = new ModelEditorViewModel(eventAggregator);
            SimulationResultEditor = new SimulationResultEditorViewModel(eventAggregator);
            ProjectManager = new ProjectManagerViewModel(eventAggregator, @"G:\Modelica\My Library");
        }

        public Task HandleAsync(UpdateSolverStatusMessage message, CancellationToken cancellationToken)
        {
            var colorName = "DimGray";
            switch (message.StatusCode)
            {
                case SolverStatus.OK:
                    colorName = "YellowGreen";
                    break;
                case SolverStatus.Warning:
                    colorName = "DarkYellow";
                    break;
                case SolverStatus.Error:
                    colorName = "Red";
                    break;
                case SolverStatus.Fatal:
                    colorName = "DarkRed";
                    break;
                case SolverStatus.Busy:
                    colorName = "LightBlue";
                    break;

            }
            SimulationStatusMessage = message.StatusMessage;
            SimulationStatusColor = colorName;
            return Task.CompletedTask;
        }

        public Task HandleAsync(UpdateProgressBarMessage message, CancellationToken cancellationToken)
        {
            CurrentProgress = message.CurrentProgress;
            return Task.CompletedTask;
        }
             
    }
}
