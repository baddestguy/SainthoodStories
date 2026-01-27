Shader "UI/CRTBarrel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)
        _Strength ("Curvature Strength", Range(-1,1)) = 0.15
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            float _Strength;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Center around (0.5, 0.5)
                float2 centered = uv - 0.5;

                // Radius^2 for distortion strength
                float r2 = dot(centered, centered);

                // Apply barrel distortion
                float2 distorted = uv + centered * r2 * _Strength;

                // If outside [0..1], output transparent pixel
                if (distorted.x < 0.0 || distorted.x > 1.0 || distorted.y < 0.0 || distorted.y > 1.0)
                {
                    return float4(0,0,0,0);
                }

                fixed4 col = tex2D(_MainTex, distorted) * i.color;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "UI/Default"
}
