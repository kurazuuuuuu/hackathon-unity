Shader "UI/PulsingGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _GlowColor ("Glow Color", Color) = (1,1,1,0.5)
        _Speed ("Pulse Speed", Range(0.1, 10.0)) = 3.0
        _Spread ("Pulse Spread", Range(0, 1)) = 0.3
        
        _BorderWidth ("Border Width", Range(0, 0.5)) = 0.1
        _BorderColor ("Border Color", Color) = (1,1,1,1)
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            fixed4 _GlowColor;
            float _Speed;
            float _Spread;
            
            // Border properties
            float _BorderWidth;
            fixed4 _BorderColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                
                // --- NEON GLOW LOGIC ---
                // Pulse: 0.8 to 1.2
                float pulse = 1.0 + 0.3 * sin(_Time.y * _Speed);
                
                // 1. Calculate distance from center to edge (0 at center, 1 at edge)
                float2 uvCentered = abs(IN.texcoord - 0.5) * 2.0;
                float dist = max(uvCentered.x, uvCentered.y);
                
                // 2. Core Intensity (glows strongest at the edge)
                float edgeDist = 1.0 - dist; // 0 at edge, 1 at center
                float glowMask = exp(-edgeDist * (1.0 / max(_BorderWidth, 0.001)) * 3.0); 
                
                // 3. Compose Neon
                half3 neonColor = _BorderColor.rgb * glowMask * pulse * 3.0; // High HDR boost
                
                // ALWAYS add neon to final color (not multiplied by color.a)
                // This ensures the glow is visible even on solid color images
                color.rgb += neonColor;
                
                // Clamp to prevent overflow (optional, for visual consistency)
                color.rgb = min(color.rgb, half3(2.0, 2.0, 2.0));
                
                // DEBUG: Tint entire card with BorderColor to verify shader works
                // Remove this line after confirming shader is applied
                color.rgb = lerp(color.rgb, _BorderColor.rgb, 0.3);

                return color;
            }
            ENDCG
        }
    }
}
