using UnityEngine;

[CreateAssetMenu(menuName = "Tactics/Tile Type Library", fileName = "TileTypeLibrary")]
public class TileTypeLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public TileType type;
        public string   displayName;
        public Material material;
        public int      moveCost;
        public bool     blocksMovement;
        public Color    paletteColor; // used by the scene painter overlay
    }

    public Entry[] entries;

    public Entry GetEntry(TileType type)
    {
        if (entries != null)
            foreach (var e in entries)
                if (e.type == type) return e;

        // Fallback: default entry with moveCost = 1
        return new Entry { type = type, moveCost = 1 };
    }
}
