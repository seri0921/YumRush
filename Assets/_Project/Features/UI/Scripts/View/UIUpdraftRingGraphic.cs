using UnityEngine;

public sealed class UIUpdraftRingGraphic : UnityEngine.UI.MaskableGraphic
{
    [SerializeField, Min(1f)] private float _thickness = 3f;

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float outer = Mathf.Min(rect.width, rect.height) * 0.5f;
        float inner = Mathf.Max(0f, outer - _thickness);
        const int segments = 96;
        for (int index = 0; index <= segments; index++)
        {
            float angle = index * Mathf.PI * 2f / segments;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            mesh.AddVert(rect.center + direction * outer, color, Vector2.zero);
            mesh.AddVert(rect.center + direction * inner, color, Vector2.zero);
            if (index == segments)
                continue;
            int vertexIndex = index * 2;
            mesh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 1);
            mesh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);
        }
    }
}
