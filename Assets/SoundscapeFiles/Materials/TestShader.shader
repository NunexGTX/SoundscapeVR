Shader "Custom/EAC_Skybox"
{
    Properties
    {
        _MainTex ("Video Texture (Render Texture)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // The vertex position in object space is our direction vector
                o.viewDir = v.vertex.xyz;
                return o;
            }

            // Using float4 instead of fixed4 for Vulkan compatibility
            float4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.viewDir);
                float3 absDir = abs(dir);
                float2 uv;
                int faceIndex;

                // 1. Logic to determine face index and local UVs
                if (absDir.x > absDir.y && absDir.x > absDir.z) {
                    faceIndex = dir.x > 0 ? 0 : 1;
                    uv = float2(-dir.z, dir.y) / absDir.x;
                } 
                else if (absDir.y > absDir.z) {
                    faceIndex = dir.y > 0 ? 2 : 3;
                    // Apply the 90-degree rotation for the "sideways" top/bottom
                    uv = float2(dir.x, dir.z) / absDir.y;
                    float temp = uv.x;
                    uv.x = -uv.y;
                    uv.y = temp;
                } 
                else {
                    faceIndex = dir.z > 0 ? 4 : 5;
                    uv = float2(dir.x, dir.y) / absDir.z;
                }

                // 2. Map from [-1, 1] to [0, 1]
                uv = uv * 0.5 + 0.5;

                // 3. Tile the UVs for a 3x2 grid (adjust if your grid is different)
                // Assuming Row 0: Faces 0,1,2 | Row 1: Faces 3,4,5
                float2 tileOffset = float2(faceIndex % 3, faceIndex / 3);
                uv = (uv + tileOffset) / float2(3, 2);

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}