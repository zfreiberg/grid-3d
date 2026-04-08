using System;
using System.Collections.Generic;

/// <summary>
/// Tracks the player's progress through the current act map.
/// Uses List instead of HashSet so Unity can serialize this later.
/// </summary>
[Serializable]
public class MapState
{
    public string       currentNodeId;
    public List<string> completedNodeIds  = new();
    public List<string> accessibleNodeIds = new();

    public bool IsCompleted(string id)  => completedNodeIds.Contains(id);
    public bool IsAccessible(string id) => accessibleNodeIds.Contains(id);
    public bool IsCurrent(string id)    => id == currentNodeId;
}
