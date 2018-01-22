// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Ray Marching/Ray Marching" 
{
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	//#pragma target 3.0
	#pragma profileoption MaxLocalParams=1024 
	#pragma profileoption NumInstructionSlots=4096
	#pragma profileoption NumMathInstructionSlots=4096
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
	
	sampler2D _VolumeTex;
	sampler2D _VolumeTex2;
	sampler2D _VolumeTex3;
	sampler2D _VolumeTex4;

	float4 _VolumeTex_TexelSize;

	float _numSlices;
	float _maxX;
	float _texturesPerSlice;
	float _tPixelWidth;
	float _tPixelHeight;
	float _packedWidth;
	float _packedHeight;
	float _zScale;

	sampler2D _FrontTex;
	sampler2D _BackTex;
	
	float4 _LightDir;
	float4 _LightPos;
	
	float _Dimensions;
	
	float _Opacity;
	float _Threshold;
	float _Intensity;

	float4 _ClipPlane;
	float4 _ClipPlane2;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		
		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy;
		#if SHADER_API_D3D9
		if (_MainTex_TexelSize.y < 0)
			o.uv[0].y = 1-o.uv[0].y;
		#endif			
		return o;
	}
	
	#define TOTAL_STEPS 128.0
	#define STEP_CNT 128
	#define STEP_SIZE 1 / 127.0
	
	half4 raymarch(v2f i, float offset) 
	{
		float3 frontPos = tex2D(_FrontTex, i.uv[1]).xyz;		
		float3 backPos = tex2D(_BackTex, i.uv[1]).xyz;				
		float3 dir = backPos - frontPos;
		float3 pos = frontPos;
		float4 dst = 0;
		float3 stepDist = dir * STEP_SIZE;
					
		for(int k = 0; k < STEP_CNT; k++)
		{
			pos = frontPos + k * stepDist;

			//float roundZ = 0.51f; //floor(pos.z * 127.0f)/127.0f;

			//float i = floor(roundZ * (_numSlices)); 
			float i = round(pos.z * (_numSlices - 1));


			float slice = fmod(i, 4.0f);
			float z = floor(i/4.0f);

			float xStart = fmod(z, _maxX) * (_tPixelWidth); 
			float yStart = floor(z / _maxX) * (_tPixelHeight);

			float x = xStart + pos.x * (_tPixelWidth-1); // does flooring this cause the voxellation?
			float y = yStart + pos.y * (_tPixelHeight-1); // this too.

			float u = x/_packedWidth;
			float v = y/_packedHeight;

			float4 src;
			if (slice == 0){
				src = tex2D(_VolumeTex, float2(u,v));
			} else if (slice == 1) {
				src = tex2D(_VolumeTex2, float2(u,v));
			} else if (slice == 2) {
				src = tex2D(_VolumeTex3, float2(u,v));
			} else if (slice == 3) {
				src = tex2D(_VolumeTex4, float2(u,v));
			}

			// Threshold:
			float meanColor = (src.r + src.g + src.b) / 3.0f;
			if (meanColor < _Threshold){
				src.a = 0.0f;
			}

			// Smoothing:
			src.a *= meanColor; // hang on what's this? Was this in rayMarching.cs before? This is supposed to 'increase visibility'
			
			// Clipping:
			float border = step(-_ClipPlane.w, dot(_ClipPlane, float4(pos-0.5, 0)));
			border *= step(-_ClipPlane2.w, dot(_ClipPlane2, float4(pos-0.5, 0)));


	        // Standard blending	        
	        src.a *= saturate(_Opacity * border);
	        src.rgb *= src.a * _Intensity;
	        dst = (1.0f - dst.a) * src + dst;
		}

    	return dst + dst;
	}

	ENDCG
	
Subshader {
	ZTest Always Cull Off ZWrite Off
	Fog { Mode off }
		
	Pass 
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		half4 frag(v2f i) : COLOR { return raymarch(i, 0); }	
		ENDCG
	}					
}

Fallback off
	
} // shader