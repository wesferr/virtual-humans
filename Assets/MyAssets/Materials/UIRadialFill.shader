Shader "Custom/UIRadialFillRound"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)
        _Fill    ("Fill Amount (0..1)", Range(0,1)) = 1
        _RectAspect ("Rect Aspect (W/H)", Float) = 1.0
        _StartAngle ("Start Angle (deg)", Range(0,360)) = 0 // 0=3h, 90=6h, 180=9h, 270=12h
        _Clockwise  ("Clockwise (0/1)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Lighting Off ZWrite Off Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0

            // Instancing + variantes de XR (Unity liga STEREO_INSTANCING_ON no Editor/Player conforme o modo XR)
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Fill;
            float _RectAspect; // width/height
            float _StartAngle; // degrees
            float _Clockwise;  // 0 or 1

            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            // (sem propriedades instanciadas específicas)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            // -pi..pi -> 0..1
            inline float Angle01(float a)
            {
                const float PI = 3.14159265;
                return frac((a + PI) / (2.0 * PI));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // UV centralizada
                float2 p = i.uv - 0.5;

                // Corrige aspecto para manter círculo
                p.x /= max(_RectAspect, 1e-6);

                // Raio normalizado (0.5 = borda no UV 0..1)
                float r = length(p) / 0.5;

                if (r > 1.0)
                    discard;

                // Ângulo no ponto
                float a = atan2(p.y, p.x);   // -pi..pi
                float ang01 = Angle01(a);    // 0..1

                // Offset do início
                float start01 = frac(_StartAngle / 360.0);
                ang01 = frac(ang01 - start01 + 1.0);

                // Preenchimento
                float fill = saturate(_Fill);
                if (_Clockwise >= 0.5)
                {
                    if (ang01 > fill) discard;
                }
                else
                {
                    if ((1.0 - ang01) > fill) discard;
                }

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
