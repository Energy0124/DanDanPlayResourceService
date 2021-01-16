using System.Collections.Generic;

namespace DanDanPlayResourceService.Models
{
    public class SearchResult
    {
        public SearchResult(List<Resource> resources)
        {
            Resources = resources;
        }

        public bool HasMore { get; set; }
        public List<Resource> Resources { get; set; }
    }

    public class Resource
    {
        public string Title { get; set; } = "";
        public int TypeId { get; set; }
        public string TypeName { get; set; } = "";
        public int SubgroupId { get; set; }
        public string SubgroupName { get; set; } = "";
        public string Magnet { get; set; } = "";
        public string PageUrl { get; set; } = "";
        public string FileSize { get; set; } = "";
        public string PublishDate { get; set; } = "";
    }
}