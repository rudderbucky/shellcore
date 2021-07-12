using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CircleGraphic : MaskableGraphic
{
    [SerializeField]
    private int vertices = 3;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Vector2 corner1 = Vector2.zero;
        Vector2 corner2 = Vector2.one;

        corner1 -= rectTransform.pivot;
        corner2 -= rectTransform.pivot;

        corner1.x *= rectTransform.rect.width;
        corner1.y *= rectTransform.rect.height;
        corner2.x *= rectTransform.rect.width;
        corner2.y *= rectTransform.rect.height;

        var xRadius = corner1.x;
        var yRadius = corner2.y;
        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;
        var degree = 360f / vertices;
        for (int i = 0; i < vertices; i++)
        {
            var x = xRadius * Mathf.Cos(degree * Mathf.Deg2Rad * i);
            var y = yRadius * Mathf.Sin(degree * Mathf.Deg2Rad * i);
            vert.position = new Vector2(x, y);
            vert.color = color;
            vh.AddVert(vert);
        }

        for (int i = 0; i < vertices - 2; i++)
        {
            vh.AddTriangle(i, i + 1, i + 2);
        }

        vh.AddTriangle(vertices - 2, vertices - 1, 0);
        vh.AddTriangle(vertices - 1, 0, 1);

        /*
        vert.position = new Vector2(corner1.x, corner1.y);
        vert.color = color;
        vh.AddVert(vert);

        vert.position = new Vector2(corner1.x, corner2.y);
        vert.color = color;
        vh.AddVert(vert);

        vert.position = new Vector2(corner2.x, corner2.y);
        vert.color = color;
        vh.AddVert(vert);

        vert.position = new Vector2(corner2.x, corner1.y);
        vert.color = color;
        vh.AddVert(vert);
        */


        //vh.AddTriangle(2, 3, 0);
    }
}
