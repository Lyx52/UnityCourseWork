using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DefaultNamespace.Models
{
    public class OsuMap
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public Dictionary<string, OsuMapVersion> Versions { get; set; } = new Dictionary<string, OsuMapVersion>();
        [CanBeNull] public string BackgroundImage => Versions.Values.FirstOrDefault()?.BackgroundImage;
        [CanBeNull] public string AudioFile => Versions.Values.FirstOrDefault()?.AudioFile;
    }
}