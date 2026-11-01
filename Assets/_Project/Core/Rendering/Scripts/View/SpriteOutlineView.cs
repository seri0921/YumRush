using UnityEngine;
using UnityEngine.Sprites;

/// <summary>Supplies the current sprite's texture region to the outline material.</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteOutlineView : MonoBehaviour
{
    private static readonly int SpriteUVRect = Shader.PropertyToID("_SpriteUVRect");

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _properties;
    private Sprite _previousSprite;
    private Texture _previousTexture;

    private void OnEnable()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _properties = new MaterialPropertyBlock();
        _previousSprite = null;
        _previousTexture = null;
        UpdateSpriteRegion();
    }

    private void LateUpdate()
    {
        UpdateSpriteRegion();
    }

    private void UpdateSpriteRegion()
    {
        Sprite sprite = _spriteRenderer.sprite;
        // Animation clips may temporarily clear a sprite.
        if (sprite == null || (sprite == _previousSprite && sprite.texture == _previousTexture))
            return;

        _spriteRenderer.GetPropertyBlock(_properties);
        _properties.SetVector(SpriteUVRect, DataUtility.GetOuterUV(sprite));
        _spriteRenderer.SetPropertyBlock(_properties);
        _previousSprite = sprite;
        _previousTexture = sprite.texture;
    }
}
