Shader "Custom/CinemaStereoProjection"
{
    Properties
    {
        _MainTex ("360 Video Texture", 2D) = "white" {}
        _FOV ("Field of View", Range(10, 150)) = 60
        _Opacity ("Opacity", Range(0, 1)) = 0.5
        _RotationX ("Horizontal Rotation", Range(-180, 180)) = 0
        _RotationY ("Vertical Rotation", Range(-90, 90)) = 0
        _Aspect ("Aspect Ratio", Float) = 1.77
        [KeywordEnum(None, SideBySide, OverUnder)] _Layout("Stereo Layout", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _LAYOUT_NONE _LAYOUT_SIDEBYSIDE _LAYOUT_OVERUNDER
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float _FOV, _RotationX, _RotationY, _Aspect;

            float _Opacity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float f = 1.0 / tan(radians(_FOV) * 0.5);
                float3 ray = normalize(float3(i.uv.x * _Aspect, i.uv.y, f));

                // Rotations
                float radX = radians(_RotationX);
                float radY = radians(_RotationY);
                float3x3 rot = mul(
                    float3x3(1, 0, 0, 0, cos(radY), -sin(radY), 0, sin(radY), cos(radY)),
                    float3x3(cos(radX), 0, sin(radX), 0, 1, 0, -sin(radX), 0, cos(radX))
                );
                ray = mul(rot, ray);

                // Spherical Mapping
                float phi = atan2(ray.x, ray.z);
                float theta = asin(ray.y);
                float2 uv360 = float2(phi / (2 * 3.14159) + 0.5, theta / 3.14159 + 0.5);

                // Stereo Remapping
                #if _LAYOUT_SIDEBYSIDE
                    // Use left eye (left half of texture)
                    uv360.x = uv360.x * 0.5;
                #elif _LAYOUT_OVERUNDER
                    // Use top eye (top half of texture)
                    uv360.y = uv360.y * 0.5 + 0.5;
                #endif

                // 1. Get the color from the texture first
                fixed4 col = tex2D(_MainTex, uv360);
                
                // 2. Modify the alpha of that color
                col.a = _Opacity; 

                // 3. Return the final modified color
                return col;
            }
            ENDCG
        }
    }
}