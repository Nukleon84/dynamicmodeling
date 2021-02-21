using Apps.ModelEditor.Interfaces;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using Apps.ModelEditor.Messaging;
using System.Threading;

namespace Apps.ModelEditor.ViewModels
{
    public class ModelEditorViewModel : Screen, IModelEditor, IHandle<RequestModelEditorMessage>
    {
        IEventAggregator _eventAggregator;
        public TextDocument Document { get; set; }

        public string Text { get; set; }
        public string Filename { get; set; }
        public bool HasUnsavedChanges { get; set; } = false;

        public ModelEditorViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
            Document = new TextDocument();

        }

        public void OnTextChanged(object sender, TextChangeEventArgs args)
        {
            HasUnsavedChanges = true;
            Text = Document.Text;
            _eventAggregator.PublishOnUIThreadAsync(new RequestSyntaxCheckMessage() { SourceCode = Text });
            NotifyOfPropertyChange(() => HasUnsavedChanges);
        }
        public void CheckModel()
        {
            _eventAggregator.PublishOnUIThreadAsync(new RequestModelAnalysisMessage() { SourceCode = Text });
        }
        public void ExecuteModel()
        {
            Text = Document.Text;

            _eventAggregator.PublishOnUIThreadAsync(new RequestSimulationMessage() { SourceCode = Text });

        }
        public void SaveModel()
        {
            Text = Document.Text;
            _eventAggregator.PublishOnUIThreadAsync(new RequestSaveModelMessage() { SourceCode = Text, Path = Filename });
            HasUnsavedChanges = false;
            NotifyOfPropertyChange(() => HasUnsavedChanges);
        }

        public Task HandleAsync(RequestModelEditorMessage message, CancellationToken cancellationToken)
        {
            Filename = message.Owner.Path;
            Document.Text = message.Owner.SourceCode;
            HasUnsavedChanges = false;
            NotifyOfPropertyChange(() => Document);
            NotifyOfPropertyChange(() => HasUnsavedChanges);
            return Task.CompletedTask;
        }
    }
}
