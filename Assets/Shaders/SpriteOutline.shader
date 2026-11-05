Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1, 0.85, 0.2, 1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 2.0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);

                // Nếu pixel có màu gốc
                if (c.a > 0.1)
                {
                    c.rgb *= c.a;
                    return c * IN.color;
                }

                // Kiểm tra viền 8 hướng
                float2 offsets[8] = {
                    float2( 1,  0), float2(-1,  0),
                    float2( 0,  1), float2( 0, -1),
                    float2( 0.707,  0.707), float2(-0.707,  0.707),
                    float2( 0.707, -0.707), float2(-0.707, -0.707)
                };

                float maxAlpha = 0.0;
                for (int i = 0; i < 8; i++)
                {
                    float2 sampleCoord = IN.texcoord + offsets[i] * _MainTex_TexelSize.xy * _OutlineSize;
                    maxAlpha = max(maxAlpha, tex2D(_MainTex, sampleCoord).a);
                }

                if (maxAlpha > 0.2)
                {
                    fixed4 outColor = _OutlineColor;
                    outColor.rgb *= outColor.a * maxAlpha;
                    return outColor;
                }

                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
