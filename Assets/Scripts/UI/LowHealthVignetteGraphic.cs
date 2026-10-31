using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LowHealthVignetteGraphic : MaskableGraphic
{
    [Range(0.03f, 0.45f)] [SerializeField] private float edgeWidth = 0.22f;

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = GetPixelAdjustedRect();
        float inset = Mathf.Min(rect.width, rect.height) * edgeWidth;
        Rect inner = new Rect(
            rect.xMin + inset,
            rect.yMin + inset,
            Mathf.Max(0f, rect.width - inset * 2f),
            Mathf.Max(0f, rect.height - inset * 2f));

        Color32 outerColor = color;
        Color transparent = color;
        transparent.a = 0f;
        Color32 innerColor = transparent;

        AddVertex(vertexHelper, rect.xMin, rect.yMin, outerColor); // 0 outer BL
        AddVertex(vertexHelper, rect.xMin, rect.yMax, outerColor); // 1 outer TL
        AddVertex(vertexHelper, rect.xMax, rect.yMax, outerColor); // 2 outer TR
        AddVertex(vertexHelper, rect.xMax, rect.yMin, outerColor); // 3 outer BR
        AddVertex(vertexHelper, inner.xMin, inner.yMin, innerColor); // 4 inner BL
        AddVertex(vertexHelper, inner.xMin, inner.yMax, innerColor); // 5 inner TL
        AddVertex(vertexHelper, inner.xMax, inner.yMax, innerColor); // 6 inner TR
        AddVertex(vertexHelper, inner.xMax, inner.yMin, innerColor); // 7 inner BR

        AddQuad(vertexHelper, 0, 1, 5, 4);
        AddQuad(vertexHelper, 1, 2, 6, 5);
        AddQuad(vertexHelper, 2, 3, 7, 6);
        AddQuad(vertexHelper, 3, 0, 4, 7);
    }

    private static void AddVertex(VertexHelper helper, float x, float y, Color32 vertexColor)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = new Vector3(x, y);
        vertex.color = vertexColor;
        helper.AddVert(vertex);
    }

    private static void AddQuad(VertexHelper helper, int a, int b, int c, int d)
    {
        helper.AddTriangle(a, b, c);
        helper.AddTriangle(c, d, a);
    }
}
