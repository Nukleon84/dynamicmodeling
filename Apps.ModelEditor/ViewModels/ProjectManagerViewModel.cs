using Apps.ModelEditor.Interfaces;
using Apps.ModelEditor.Messaging;
using Apps.ModelEditor.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Apps.ModelEditor.ViewModels
{
    public class ProjectManagerViewModel : IProjectManager, IHandle<RequestSaveModelMessage>
    {
        public Entity Root { get; set; }
        public Entity SelectedEntity { get; set; }

        IEventAggregator _eventAggregator;

        public ProjectManagerViewModel(IEventAggregator eventAggregator, string rootPath)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
            var root = new Folder() { Name = "root", Path = rootPath };
            Explore(root);
            Root = root;
        }

        void Explore(Entity node)
        {
            var subdirs = Directory.EnumerateDirectories(node.Path);
            foreach (var subdir in subdirs)
            {
                var di = new DirectoryInfo(subdir);
                var child = new Folder() { Name = di.Name, Path = subdir };
                Explore(child);
                node.Children.Add(child);
            }

            var models = Directory.EnumerateFiles(node.Path, "*.mo");
            foreach (var model in models)
            {
                var child = new ModelicaModel(Path.GetFileName(model), model);
                node.Children.Add(child);
            }
        }
        public void RequestEdit(ModelicaModel context)
        {
            if (context != null)
            {
                if (File.Exists(context.Path))
                    context.SourceCode = File.ReadAllText(context.Path);

                _eventAggregator.PublishOnUIThreadAsync(new RequestModelEditorMessage() { Owner = context });
            }

        }

        public void SelectItem(object source)
        {
            if (source != null)
            {
                var treeview = source as TreeView;
                if (treeview != null)
                {
                    SelectedEntity = treeview.SelectedItem as Entity;
                }
            }
            //SelectedEntity = source;
        }

        public Task HandleAsync(RequestSaveModelMessage message, CancellationToken cancellationToken)
        {
            if (File.Exists(message.Path))
                File.WriteAllText(message.Path, message.SourceCode);

            return Task.CompletedTask;
        }
    }
}
