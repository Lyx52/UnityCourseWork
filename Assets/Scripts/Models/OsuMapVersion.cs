using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace DefaultNamespace.Models
{
    public class OsuMapVersion
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Version { get; set; }
        public string MetadataFile { get; set; }
        public string BackgroundImage { get; set; }
        public string Location { get; set; }
        public string AudioFile { get; set; }
        public float OverallDifficulty { get; set; }
        [CanBeNull] public AudioClip Audio { get; set; }
        public List<HitObject> HitObjects { get; set; } = new List<HitObject>();

        public Queue<HitObject> GetHitObjectQueue() => new Queue<HitObject>(HitObjects);
        public string FullTitle => $"{Title}, {Artist}, {Version}";
        public float ZSpeed { get; set; }
    }
}