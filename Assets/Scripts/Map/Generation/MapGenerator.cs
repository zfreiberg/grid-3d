using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SysRandom = System.Random;

/// <summary>
/// Generates a seeded, act-specific node map.
/// Call MapGenerator.Generate(settings) to produce a MapData.
/// </summary>
public static class MapGenerator
{
    public static MapData Generate(MapSettings settings)
    {
        // Build a deterministic seed that varies per act and difficulty
        int baseSeed = settings.seed != 0 ? settings.seed : UnityEngine.Random.Range(1, int.MaxValue);
        int mixedSeed = baseSeed ^ (settings.act * 31337) ^ (settings.difficulty * 7919);
        var rng = new SysRandom(mixedSeed);

        int bossCol    = settings.battleColumns;      // last column index
        int totalCols  = settings.battleColumns + 1;
        int maxRows    = settings.maxNodesPerCol;

        var map = new MapData { act = settings.act, seed = baseSeed, totalColumns = totalCols };

        // --- Step 1: Create nodes ---
        for (int col = 0; col < totalCols; col++)
        {
            if (col == bossCol)
            {
                int centerRow = maxRows / 2;
                map.nodes.Add(new NodeData
                {
                    id     = $"n_{col}_{centerRow}",
                    type   = NodeType.Boss,
                    column = col,
                    row    = centerRow,
                    boss   = new BossPayload { bossId = $"boss_act{settings.act}", act = settings.act }
                });
                continue;
            }

            // First column always has ≥2 nodes so the player has a choice from the start
            int minCount   = col == 0 ? Mathf.Min(2, maxRows) : 1;
            int nodeCount  = rng.Next(minCount, maxRows + 1);
            var rows       = PickRows(nodeCount, maxRows, rng);

            foreach (int row in rows)
                map.nodes.Add(CreateNode(col, row, bossCol, settings, rng));
        }

        // --- Step 2: Connect columns ---
        ConnectColumns(map, bossCol, rng);

        return map;
    }

    // -------------------------------------------------------------------------
    // Node creation
    // -------------------------------------------------------------------------

    static NodeData CreateNode(int col, int row, int bossCol, MapSettings settings, SysRandom rng)
    {
        var node = new NodeData { id = $"n_{col}_{row}", column = col, row = row };
        node.type = AssignType(col, bossCol, settings, rng);
        PopulatePayload(node, settings, rng);
        return node;
    }

    static NodeType AssignType(int col, int bossCol, MapSettings settings, SysRandom rng)
    {
        // Column just before boss: rest or shop to prepare the player
        if (col == bossCol - 1)
            return rng.NextDouble() < 0.5 ? NodeType.Rest : NodeType.Shop;

        // Normalised position through the act (0=start, 1=just before pre-boss)
        float t = bossCol > 2 ? (float)col / (bossCol - 2) : 0f;
        t = Mathf.Clamp01(t);

        if (t < 0.33f)
            return WeightedPick(rng,
                (NodeType.Battle, 0.65), (NodeType.Event, 0.25),
                (NodeType.Shop,   0.10), (NodeType.Rest,  0.00));
        if (t < 0.67f)
            return WeightedPick(rng,
                (NodeType.Battle, 0.50), (NodeType.Event, 0.20),
                (NodeType.Shop,   0.15), (NodeType.Rest,  0.15));

        return WeightedPick(rng,
            (NodeType.Battle, 0.45), (NodeType.Event, 0.20),
            (NodeType.Shop,   0.15), (NodeType.Rest,  0.20));
    }

    static NodeType WeightedPick(SysRandom rng, params (NodeType type, double weight)[] options)
    {
        double roll = rng.NextDouble();
        foreach (var (type, weight) in options)
        {
            if (roll < weight) return type;
            roll -= weight;
        }
        return options[0].type;
    }

    static void PopulatePayload(NodeData node, MapSettings settings, SysRandom rng)
    {
        int tier = Mathf.Clamp(settings.act, 1, 5);
        switch (node.type)
        {
            case NodeType.Battle:
                node.battle = new BattlePayload
                {
                    enemyTier  = tier,
                    enemySetId = $"enemies_act{settings.act}_tier{tier}"
                };
                break;
            case NodeType.Event:
                node.eventPayload = new EventPayload
                {
                    eventId = $"event_{rng.Next(1, 11)}",   // 10 predefined events
                    seed    = rng.Next()
                };
                break;
            case NodeType.Shop:
                node.shop = new ShopPayload
                {
                    inventorySeed = rng.Next(),
                    shopTier      = settings.act
                };
                break;
            case NodeType.Rest:
                node.rest = new RestPayload
                {
                    healPercent = 25 + rng.Next(0, 11)  // 25–35 %
                };
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Connection logic
    // -------------------------------------------------------------------------

    static void ConnectColumns(MapData map, int bossCol, SysRandom rng)
    {
        for (int col = 0; col < bossCol; col++)
        {
            var current = map.GetColumn(col).OrderBy(n => n.row).ToList();
            var next    = map.GetColumn(col + 1).OrderBy(n => n.row).ToList();
            if (next.Count == 0) continue;

            var reached = new HashSet<string>();

            foreach (var node in current)
            {
                // Prefer connecting to nodes in the same or adjacent row
                var candidates = next.Where(n => Mathf.Abs(n.row - node.row) <= 1).ToList();
                if (candidates.Count == 0) candidates = next.ToList();

                int count  = Mathf.Min(rng.Next(1, 3), candidates.Count);
                var chosen = candidates.OrderBy(_ => rng.Next()).Take(count).ToList();

                foreach (var target in chosen)
                {
                    if (!node.nextNodeIds.Contains(target.id))
                        node.nextNodeIds.Add(target.id);
                    reached.Add(target.id);
                }
            }

            // Guarantee every next-column node has at least one incoming connection
            foreach (var nextNode in next.Where(n => !reached.Contains(n.id)))
            {
                var closest = current.OrderBy(n => Mathf.Abs(n.row - nextNode.row)).First();
                if (!closest.nextNodeIds.Contains(nextNode.id))
                    closest.nextNodeIds.Add(nextNode.id);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    static List<int> PickRows(int count, int maxRows, SysRandom rng)
    {
        var rows = Enumerable.Range(0, maxRows).ToList();
        // Fisher-Yates shuffle
        for (int i = rows.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (rows[i], rows[j]) = (rows[j], rows[i]);
        }
        var picked = rows.Take(count).ToList();
        picked.Sort();
        return picked;
    }
}
