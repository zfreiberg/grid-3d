using System.Collections.Generic;

/// <summary>
/// Static bridge that carries the current node's data into the destination scene.
/// Set before loading a node scene; read in the scene's controller.
/// </summary>
public static class NodeSceneContext
{
    public static NodeData CurrentNode { get; set; }
}

/// <summary>
/// Maps NodeType values to Unity scene names.
/// Add new node types here as scenes are created.
/// </summary>
public static class NodeTypeSceneMap
{
    static readonly Dictionary<NodeType, string> Scenes = new()
    {
        { NodeType.Battle, "GridScene01" },  // placeholder — update when BattleScene exists
        { NodeType.Event,  "EventScene"  },
        { NodeType.Shop,   "ShopScene"   },
        { NodeType.Rest,   "RestScene"   },
        { NodeType.Boss,   "BossScene"   },
    };

    public static string GetScene(NodeType type) =>
        Scenes.TryGetValue(type, out var name) ? name : "MapScene";
}
