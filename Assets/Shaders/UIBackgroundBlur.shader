Shader "Board Game/UI Background Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _BlurSize ("Blur Size", Range(0, 8)) = 2.5
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _BlurSize;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 color = tex2D(_MainTex, IN.texcoord) * 0.20;
                color += tex2D(_MainTex, IN.texcoord + float2(offset.x, 0)) * 0.10;
                color += tex2D(_MainTex, IN.texcoord - float2(offset.x, 0)) * 0.10;
                color += tex2D(_MainTex, IN.texcoord + float2(0, offset.y)) * 0.10;
                color += tex2D(_MainTex, IN.texcoord - float2(0, offset.y)) * 0.10;
                color += tex2D(_MainTex, IN.texcoord + offset) * 0.10;
                color += tex2D(_MainTex, IN.texcoord - offset) * 0.10;
                color += tex2D(_MainTex, IN.texcoord + float2(offset.x, -offset.y)) * 0.10;
                color += tex2D(_MainTex, IN.texcoord + float2(-offset.x, offset.y)) * 0.10;

                color *= IN.color;
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                return color;
            }
            ENDCG
        }
    }
}
