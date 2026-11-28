// World-space text that respects depth. Unity's default TextMesh material
// ignores the depth buffer, so every label drew through walls and from any
// distance (the "quiz text is transparent and visible from anywhere" bug).
// This shader depth-tests (ZTest LEqual) and fades with _Fade, which
// WorldTextFader drives from the player's distance and viewing angle.
// It lives under Resources so builds always include it.
Shader "LightShadow/WorldText"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (1,1,1,1)
        _Fade ("Fade", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Lighting Off
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Fade;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = i.color;
                // Font atlases are alpha only: the glyph shape is the alpha.
                col.a *= tex2D(_MainTex, i.uv).a * _Fade;
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
