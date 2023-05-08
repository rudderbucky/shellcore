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
        return GetPartVertices(part.info, part.image.sprite, shrinkFactor);
    }

    public static Vector2[] GetPartVertices(EntityBlueprint.PartInfo info, Sprite partSprite = null, float shrinkFactor = 0f)
    {
        if (partSprite == null && ResourceManager.Instance.resourceExists(info.partID + "_sprite"))
        {
            partSprite = ResourceManager.GetAsset<Sprite>(info.partID + "_sprite");
        }
        if (partSprite == null) return new Vector2[4];
        var rect = partSprite.bounds;
        rect.size *= 100;
        rect.Expand(0.001f);

        float rot = info.rotation * Mathf.Deg2Rad;

        Matrix4x4 matrix = new()
        {
            m00 = Mathf.Cos(rot),
            m01 = -Mathf.Sin(rot),
            m10 = Mathf.Sin(rot),
            m11 = Mathf.Cos(rot),
        };

        if (!shrinkFactor.Equals(0f))
            rect.Expand(shrinkFactor * rect.extents);

        Vector2 center = info.location * 100f;
        Vector2 right = new(rect.extents.x, 0f);
        Vector2 up = new(0f, rect.extents.y);

        Vector2[] points = new Vector2[4];
        points[0] = (Vector2)(matrix * ( -right - up)) + center;
        points[1] = (Vector2)(matrix * (  right - up)) + center;
        points[2] = (Vector2)(matrix * (  right + up)) + center;
        points[3] = (Vector2)(matrix * ( -right + up)) + center;

        return points;
    }

    public static Vector2[] GetPartVertices(ShellPart part)
    {
        var partSprite = part.spriteRenderer.sprite;
        var rect = partSprite.bounds;
        var rot = part.transform.eulerAngles.z * Mathf.Deg2Rad;

        Matrix4x4 matrix = new()
        {
            m00 = Mathf.Cos(rot),
            m01 = -Mathf.Sin(rot),
            m10 = Mathf.Sin(rot),
            m11 = Mathf.Cos(rot),
        };

        Vector2 center = part.transform.position;
        Vector2 right = new(rect.extents.x, 0f);
        Vector2 up = new(0f, rect.extents.y);

        Vector2[] points = new Vector2[4];
        points[0] = (Vector2)(matrix * (-right - up)) + center;
        points[1] = (Vector2)(matrix * (right - up)) + center;
        points[2] = (Vector2)(matrix * (right + up)) + center;
        points[3] = (Vector2)(matrix * (-right + up)) + center;

        return points;
    }

    public static Vector2[] GetColliders(Entity entity)
    {
        float rot = entity.transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 pos = entity.transform.position;

        Matrix4x4 shipMatrix = new Matrix4x4()
        {
            m00 = Mathf.Cos(rot),
            m01 = -Mathf.Sin(rot),
            m10 = Mathf.Sin(rot),
            m11 = Mathf.Cos(rot),
            m03 = pos.x,
            m13 = pos.y,
        };

        Vector2[] vertices = new Vector2[4 * (entity.parts.Count + 1)];

        for (int i = 0; i < entity.parts.Count; i++)
        {
            var part = entity.parts[i];
            Vector3 right = new(part.colliderExtents.x, 0f);
            Vector3 up = new(0f, part.colliderExtents.y);

            vertices[i * 4 + 0] = shipMatrix.MultiplyPoint3x4(part.colliderMatrix.MultiplyPoint3x4(-right - up + Vector3.forward));
            vertices[i * 4 + 1] = shipMatrix.MultiplyPoint3x4(part.colliderMatrix.MultiplyPoint3x4( right - up + Vector3.forward));
            vertices[i * 4 + 2] = shipMatrix.MultiplyPoint3x4(part.colliderMatrix.MultiplyPoint3x4( right + up + Vector3.forward));
            vertices[i * 4 + 3] = shipMatrix.MultiplyPoint3x4(part.colliderMatrix.MultiplyPoint3x4(-right + up + Vector3.forward));
        }

        int offset = entity.parts.Count * 4;
        float size = 0.5f;
        if (entity is Drone)
            size = 0.25f;
        vertices[offset + 0] = shipMatrix.MultiplyPoint3x4(new Vector3(-size, -size));
        vertices[offset + 1] = shipMatrix.MultiplyPoint3x4(new Vector3( size, -size));
        vertices[offset + 2] = shipMatrix.MultiplyPoint3x4(new Vector3( size,  size));
        vertices[offset + 3] = shipMatrix.MultiplyPoint3x4(new Vector3(-size,  size));

        return vertices;
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

    public static bool PointInRectangle(Vector2 r0, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 point)
    {
        static float CrossProduct(Vector2 vertex1, Vector2 vertex2, Vector2 point)
        {
            return ((vertex2.x - vertex1.x) * (point.y - vertex1.y) - (point.x - vertex1.x) * (vertex2.y - vertex1.y));
        }

        return CrossProduct(r0, r1, point) > 0 
            && CrossProduct(r1, r2, point) > 0 
            && CrossProduct(r2, r3, point) > 0 
            && CrossProduct(r3, r0, point) > 0;
    }
}
