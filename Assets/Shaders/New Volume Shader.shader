// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Raymarching/New Volume Shader"
{
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#pragma profileoption MaxLocalParams=1024 
	#pragma profileoption NumInstructionSlots=4096
	#pragma profileoption NumMathInstructionSlots=4096

		// Currenty using 8 texture atlases, max size 4096x4096. 
	sampler2D _VolumeTex;
	sampler2D _VolumeTex2;
	sampler2D _VolumeTex3;
	sampler2D _VolumeTex4;
	sampler2D _VolumeTex5;
	sampler2D _VolumeTex6;
	sampler2D _VolumeTex7;
	sampler2D _VolumeTex8;

	float _numSlices;
	float _maxX;
	float _texturesPerSlice;
	float _tPixelWidth;
	float _tPixelHeight;
	float _packedWidth;
	float _packedHeight;
	float _zScale;

	float _interp = 0;

	#define STEPS 128
	#define MIN_DIST 0.01


	struct appdata {
		float4 vertex : POSITION;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		float3 wPos : TEXCOORD1; // World Position
	};

	v2f vert (appdata v) {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		return o;
	}

	float sphereDistance(float3 p, float3 c, float r) {
		return 	distance(p, c) - r;
	}

	float4 V(float X, float Y, float Z)
	{
			float4 pos;
			pos.x = X; pos.y = Y; pos.z = Z;

			float slice = fmod(pos.z, 8.0f);
			float z = floor(pos.z / 8.0f);

			float xStart = fmod(z, _maxX) * _tPixelWidth; 
			float yStart = floor(z / _maxX) * _tPixelHeight;

			float x = xStart + pos.x;
			float y = yStart + pos.y; 

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
			} else if (slice == 4) {
				src = tex2D(_VolumeTex5, float2(u,v));
			} else if (slice == 5) {
				src = tex2D(_VolumeTex6, float2(u,v));
			} else if (slice == 6) {
				src = tex2D(_VolumeTex7, float2(u,v));
			} else if (slice == 7) {
				src = tex2D(_VolumeTex8, float2(u,v));
			}

			return src;
	}

	float4 sample2D(float3 pos)
	{
		// Pos is in 0->1 coordinates. Convert to 0--> XYZ
		float3 POS;
		POS.x = pos.x * (_tPixelWidth-1);
		POS.y = pos.y * (_tPixelHeight-1);
		POS.z = pos.z * (_numSlices-1);

		float4 c;

		if (_interp == 1 || _interp == 2)
		{
			// Set up corner coordinates around POS
			float x0 = floor(POS.x); 
			float x1 = ceil(POS.x);
			float y0 = floor(POS.y);
			float y1 = ceil(POS.y);
			float z0 = floor(POS.z);
			float z1 = ceil(POS.z);

			// Get interpolation fraction
			//float xd = frac(POS.x);
			//float yd = frac(POS.y);
			//float zd = frac(POS.z);
			float xd = POS.x-x0; // Same thing as frac
			float yd = POS.y-y0;
			float zd = POS.z-z0;

			if (_interp == 2)
			{
			// Perform trilinear interpolation, according to
			// https://en.wikipedia.org/wiki/Trilinear_interpolation
				float4 c00 = V(x0,y0,z0)*(1.0f-xd) + V(x1,y0,z0)*xd;
				float4 c01 = V(x0,y0,z1)*(1.0f-xd) + V(x1,y0,z1)*xd;
				float4 c10 = V(x0,y1,z0)*(1.0f-xd) + V(x1,y1,z0)*xd;
				float4 c11 = V(x0,y1,z1)*(1.0f-xd) + V(x1,y1,z1)*xd;

				float4 c0 = c00*(1.0f-yd) + c10*yd;
				float4 c1 = c01*(1.0f-yd) + c11*yd;

				c = c0 * (1-zd) + c1*zd;
			} else {
				float mix = (xd + yd + zd) / 3.0f;
				c = V(x0, y0, z0) * (1.0f-mix) + V(x1, y1, z1) * mix;
			}
		} else {
			c = V(POS.x, POS.y, round(POS.z));
		}
		return c;
	}

	half4 raymarch (v2f i){
		float3 position = i.wPos;
		float3 direction = normalize(i.wPos - _WorldSpaceCameraPos);

		half4 painter = float4(0,0,0,0);

		for(int k=0; k<STEPS; k++)
		{
			float dist = sphereDistance(position, float3(0,0,0), 1);
			if (dist < MIN_DIST)
			{ 
				float4 src = sample2D(position); 
				src.a *= saturate(0.4);
				src.rgb *= src.a * 0.8f;
				painter = (1.0f - painter.a) * src + painter;
			} 
			position += direction * dist;
		}
		return painter;

	}

	ENDCG

	Subshader {
		//ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
			
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			half4 frag(v2f i) : SV_Target { return raymarch(i); }	
			ENDCG
		}					
	}

}
