Shader "Player/Impact Flash Outline"
{
    Properties
    {
        [MainTexture] _MainTex ("Particle Texture", 2D) = "white" {}
        [MainColor] _Color ("Body Color", Color) = (1, 0.7, 0.05, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width (Texture Pixels)", Range(0, 4)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "ImpactFlashOutline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half4 _Color;
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half SampleAlpha(float2 uv)
            {
                // Texture edges must stay transparent even when the sampler clamps.
                float2 inside = step(0.0, uv) * step(uv, 1.0);
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a * inside.x * inside.y;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 textureColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float2 offset = _MainTex_TexelSize.xy * _OutlineWidth;
                float2 diagonal = offset * 0.70710678;
                half expandedAlpha = textureColor.a;
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv + float2(offset.x, 0)));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv - float2(offset.x, 0)));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv + float2(0, offset.y)));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv - float2(0, offset.y)));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv + diagonal));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv - diagonal));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv + float2(diagonal.x, -diagonal.y)));
                expandedAlpha = max(expandedAlpha, SampleAlpha(input.uv + float2(-diagonal.x, diagonal.y)));

                half bodyAlpha = textureColor.a * _Color.a;
                half outlineAlpha = (expandedAlpha - textureColor.a) * _OutlineColor.a;
                half3 bodyColor = textureColor.rgb * _Color.rgb * input.color.rgb;
                // Keep the outline color independent of particle RGB; particle alpha fades both.
                half3 premultipliedColor = bodyColor * bodyAlpha + _OutlineColor.rgb * outlineAlpha;
                return half4(premultipliedColor, bodyAlpha + outlineAlpha) * input.color.a;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
