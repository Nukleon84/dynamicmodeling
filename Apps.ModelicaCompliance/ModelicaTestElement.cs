using System;
using System.Collections.Generic;
using System.Text;

namespace Apps.ModelicaCompliance
{
    public enum ModelicaTestElementType { Folder, Model};
    public class ModelicaTestElement
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public double PercentParsed { get; set; } = 0;
        public double PercentTranslated { get; set; } = 0;
        public double PercentSolved { get; set; } = 0;

        public int TotalSubelements { get; set; }

        public ModelicaTestElementType ElementType { get; set; } = ModelicaTestElementType.Folder;

        public List<ModelicaTestElement> Children { get; set; } = new List<ModelicaTestElement>();
    }
}
