#ifndef PROJECT_SPRITE_OUTLINE_SAMPLING_INCLUDED
#define PROJECT_SPRITE_OUTLINE_SAMPLING_INCLUDED

float4 _MainTex_TexelSize;
float4 _SpriteUVRect;

half OutlineSampleAlpha(float2 uv)
{
    float2 inset = _MainTex_TexelSize.xy * 0.5;
    float2 sampleUV = clamp(uv, _SpriteUVRect.xy + inset, _SpriteUVRect.zw - inset);
    half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV).a;
    float2 inside = step(_SpriteUVRect.xy, uv) * step(uv, _SpriteUVRect.zw);
    // Tiled terrain continues at slice edges: do not draw a line at every join.
    return alpha * lerp(1.0, inside.x * inside.y, _OutlineAtRectEdge);
}

half4 OutlineSpriteColor(float2 uv, half4 tint)
{
    half4 body = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * tint;
    float2 offset = _MainTex_TexelSize.xy * _OutlineWidth;
    float2 diagonal = offset * 0.70710678;
    half neighborAlpha = OutlineSampleAlpha(uv + float2(offset.x, 0));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv - float2(offset.x, 0)));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv + float2(0, offset.y)));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv - float2(0, offset.y)));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv + diagonal));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv - diagonal));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv + float2(diagonal.x, -diagonal.y)));
    neighborAlpha = min(neighborAlpha, OutlineSampleAlpha(uv + float2(-diagonal.x, diagonal.y)));
    // Drawing inward preserves Tight meshes, sprite bounds, and physics outlines.
    half edge = saturate(1.0 - neighborAlpha) * step(0.001, _OutlineWidth);
    body.rgb = lerp(body.rgb, _OutlineColor.rgb * tint.rgb, edge * _OutlineColor.a);
    return body;
}

#endif
