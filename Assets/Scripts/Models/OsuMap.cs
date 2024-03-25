using System.Collections.Generic;

namespace DefaultNamespace.Models
{
    public class OsuMap
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public Dictionary<string, OsuMapVersion> Versions { get; set; } = new Dictionary<string, OsuMapVersion>();
    }
}