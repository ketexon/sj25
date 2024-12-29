Shader "Ketexon/Outline"
{
    Properties
    {
        _DepthBlend ("Depth Blend", Range(0, 1)) = 0.9
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _DownSample ("Downsample", int) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment OutlineFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            uniform float _DepthBlend;
            uniform float _Threshold;
            uniform int _DownSample;
            uniform float4 _OutlineColor;

            float Sobel(texture2D tex, float2 texelSize, float2 uv){
                float4 tl = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(-texelSize.x, texelSize.y));
                float4 tc = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(0, texelSize.y));
                float4 tr = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(texelSize.x, texelSize.y));
                float4 cl = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(-texelSize.x, 0));
                float4 cr = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(texelSize.x, 0));
                float4 bl = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(-texelSize.x, -texelSize.y));
                float4 bc = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(0, -texelSize.y));
                float4 br = SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, uv + float2(texelSize.x, -texelSize.y));

                float4 gx = tl + 2 * cl + bl - tr - 2 * cr - br;
                float4 gy = tl + 2 * tc + tr - bl - 2 * bc - br;

                return sqrt(gx * gx + gy * gy);
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                float depthEdge = Sobel(
                    _CameraDepthTexture,
                    _CameraDepthTexture_TexelSize.xy * _DownSample,
                    input.texcoord
                );
                float normalsEdge = Sobel(
                    _CameraNormalsTexture,
                    _CameraNormalsTexture_TexelSize.xy * _DownSample,
                    input.texcoord
                );

                float edge = depthEdge * _DepthBlend + normalsEdge * (1 - _DepthBlend);
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                return edge > _Threshold
                    ? half4(_OutlineColor.rgb, 1) + color * (1 - _OutlineColor.a)
                    : color;
            }
            ENDHLSL
        }
    }
}
