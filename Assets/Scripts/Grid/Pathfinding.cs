using System.Collections.Generic;

public static class Pathfinding
{
    public static void ComputeReachable(
        GridManager grid,
        GridCoord start,
        int movePoints,
        out HashSet<GridCoord> reachable,
        out Dictionary<GridCoord, GridCoord> cameFrom,
        out Dictionary<GridCoord, int> costSoFar
    )
    {
        reachable = new HashSet<GridCoord>();
        cameFrom = new Dictionary<GridCoord, GridCoord>();
        costSoFar = new Dictionary<GridCoord, int>();

        var frontier = new Queue<GridCoord>();
        frontier.Enqueue(start);

        reachable.Add(start);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            foreach (var next in grid.GetNeighbors4(current))
            {
                if (!grid.TryGetTile(next, out var tile)) continue;
                if (tile.BlocksMovement) continue;

                int newCost = costSoFar[current] + tile.BaseMoveCost;
                if (newCost > movePoints) continue;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    cameFrom[next] = current;

                    if (!reachable.Contains(next))
                        reachable.Add(next);

                    frontier.Enqueue(next);
                }
            }
        }
    }

    public static List<GridCoord> ReconstructPath(
        Dictionary<GridCoord, GridCoord> cameFrom,
        GridCoord start,
        GridCoord goal
    )
    {
        var path = new List<GridCoord>();
        if (!cameFrom.ContainsKey(goal))
            return path;

        var current = goal;
        path.Add(current);

        while (current != start)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
