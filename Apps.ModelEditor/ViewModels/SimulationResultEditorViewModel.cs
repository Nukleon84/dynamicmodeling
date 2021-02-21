using System;
using System.Collections.Generic;
using System.Text;
using Apps.ModelEditor.Messaging;
using Apps.ModelEditor.Interfaces;
using OxyPlot;
using OxyPlot.Series;
using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;

namespace Apps.ModelEditor.ViewModels
{
    public class SimulationResultEditorViewModel : Screen, ISimulationResultEditor, IHandle<UpdateModelResultMessage>, IHandle<UpdateModelAnalysisMessage>
    {
        public PlotModel TrendPlotModel { get; private set; }
        public PlotModel PhasePlotModel { get; private set; }
        IEventAggregator _eventAggregator;
        public TextDocument FlatModelDocument { get; set; }
        public TextDocument SyntaxTreeDocument { get; set; }
        public TextDocument InstanceTreeDocument { get; set; }

        public SimulationResultEditorViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            this.TrendPlotModel = new PlotModel { Title = "Example 1" };
            this.TrendPlotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
            this.PhasePlotModel = new PlotModel { Title = "Example 1" };
            this.PhasePlotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));


        }

        public Task HandleAsync(UpdateModelResultMessage message, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {

                this.TrendPlotModel = new PlotModel { Title = message.ModelName };

                for (int i = 0; i < message.AlgebraicStates.Count; i++)
                {
                    var y = message.AlgebraicStates[i];
                    var series = new LineSeries();
                    series.Title = y.Name;
                    
                    if (message.TimeSteps.Count < 5000)
                        series.MarkerType = MarkerType.Circle;

                    for (int j = 0; j < message.TimeSteps.Count; j++)
                    {
                        series.Points.Add(new DataPoint(message.TimeSteps[j].Time, message.TimeSteps[j].AlgebraicStates[i]));
                    }
                    this.TrendPlotModel.Series.Add(series);
                }


                this.PhasePlotModel = new PlotModel { Title = message.ModelName + " (Phase Plot)" };
                int v1 = 0;
                int v2 = 1;

                var phaseseries = new LineSeries();
                phaseseries.Title = "Phase Portrait";
                for (int j = 0; j < message.TimeSteps.Count; j++)
                {
                    phaseseries.Points.Add(new DataPoint(message.TimeSteps[j].AlgebraicStates[v1], message.TimeSteps[j].AlgebraicStates[v2]));
                }

                this.PhasePlotModel.Series.Add(phaseseries);

                // PhasePlotModel.DefaultXAxis.Title = message.AlgebraicStates[v1].Name;
                // PhasePlotModel.DefaultYAxis.Title = message.AlgebraicStates[v2].Name;


                NotifyOfPropertyChange(() => TrendPlotModel);
                NotifyOfPropertyChange(() => PhasePlotModel);
            });

        }

        public Task HandleAsync(UpdateModelAnalysisMessage message, CancellationToken cancellationToken)
        {
            SyntaxTreeDocument = new TextDocument(message.SyntaxTree);
            if (!String.IsNullOrEmpty(message.InstanceTree))
                InstanceTreeDocument = new TextDocument(message.InstanceTree);
            if (!String.IsNullOrEmpty(message.FlattenedModel))
                FlatModelDocument = new TextDocument(message.FlattenedModel);

            NotifyOfPropertyChange(() => SyntaxTreeDocument);
            NotifyOfPropertyChange(() => FlatModelDocument);
            NotifyOfPropertyChange(() => InstanceTreeDocument);
            return Task.CompletedTask;
        }
    }
}
