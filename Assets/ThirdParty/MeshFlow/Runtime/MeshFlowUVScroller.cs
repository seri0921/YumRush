using UnityEngine;

namespace MeshFlow
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public sealed class MeshFlowUVScroller : MonoBehaviour
    {
        public Vector2 speed = new Vector2(0f, 0.25f);
        public bool pingPong;
        [Min(0.01f)] public float duration = 8f;
        [Min(0f)] public float playbackRate = 1f;
        public bool useUnscaledTime;
        [Header("Flipbook")]
        public bool flipbook;
        [Min(1)] public int columns = 4;
        [Min(1)] public int rows = 4;
        [Min(0f)] public float flipbookFps = 12f;
        [Tooltip("MeshFlow: _MainTex; URP: _BaseMap")]
        public string textureProperty = "_MainTex";
        public bool playOnEnable = true;

        Renderer targetRenderer;
        MaterialPropertyBlock block;
        Vector2 baseScale = Vector2.one, baseOffset;
        int propertyId;
        double elapsed;

        void OnEnable()
        {
            targetRenderer = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            propertyId = Shader.PropertyToID(textureProperty + "_ST");
            var material = targetRenderer.sharedMaterial;
            if (material != null && material.HasProperty(textureProperty))
            {
                baseScale = material.GetTextureScale(textureProperty);
                baseOffset = material.GetTextureOffset(textureProperty);
            }
            elapsed = 0;
            Apply();
        }

        void Update()
        {
            if (!playOnEnable) return;
            elapsed += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * playbackRate;
            Apply();
        }

        public void Seek(float seconds) { elapsed = System.Math.Max(0, seconds); Apply(); }

        void Apply()
        {
            if (targetRenderer == null || block == null) return;
            double t = elapsed;
            if (pingPong)
            {
                double period = System.Math.Max(0.01, duration);
                t = period * 0.25 * (1.0 - System.Math.Cos(2.0 * System.Math.PI * elapsed / period));
            }
            Vector2 scale = baseScale, offset = baseOffset;
            if (flipbook)
            {
                int cols = Mathf.Max(1, columns), rowCount = Mathf.Max(1, rows), frames = cols * rowCount;
                int index = flipbookFps > 0f ? (int)(System.Math.Floor(elapsed * flipbookFps) % frames) : 0;
                if (index < 0) index += frames;
                scale = new Vector2(1f / cols, 1f / rowCount);
                offset = new Vector2((index % cols) / (float)cols, 1f - ((index / cols) + 1f) / rowCount);
            }
            targetRenderer.GetPropertyBlock(block);
            block.SetVector(propertyId, new Vector4(scale.x, scale.y,
                offset.x + (float)(speed.x * t), offset.y + (float)(speed.y * t)));
            targetRenderer.SetPropertyBlock(block);
        }

        void OnDisable()
        {
            if (targetRenderer == null || block == null) return;
            targetRenderer.GetPropertyBlock(block);
            block.SetVector(propertyId, new Vector4(baseScale.x, baseScale.y, baseOffset.x, baseOffset.y));
            targetRenderer.SetPropertyBlock(block);
        }
    }
}
