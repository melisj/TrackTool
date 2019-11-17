Shader "Custom/CombineTextures"
{
    Properties
    {
        _Color1 ("Color", Color) = (1,1,1,1)
		_Tex1 ("Albedo 1", 2D) = "white" {}
		_NormalTex1 ("Normal Map 1", 2D) = "white" {}
		_NormalScale1 ("Normal Map Scale 1", Float) = 1
		_Glossiness1 ("Smoothness 1", Range(0,1)) = 0.5
        _Metallic1 ("Metallic 1", Range(0,1)) = 0.0

		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Tex2 ("Albedo 2", 2D) = "white" {}
		_NormalTex2 ("Normal Map 2", 2D) = "white" {}
		_NormalScale2 ("Normal Map Scale 2", Float) = 1
		_Glossiness2 ("Smoothness 2", Range(0,1)) = 0.5
        _Metallic2 ("Metallic 2", Range(0,1)) = 0.0

		_NoiseTex ("Noise", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _Tex1;
        sampler2D _Tex2;
        sampler2D _NoiseTex;
        sampler2D _NormalTex1;
        sampler2D _NormalTex2;

        struct Input
        {
            float2 uv_Tex1;
			float2 uv_Tex2;
			float2 uv_NoiseTex;
			float2 uv_NormalTex1;
			float2 uv_NormalTex2;
        };

        half _Glossiness1;
        half _Glossiness2;
        half _Metallic1;
        half _Metallic2;
        half _NormalScale1;
        half _NormalScale2;

        fixed4 _Color1;
        fixed4 _Color2;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			// Sample the textures here
			fixed4 color1 = tex2D (_Tex1, IN.uv_Tex1) * _Color1;
			fixed4 color2 = tex2D (_Tex2, IN.uv_Tex2) * _Color2;
			fixed noise = tex2D (_NoiseTex, IN.uv_NoiseTex);

			// Merge the textures
			o.Albedo = lerp(color1, color2, noise);
            o.Alpha = lerp(color1, color2, noise);

			// Set the normal textures
			fixed3 normal1 = lerp(UnpackNormal(tex2D(_NormalTex1, IN.uv_NormalTex1)), fixed3(0, 0, 1), -_NormalScale1 + 1);
			fixed3 normal2 = lerp(UnpackNormal(tex2D(_NormalTex2, IN.uv_NormalTex2)), fixed3(0, 0, 1), -_NormalScale2 + 1);
			o.Normal = lerp(normal1, normal2, noise);

			// Set other values
			o.Metallic = lerp(_Metallic1, _Metallic2, noise);
            o.Smoothness = lerp(_Glossiness1, _Glossiness2, noise);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
