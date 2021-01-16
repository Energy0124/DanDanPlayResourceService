using System.Collections.Generic;

namespace DanDanPlayResourceService.Models
{
    public class TypeWrap
    {
        public TypeWrap(List<Type> types)
        {
            Types = types;
        }

        public List<Type> Types { get; set; }
    }

    public class Type
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}