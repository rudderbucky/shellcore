using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class UILandPlatformRenderer : MaskableGraphic
{
    public Vector2[] vertices;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (vertices.Length == 0)
        {
            vh.Clear();
            return;
        }

        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    vertices[i] -= rectTransform.pivot;
        //    vertices[i] *= rectTransform.rect.size;
        //}

        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;

        for (int i = 0; i < vertices.Length; i++)
        {
            vert.position = new Vector2(vertices[i].x, vertices[i].y);
            vert.color = color;
            vh.AddVert(vert);
        }

        for (int i = 0; i < vertices.Length / 4; i++)
        {
            vh.AddTriangle(i * 4 + 0, i * 4 + 1, i * 4 + 2);
            vh.AddTriangle(i * 4 + 2, i * 4 + 3, i * 4 + 0);
        }

        vert.position = new Vector2(0f, 0f);
        vert.color = color;
        vh.AddVert(vert);

        vert.position = new Vector2(0f, 20f);
        vert.color = color;
        vh.AddVert(vert);

        vert.position = new Vector2(20f, 0f);
        vert.color = color;
        vh.AddVert(vert);
    }
}