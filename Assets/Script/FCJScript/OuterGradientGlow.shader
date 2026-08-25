Shader "UI/Outer Gradient Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowAlpha ("Glow Alpha", Range(0, 1)) = 0.15
        _InnerBounds ("Inner Bounds", Vector) = (0.1, 0.1, 0.9, 0.9)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _GlowColor;
            float _GlowAlpha;
            float4 _InnerBounds;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 nearestUv = clamp(uv, _InnerBounds.xy, _InnerBounds.zw);
                float2 outsideDistance = abs(uv - nearestUv);
                float distanceFromImage = max(outsideDistance.x, outsideDistance.y);
                float glowRadius = max(_InnerBounds.x, _InnerBounds.y);

                if (distanceFromImage <= 0.0001)
                {
                    discard;
                }

                float2 sourceUv = (nearestUv - _InnerBounds.xy) / (_InnerBounds.zw - _InnerBounds.xy);
                float sourceAlpha = tex2D(_MainTex, sourceUv).a;
                float fade = 1.0 - smoothstep(0.0, glowRadius, distanceFromImage);
                fixed4 outputColor = _GlowColor * input.color;
                outputColor.a *= sourceAlpha * fade * _GlowAlpha;
                return outputColor;
            }
            ENDCG
        }
    }
}