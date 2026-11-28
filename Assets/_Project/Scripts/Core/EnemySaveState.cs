using System;
using UnityEngine;

namespace LightShadow.Core
{
    /// <summary>
    /// Pure helpers for the enemy's part of a save (kept out of GameManager so
    /// EditMode tests need no scene). Stored under the contrib key "enemy".
    /// </summary>
    public static class EnemySaveState
    {
        public const string Key = "enemy";

        // A restored wraith must not land on top of the player.
        public const float MinPlayerDistance = 10f;

        [Serializable]
        public class Blob
        {
            public float x, y, z;
            public float gauge;
            public bool aggressive;
            public Vector3 Position { get { return new Vector3(x, y, z); } }
        }

        public static string ToJson(Vector3 position, float gauge, bool aggressive)
        {
            Blob blob = new Blob { x = position.x, y = position.y, z = position.z, gauge = gauge, aggressive = aggressive };
            return JsonUtility.ToJson(blob);
        }

        public static bool TryParse(string json, out Blob blob)
        {
            blob = null;
            if (string.IsNullOrEmpty(json)) return false;
            try { blob = JsonUtility.FromJson<Blob>(json); }
            catch (Exception) { blob = null; }
            if (blob == null) return false;
            return Finite(blob.x) && Finite(blob.y) && Finite(blob.z) && Finite(blob.gauge);
        }

        public static bool ShouldPlace(Vector3 saved, Vector3 player)
        {
            if (!Finite(saved.x) || !Finite(saved.y) || !Finite(saved.z)) return false;
            return Vector3.Distance(saved, player) >= MinPlayerDistance;
        }

        static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
    }
}
