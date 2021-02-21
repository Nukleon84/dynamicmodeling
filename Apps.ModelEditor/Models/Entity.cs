using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.ModelEditor.Models
{
    public enum EntityType { Folder, Model };
    public class Entity
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public EntityType Type { get; protected set; }        
        public List<Entity> Children { get; set; } = new List<Entity>();
        public Entity Parent { get; set; }

    }
}
