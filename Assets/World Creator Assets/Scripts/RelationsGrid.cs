using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RelationsGrid : MaskableGraphic, IPointerMoveHandler, IPointerClickHandler, ILayoutElement, IPointerExitHandler
{
    public GameObject factionNamePrefabX;
    public GameObject factionNamePrefabY;
    public Transform xRoot;
    public Transform yRoot;

    int[] relations;
    int[] _factionIDs;
    int _existingFactionCount = 0;
    float _preferredWidth, _preferredHeight;
    int mouseX, mouseY;

    public float minWidth => _preferredWidth;

    public float preferredWidth => _preferredWidth;

    public float flexibleWidth => -1;

    public float minHeight => _preferredHeight;

    public float preferredHeight => _preferredHeight;

    public float flexibleHeight => -1;

    public int layoutPriority => -1;

    const float CellSize = 40f;

    readonly Color bgColor = new Color(0.1f, 0.2f, 0.05f);
    readonly Color lineColor = new Color(0.2f, 0.3f, 0.1f);
    readonly Color highlightColor = new Color(0.4f, 0.5f, 0f);
    readonly Color alliedColor = new Color(0f, 0.7f, 0f);
    readonly Color enemyColor = new Color(0.3f, 0f, 0f);
    readonly Color alliedHoverColor = new Color(0f, 1f, 0f);
    readonly Color enemyHoverColor = new Color(0.6f, 0f, 0f);

    protected override void OnEnable()
    {
        base.OnEnable();
        _existingFactionCount = 0;

        mouseX = -1;
        mouseY = -1;

        if (!FactionManager.Exists)
        {
            relations = new int[1] { 0 };
            _factionIDs = new int[1] { 0 };
            _existingFactionCount = 1;
            return;
        }

        // Count existing factions & get their IDs
        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            if (FactionManager.FactionExists(i))
                _existingFactionCount++;
        }

        _factionIDs = new int[_existingFactionCount];
        int index = 0;
        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            if (FactionManager.FactionExists(i))
                _factionIDs[index++] = i;
        }

        // Clear existing labels
        foreach (Transform child in xRoot)
            Destroy(child.gameObject);
        foreach (Transform child in yRoot)
            Destroy(child.gameObject);

        // Copy faction relations
        relations = new int[_existingFactionCount];
        for (int i = 0; i < _existingFactionCount; i++)
        {
            relations[i] = FactionManager.GetFactionRelations(_factionIDs[i]);
            string factionName = FactionManager.GetFactionName(_factionIDs[i]);

            // Create X-axis label
            var xLabel = Instantiate(factionNamePrefabX, xRoot);
            xLabel.GetComponentInChildren<Text>().text = factionName;
            // Create Y-axis label
            var yLabel = Instantiate(factionNamePrefabY, yRoot);
            yLabel.GetComponentInChildren<Text>().text = factionName;
        }

        var parentRect = rectTransform.parent.GetComponent<RectTransform>();
        const float labelPadding = 210f;
        CalculateLayoutInputHorizontal();
        CalculateLayoutInputVertical();
        parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _preferredWidth + labelPadding);
        parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _preferredHeight + labelPadding);

        Debug.Log("RelationsGrid initialized with " + _existingFactionCount + " factions.");
    }

    public void ApplyChanges()
    {
        for (int i = 0; i < _existingFactionCount; i++)
        {
            int id = _factionIDs[i];
            FactionManager.SetFactionRelations(id, relations[i]);
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // Draw background
        int index = vh.currentVertCount;
        Rect rect = rectTransform.rect;
        vh.AddVert(new Vector3(rect.xMin, rect.yMin), bgColor, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax, rect.yMin), bgColor, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax, rect.yMax), bgColor, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin, rect.yMax), bgColor, Vector2.zero);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);

        Vector3 origin = new Vector3(rect.xMin, rect.yMin, 1f);

        for (int i = 0; i < _existingFactionCount; i++)
        {
            for (int j = 0; j < _existingFactionCount; j++)
            {
                int idy = _factionIDs[j];
                var color = ((relations[i] & (1 << idy)) != 0) ? alliedColor : enemyColor;
                var hoverColor = ((relations[i] & (1 << idy)) != 0) ? alliedHoverColor : enemyHoverColor;
                if (i == mouseX || j == mouseY)
                    color = hoverColor;
                var tileColor = (i == mouseX || j == mouseY) ? highlightColor : lineColor;

                // Draw tile
                index = vh.currentVertCount;
                vh.AddVert(origin + new Vector3(i * CellSize, j * CellSize), tileColor, Vector2.zero);
                vh.AddVert(origin + new Vector3((i + 1) * CellSize, j * CellSize), tileColor, Vector2.zero);
                vh.AddVert(origin + new Vector3((i + 1) * CellSize, (j + 1) * CellSize), tileColor, Vector2.zero);
                vh.AddVert(origin + new Vector3(i * CellSize, (j + 1) * CellSize), tileColor, Vector2.zero);
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index, index + 2, index + 3);

                // Draw relation toggle
                index = vh.currentVertCount;
                float offsetAmount = CellSize * 0.2f;
                vh.AddVert(origin + new Vector3(i * CellSize + offsetAmount, j * CellSize + offsetAmount), color, Vector2.zero);
                vh.AddVert(origin + new Vector3((i + 1) * CellSize - offsetAmount, j * CellSize + offsetAmount), color, Vector2.zero);
                vh.AddVert(origin + new Vector3((i + 1) * CellSize - offsetAmount, (j + 1) * CellSize - offsetAmount), color, Vector2.zero);
                vh.AddVert(origin + new Vector3(i * CellSize + offsetAmount, (j + 1) * CellSize - offsetAmount), color, Vector2.zero);
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index, index + 2, index + 3);
            }
        }
    }

    public void CalculateLayoutInputHorizontal()
    {
        _preferredWidth = _existingFactionCount * CellSize;
    }

    public void CalculateLayoutInputVertical()
    {
        _preferredHeight = _existingFactionCount * CellSize;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos);
        mouseX = (int)(localPos.x / CellSize);
        mouseY = (int)(localPos.y / CellSize);
        UpdateGeometry();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos);
        int x = (int)(localPos.x / CellSize);
        int y = (int)(localPos.y / CellSize);
        if (x < _existingFactionCount && y < _existingFactionCount && x >= 0 && y >= 0)
        {
            int yid = _factionIDs[y];
            relations[x] ^= (1 << yid);
            UpdateGeometry();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseX = mouseY = -1;
        UpdateGeometry();
    }
}
