using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Apps.ModelEditor.Views
{
    /// <summary>
    /// Interaktionslogik für SimulationResultEditorView.xaml
    /// </summary>
    public partial class SimulationResultEditorView : UserControl
    {
        public SimulationResultEditorView()
        {
            InitializeComponent();
            LoadSyntaxHighlighting("Apps.ModelEditor.Resources.Modelica.xshd");

        }

        public void LoadSyntaxHighlighting(string name)
        {

            var names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    syntaxTreeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                  //  instanceTreeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    flatModelEditor.SyntaxHighlighting = syntaxTreeEditor.SyntaxHighlighting;
                }
            }
        }


    }
}
