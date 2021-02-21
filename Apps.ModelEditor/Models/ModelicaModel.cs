using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Models
{
    public class ModelicaModel : Entity
    {
         public string SourceCode { get; set; }

        public ModelicaModel(string name, string path)
        {
            Name = name;
            Path = path;
            Type = EntityType.Model;
         
        }
    }
}
