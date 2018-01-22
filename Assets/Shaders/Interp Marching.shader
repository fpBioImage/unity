// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Ray Marching/Ray Marching Interp" 
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

	int _interp;
	int _noise;       

	// Currenty using 8 texture atlases, max size 4096x4096. 
	sampler2D _VolumeTex;
	sampler2D _VolumeTex2;
	sampler2D _VolumeTex3;
	sampler2D _VolumeTex4;
	sampler2D _VolumeTex5;
	sampler2D _VolumeTex6;
	sampler2D _VolumeTex7;
	sampler2D _VolumeTex8;

	sampler2D _noiseTexture;

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

	float _ImageAlpha;
	
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
	
	//#define TOTAL_STEPS 128.0
	//#define STEP_CNT 128
	//#define STEP_SIZE 1 / 128.0

	//float _zSteps = 128;

	#define ZSTEPS_MAX 512
	float _zSteps = 128;

	half frand(float xx){

        half x0=floor(xx);
        half x1=x0+1;
        half v0 = frac(sin (x0*.014686)*31718.927+x0);
        half v1 = frac(sin (x1*.014686)*31718.927+x1);          

        return (v0*(1-frac(xx))+v1*(frac(xx)))*2-1*sin(xx);
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

	float raydist(float3 pos){
		return distance(pos, float3(0,0,0)) - 1.5f;
	}

	half4 raymarch(v2f i, float offset) 
	{
		float STEP_SIZE = 1/_zSteps;
		float3 frontPos = tex2D(_FrontTex, i.uv[1]).xyz;	// Should check if ray intersects cube, otherwise return 0. 	
		float3 backPos = tex2D(_BackTex, i.uv[1]).xyz;				
		float3 dir = backPos - frontPos;
		//float3 pos = frontPos;
		float4 painter = 0;
		float3 stepDist = dir * STEP_SIZE;

		// Apply noise for smoothing/anti-alisasing... Could be improved...

		//if (_noise == 1){
		//	frontPos += STEP_SIZE/16 * float3(frand(i.pos.x*i.pos.y),frand(i.pos.y*i.pos.z), frand(i.pos.z));
		//} else if (_noise == 2) {
		//	frontPos += STEP_SIZE/32 * float3(frand(sin(_Time.w*100)*i.pos.x),frand(sin(_Time.w*100)*2.156*i.pos.y), frand(sin(_Time.w*100)*1.0158*i.pos.z));
		//}
		//frontPos += STEP_SIZE/16 * float3(frand(_noise*i.pos.x*i.pos.y),frand(_noise*i.pos.y*i.pos.z), frand(_noise*i.pos.z));


		for(int k = 0; k < ZSTEPS_MAX; k++) // webGL loops must have predefined number of iterations
		{

			if(k<_zSteps){ // Do nothing if quality we're on a lower quality and have used our steps
				// Add a small amount of noies to the step distance? Doesn't look great...  
				//pos = frontPos + (k + (frand(_Time.x * i.uv[1])-0.5f))  * stepDist;

				float3 pos = frontPos + k * stepDist;

				// Ray March
				float4 src = sample2D(pos);

				if (_ImageAlpha<1.0f){
					// Use alpha as mean of rgb colours
					float meanColor = (src.r + src.g + src.b) / 3.0f;
					if (meanColor < _Threshold){
						src.a = 0.0f;
					} else {
						src.a = meanColor;
					}
				} else {
					// Using alpha from image. Threshold based on alpha value. 
					if (src.a < _Threshold){
						src.a = 0.0f;
					}
				}
								
				// Clipping:
				float border = step(-_ClipPlane.w, dot(_ClipPlane, float4(pos-0.5, 0)));
				border *= step(-_ClipPlane2.w, dot(_ClipPlane2, float4(pos-0.5, 0)));

		        // Standard blending	        
		        src.a *= saturate(_Opacity * border * 128.0f/_zSteps); // Divide by _zSteps to transparency looks equal for all qualities
		        src.rgb *= src.a * _Intensity; // Multiplying by alpha gives smoothing of some kind...

		        painter = (1.0f - painter.a) * src + painter;
	        }
		}

    	return painter;
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