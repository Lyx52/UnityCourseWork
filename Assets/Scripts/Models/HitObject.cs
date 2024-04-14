using UnityEngine;

namespace DefaultNamespace.Models
{
    public class HitObject
    {
        public uint Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public uint StartTime { get; set; }
        public uint EndTime { get; set; }
        public uint Lifetime { get; set; }
        public uint Type { get; set; }
        public Vector2 Position => new Vector2(X, Y);
    }
}