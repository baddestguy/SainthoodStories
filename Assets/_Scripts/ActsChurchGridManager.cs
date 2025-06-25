using UnityEngine;
using System.Collections.Generic;
using Rewired.UI.ControlMapper;
using JetBrains.Annotations;

public class ActsChurchGridManager : MonoBehaviour
{
    public float GridSpacing = 3f;
    public int GridRadius = 10;
    public Camera WorldCamera;

    private HashSet<Vector2Int> occupiedPositions = new();
    private IGridShape gridShape;

    public string GridShapeChosen;

    private void Awake()
    {
        gridShape = ChooseRandomGridShape();
        GridShapeChosen = gridShape.ToString();
    }

    public Vector3? GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            Vector2Int coord = gridShape.GetRandomCoord(GridRadius);

            if (occupiedPositions.Contains(coord))
                continue;

            Vector3 worldPos = gridShape.GridToWorld(coord, GridSpacing);
            if (!IsVisibleToCamera(worldPos))
                continue;

            occupiedPositions.Add(coord);
            return worldPos;
        }

        return null;
    }

    bool IsVisibleToCamera(Vector3 worldPos)
    {
        Vector3 viewportPos = WorldCamera.WorldToViewportPoint(worldPos);
        return viewportPos.x > 0.01f && viewportPos.x < 0.99f && viewportPos.y > 0.01f && viewportPos.y < 0.99f;
    }

    IGridShape ChooseRandomGridShape()
    {
        var shapes = new List<IGridShape>
        {
            //new BlobGridShape(),
            //new ArchipelagoGridShape(),
            //new PlateauGridShape()
           // new SquareGrid(),
            //new HexGrid(),
            //new DiamondGrid(),
            //new CircularGrid(),
            new CrossGridShape()
            //new SpiralGrid()
        };

        int index = Random.Range(0, shapes.Count);
        return shapes[index];
    }
}


public interface IGridShape
{
    Vector2Int GetRandomCoord(int radius);
    Vector3 GridToWorld(Vector2Int gridCoord, float spacing);
}


public class SquareGrid : IGridShape
{
    public Vector2Int GetRandomCoord(int radius)
    {
        int x = Random.Range(-radius, radius + 1);
        int y = Random.Range(-radius, radius + 1);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridCoord, float spacing)
    {
        return new Vector3(gridCoord.x * spacing, gridCoord.y * spacing, 0f);
    }
}


public class HexGrid : IGridShape
{
    public Vector2Int GetRandomCoord(int radius)
    {
        int q = Random.Range(-radius, radius + 1);
        int r = Random.Range(Mathf.Max(-radius, -q - radius), Mathf.Min(radius, -q + radius));
        return new Vector2Int(q, r);
    }

    public Vector3 GridToWorld(Vector2Int gridCoord, float spacing)
    {
        float x = spacing * Mathf.Sqrt(3) * (gridCoord.x + gridCoord.y / 2f);
        float y = spacing * 1.5f * gridCoord.y;
        return new Vector3(x, y, 0f);
    }
}


public class DiamondGrid : IGridShape
{
    public Vector2Int GetRandomCoord(int radius)
    {
        int x = Random.Range(-radius, radius + 1);
        int y = Random.Range(-radius, radius + 1);
        if (Mathf.Abs(x + y) > radius) return GetRandomCoord(radius); // Diamond constraint
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridCoord, float spacing)
    {
        float x = spacing * (gridCoord.x - gridCoord.y);
        float y = spacing * (gridCoord.x + gridCoord.y) * 0.5f;
        return new Vector3(x, y, 0f);
    }
}

public class CircularGrid : IGridShape
{
    public Vector2Int GetRandomCoord(int radius)
    {
        for (int i = 0; i < 10; i++) // small retry loop
        {
            int x = Random.Range(-radius, radius + 1);
            int y = Random.Range(-radius, radius + 1);

            if (x * x + y * y <= radius * radius)
                return new Vector2Int(x, y);
        }
        return new Vector2Int(0, 0); // fallback
    }

    public Vector3 GridToWorld(Vector2Int coord, float spacing)
    {
        return new Vector3(coord.x * spacing, coord.y * spacing, 0f);
    }
}

public class SpiralGrid : IGridShape
{
    private int currentIndex = 0;

    public Vector2Int GetRandomCoord(int radius)
    {
        float angle = currentIndex * 0.5f;
        float dist = 0.5f * currentIndex;
        currentIndex++;

        int x = Mathf.RoundToInt(dist * Mathf.Cos(angle));
        int y = Mathf.RoundToInt(dist * Mathf.Sin(angle));

        if (Mathf.Abs(x) > radius || Mathf.Abs(y) > radius)
            currentIndex = 0;

        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int coord, float spacing)
    {
        return new Vector3(coord.x * spacing, coord.y * spacing, 0f);
    }
}

public class CrossGridShape : IGridShape
{
    private HashSet<Vector2Int> coords = new HashSet<Vector2Int>();
    private int thickness = 5;

    public Vector2Int GetRandomCoord(int radius)
    {
        if (coords.Count == 0)
            Generate(radius);

        int index = Random.Range(0, coords.Count);
        foreach (var coord in coords)
            if (index-- == 0)
                return coord;

        return Vector2Int.zero;
    }

    public Vector3 GridToWorld(Vector2Int gridCoord, float spacing)
    {
        return new Vector3(gridCoord.x * spacing, gridCoord.y * spacing, 0f);
    }

    private void Generate(int radius)
    {
        coords.Clear();

        int verticalLength = radius;
        int horizontalLength = (int)(radius * 1.25f);
        int barOffset = verticalLength / 2; // how far down the horizontal bar sits

        // Vertical shaft (centered at x = 0)
        for (int x = -thickness / 2; x <= thickness / 2; x++)
        {
            for (int y = -verticalLength; y <= verticalLength; y++)
            {
                coords.Add(new Vector2Int(x, y));
            }
        }

        // Horizontal bar (lower, not centered)
        int barY = verticalLength - barOffset;
        for (int y = barY - thickness / 2; y <= barY + thickness / 2; y++)
        {
            for (int x = -horizontalLength; x <= horizontalLength; x++)
            {
                coords.Add(new Vector2Int(x, y));
            }
        }

        // Optional: Top protrusion (slightly above the shaft)
        for (int x = -thickness / 2; x <= thickness / 2; x++)
        {
            for (int y = verticalLength + 1; y <= verticalLength + thickness; y++)
            {
                coords.Add(new Vector2Int(x, y));
            }
        }
    }
}


public abstract class BaseGridShape : IGridShape
{
    protected HashSet<Vector2Int> coords = new HashSet<Vector2Int>();

    public abstract void Generate(int radius);

    public Vector2Int GetRandomCoord(int radius)
    {
        if (coords.Count == 0)
            Generate(radius);

        int index = Random.Range(0, coords.Count);
        foreach (var coord in coords)
            if (index-- == 0)
                return coord;

        return Vector2Int.zero; // fallback
    }

    public Vector3 GridToWorld(Vector2Int gridCoord, float spacing)
    {
        return new Vector3(gridCoord.x * spacing, gridCoord.y * spacing, 0f);
    }
}

public class BlobGridShape : BaseGridShape
{
    public override void Generate(int radius)
    {
        coords.Clear();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int center = Vector2Int.zero;
        frontier.Enqueue(center);
        visited.Add(center);

        int maxCells = radius * radius;

        while (coords.Count < maxCells && frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            coords.Add(current);

            foreach (Vector2Int dir in Directions.OrthogonalAndDiagonal)
            {
                Vector2Int next = current + dir;

                if (!visited.Contains(next) && Random.value > 0.25f)
                {
                    visited.Add(next);
                    frontier.Enqueue(next);
                }
            }
        }
    }
}

public class ArchipelagoGridShape : BaseGridShape
{
    public override void Generate(int radius)
    {
        coords.Clear();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (pos.magnitude < radius && Random.value > 0.5f)
                    coords.Add(pos);
            }
        }
    }
}

public class PlateauGridShape : BaseGridShape
{
    public override void Generate(int radius)
    {
        coords.Clear();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                float noise = Mathf.PerlinNoise((x + 1000) * 0.1f, (y + 1000) * 0.1f);

                if (noise > 0.45f)
                    coords.Add(pos);
            }
        }
    }
}

public static class Directions
{
    public static readonly Vector2Int[] Orthogonal = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public static readonly Vector2Int[] Diagonal = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    public static readonly Vector2Int[] OrthogonalAndDiagonal = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };
}
