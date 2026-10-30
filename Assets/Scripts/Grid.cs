using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathGrid : MonoBehaviour
{
    [SerializeField] private Tilemap pathTilemap;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector3Int startCell3 = pathTilemap.WorldToCell(startWorld);
        Vector3Int targetCell3 = pathTilemap.WorldToCell(targetWorld);

        Vector2Int start = new Vector2Int(startCell3.x, startCell3.y);
        Vector2Int target = new Vector2Int(targetCell3.x, targetCell3.y);

        // Make sure start/target are actually on the path.
        start = FindNearestWalkable(start);
        target = FindNearestWalkable(target);

        if (!IsWalkable(start) || !IsWalkable(target))
        {
            Debug.LogError("Could not find valid start or target cell.");
            return new List<Vector3>();
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent =
            new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        parent[start] = start;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == target)
                break;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (!IsWalkable(next))
                    continue;

                if (parent.ContainsKey(next))
                    continue;

                parent[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!parent.ContainsKey(target))
        {
            Debug.LogWarning("No path found.");
            return new List<Vector3>();
        }

        // Reconstruct path.
        List<Vector2Int> cells = new List<Vector2Int>();

        Vector2Int currentCell = target;

        while (currentCell != start)
        {
            cells.Add(currentCell);
            currentCell = parent[currentCell];
        }

        cells.Add(start);
        cells.Reverse();

        // Convert cells back into world positions.
        List<Vector3> worldPath = new List<Vector3>();

        foreach (Vector2Int cell in cells)
        {
            Vector3Int cell3 = new Vector3Int(cell.x, cell.y, 0);
            worldPath.Add(pathTilemap.GetCellCenterWorld(cell3));
        }

        return worldPath;
    }

    private bool IsWalkable(Vector2Int cell)
    {
        Vector3Int cell3 = new Vector3Int(cell.x, cell.y, 0);
        return pathTilemap.HasTile(cell3);
    }

    private Vector2Int FindNearestWalkable(Vector2Int origin)
    {
        if (IsWalkable(origin))
            return origin;

        // Search outward for a nearby path cell.
        for (int radius = 1; radius <= 10; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int candidate =
                        origin + new Vector2Int(x, y);

                    if (IsWalkable(candidate))
                        return candidate;
                }
            }
        }

        return origin;
    }
}