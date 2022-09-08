using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SATCollision
{
    public static bool PartCollision(ShipBuilderPart p1, ShipBuilderPart p2)
    {
        return RectangleCollision(GetPartVertices(p1), GetPartVertices(p2));
    }

    public static bool TooCloseCollision(ShipBuilderPart p1, ShipBuilderPart p2, float shrinkFactor)
    {
        return RectangleCollision(GetPartVertices(p1, shrinkFactor), GetPartVertices(p2, shrinkFactor));
    }
    
    public static Vector2[] GetPartVertices(ShipBuilderPart part, float shrinkFactor = 0f)
    {
        if (part == null || part.image == null || part.image.sprite == null)
        {
            return new Vector2[0];
        }

        var rect = part.image.sprite.bounds;
        rect.size *= 100;
        rect.Expand(0.001f);

        float rot = part.info.rotation * Mathf.Deg2Rad;

        Matrix4x4 matrix = new Matrix4x4()
        {
            m00 = Mathf.Cos(rot),
            m01 = -Mathf.Sin(rot),
            m10 = Mathf.Sin(rot),
            m11 = Mathf.Cos(rot),
        };

        if (!shrinkFactor.Equals(0f))
            rect.Expand(shrinkFactor * rect.extents);

        Vector2 center = part.info.location * 100f;
        Vector2 right = new Vector2(rect.extents.x, 0f);
        Vector2 up = new Vector2(0f, rect.extents.y);

        Vector2[] points = new Vector2[4];
        points[0] = (Vector2)(matrix * ( -right - up)) + center;
        points[1] = (Vector2)(matrix * (  right - up)) + center;
        points[2] = (Vector2)(matrix * (  right + up)) + center;
        points[3] = (Vector2)(matrix * ( -right + up)) + center;

        return points;
    }

    public static bool RectangleCollision(Vector2[] points1, Vector2[] points2)
    {
        // Make sure we're dealing with rectangles
        if (points1.Length != 4 || points2.Length != 4)
        {
            Debug.LogWarning("Wrong amount of points!");
            return false;
        }

        // Two axes per shape, because with rectangles the others would be dupliates
        Vector2[] axes = new Vector2[]
        {
            points1[0] - points1[1],
            points1[1] - points1[2],
            points2[0] - points2[1],
            points2[1] - points2[2],
        };

        for (int i = 0; i < axes.Length; i++)
        {
            axes[i] = axes[i].normalized;
        }

        // For all axes
        for (int i = 0; i < 4; i++)
        {
            float min1 = float.PositiveInfinity;
            float min2 = min1;

            float max1 = float.NegativeInfinity;
            float max2 = max1;

            // Project points 1
            for (int j = 0; j < points1.Length; j++)
            {
                var projection = Vector2.Dot(points1[j], axes[i]);
                min1 = Mathf.Min(min1, projection);
                max1 = Mathf.Max(max1, projection);
            }

            // Project points 2
            for (int j = 0; j < points2.Length; j++)
            {
                var projection = Vector2.Dot(points2[j], axes[i]);
                min2 = Mathf.Min(min2, projection);
                max2 = Mathf.Max(max2, projection);
            }

            // Check if projections are separated
            if (!(max1 >= min2 && min1 <= max2))
            {
                return false;
            }
        }

        // If no separating axis is found, shapes are colliding
        return true;
    }
}
