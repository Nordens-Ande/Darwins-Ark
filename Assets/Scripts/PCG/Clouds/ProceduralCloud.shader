Shader "Custom/ProceduralCloud"
{
    Properties
    {
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 1)

        _NoiseScale ("Noise Scale", Float) = 3
        _Opacity ("Opacity", Range(0,3)) = 1
        _MorphSpeed ("Morph Speed", Float) = 0.05

        _CloudWidth ("Cloud Width", Float) = 1.4
        _CloudHeight ("Cloud Height", Float) = 0.85

        _Seed ("Seed", Float) = 0

        _EdgeSoftness ("Edge Softness", Range(0.01, 0.4)) = 0.10
        _ShapeVariation ("Shape Variation", Range(0,0.5)) = 0.18
        _DetailVariation ("Detail Variation", Range(0,0.3)) = 0.08
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _CloudColor;

            float _NoiseScale;
            float _Opacity;
            float _MorphSpeed;

            float _CloudWidth;
            float _CloudHeight;

            float _Seed;

            float _EdgeSoftness;
            float _ShapeVariation;
            float _DetailVariation;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float hash21(float2 p)
            {
                p = frac(
                    p * float2(123.34, 456.21)
                );

                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                float2 u =
                    f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(a, b, u.x),
                    lerp(c, d, u.x),
                    u.y
                );
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;

                for (int i = 0; i < 4; i++)
                {
                    value += noise(p) * amplitude;

                    p *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }
            
            float RandomShapeRadius(float2 centered, float seed, float time)
            {
                float angle = 
                    atan2(
                        centered.y,
                        centered.x
                    );

                float angle01 =
                    (angle + 3.14159265)
                    / 6.2831853;

                float largeShape =
                    fbm(
                        float2(
                            angle01 * 3.0 + seed, 
                            time + seed * 0.17
                        )
                    );

                float smallShape = 
                    fbm(
                        float2(
                            angle01 * 7.0 + seed * 1.7,
                            time * 0.7 + seed + 5.0
                        )
                    );

                float radius = 
                    0.52
                    + largeShape * _ShapeVariation
                    + smallShape * _DetailVariation;

                return radius;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float2 centered =
                    uv * 2.0 - 1.0;

                centered.x /= _CloudWidth;
                centered.y /= _CloudHeight;

                float time = 
                    _Time.y *
                    _MorphSpeed;

                float distanceFromCenter =
                    length(centered);

                float targetRadius =
                    RandomShapeRadius(
                        centered,
                        _Seed,
                        time
                    );

                float detail =
                    fbm(
                        centered *
                        _NoiseScale +
                        float2(
                        _Seed,
                        time
                        )
                    );

                targetRadius +=
                    (detail - 0.5) * _DetailVariation;

                float alpha =
                    1.0 - 
                    smoothstep(
                        targetRadius -
                        _EdgeSoftness,

                        targetRadius + 
                        _EdgeSoftness,

                        distanceFromCenter
                    );

                alpha *= _Opacity;

                alpha *= _CloudColor.a;

                return half4(
                    _CloudColor.rgb,
                    alpha
                );
            }
            ENDHLSL
        }
    }
}
