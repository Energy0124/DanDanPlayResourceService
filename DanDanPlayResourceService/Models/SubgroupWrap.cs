using System.Collections.Generic;

namespace DanDanPlayResourceService.Models
{
    public class SubgroupWrap
    {
        public SubgroupWrap(List<Subgroup> subgroups)
        {
            Subgroups = subgroups;
        }

        public List<Subgroup> Subgroups { get; set; }
    }

    public class Subgroup
    {
        public Subgroup(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}