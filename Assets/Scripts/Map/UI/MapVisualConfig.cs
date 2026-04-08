using UnityEngine;

/// <summary>
/// ScriptableObject that drives all map visual assets.
/// Create via: Assets > Create > Grid3D > Map Visual Config
/// Assign to the MapController component on MapRoot in MapScene.
/// </summary>
[CreateAssetMenu(fileName = "MapVisualConfig", menuName = "Grid3D/Map Visual Config")]
public class MapVisualConfig : ScriptableObject
{
    [Header("Defaults")]
    [Tooltip("Sprite used for all nodes unless a per-type override is set. Leave blank to use Unity's built-in Knob.")]
    public Sprite  defaultNodeSprite;
    [Tooltip("Material applied to all node and dot images.")]
    public Material defaultNodeMaterial;
    [Tooltip("Sprite used for path dots. Leave blank to use Unity's built-in Knob.")]
    public Sprite  dotSprite;

    [Header("Per-Type Node Sprites (optional overrides)")]
    public Sprite battleSprite;
    public Sprite eventSprite;
    public Sprite shopSprite;
    public Sprite restSprite;
    public Sprite bossSprite;

    /// <summary>Returns the sprite for the given node type, falling back to defaultNodeSprite.</summary>
    public Sprite GetNodeSprite(NodeType type)
    {
        Sprite s = type switch
        {
            NodeType.Battle => battleSprite,
            NodeType.Event  => eventSprite,
            NodeType.Shop   => shopSprite,
            NodeType.Rest   => restSprite,
            NodeType.Boss   => bossSprite,
            _               => null
        };
        return s != null ? s : defaultNodeSprite;
    }
}
