
// shader from: https://www.digital-dust.com/single-post/2017/03/16/GPU-Voronoi-noise-in-Unity
Shader "Noise/VoronoiNoise4D" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color", color) = (1,1,1,1)
		_Frequency("Frequency", float) = 10.0
		_Lacunarity("Lacunarity", float) = 2.0
		_Gain("Gain", float) = 0.5
		_Jitter("Jitter", Range(0,1)) = 1.0
	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend One One
		ZWrite Off
		
		CGPROGRAM
		#pragma surface surf NoLighting vertex:vert alpha
		#pragma target 3.0
		#include "GPUVoronoiNoise4D.cginc"
		#define OCTAVES 1

		sampler2D _MainTex;
		fixed4 _Color;
		
		struct Input 
		{
			float2 uv_MainTex;
			float4 noiseUV;
		};
		
		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.noiseUV = float4(v.vertex.xyz, _Time.x); //use model space, not world space for noise uvs
		}

		void surf(Input IN, inout SurfaceOutput o) 
		{
		
			float n = fBm_F0(IN.noiseUV, OCTAVES);
	
			o.Albedo = _Color * n;
			o.Alpha = n * _Color.a;
		}
		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
