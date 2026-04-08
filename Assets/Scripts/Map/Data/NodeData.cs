using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single node on the act map.
/// Only the payload matching the node's type is populated.
/// </summary>
[Serializable]
public class NodeData
{
    public string       id;
    public NodeType     type;
    public int          column;
    public int          row;
    public List<string> nextNodeIds = new();

    // Type-specific payloads — only the relevant one is populated
    public BattlePayload battle;
    public EventPayload  eventPayload;
    public ShopPayload   shop;
    public RestPayload   rest;
    public BossPayload   boss;
}

[Serializable]
public class BattlePayload
{
    public int    enemyTier;    // 1–5, scales with act + column depth
    public string enemySetId;   // references a predefined enemy configuration
}

[Serializable]
public class EventPayload
{
    public string eventId;  // references a predefined event definition
    public int    seed;     // seeded random for event-specific variation
}

[Serializable]
public class ShopPayload
{
    public int inventorySeed;   // drives seeded inventory generation
    public int shopTier;        // 1–3, based on act
}

[Serializable]
public class RestPayload
{
    public int healPercent; // 25–35%
}

[Serializable]
public class BossPayload
{
    public string bossId;   // references a predefined boss definition
    public int    act;      // which act this boss belongs to
}
