// shader that performs ray casting using a 3D texture
// adapted from a Cg example by Nvidia
// http://developer.download.nvidia.com/SDK/10/opengl/samples.html
// Gilles Ferrand, University of Manitoba / RIKEN, 2016–2017
//
// Adapted heavily for use in FPBioimage 2017
// Marcus Fantham, University of Cambridge

Shader "Custom/Volume Ray Caster" {
	
	Properties {
		_DataMin ("Data threshold: min", Range(0,1)) = 0
		_DataMax ("Data threshold: max", Range(0,1)) = 1
		_StretchPower ("Data stretch power", Range(0.1,3)) = 1  // increase it to highlight the highest data values
		_Opacity ("Intensity normalization per step", Range(0, 10)) = 1
		_Intensity  ("Intensity normalization per ray" , Range(0, 10)) = 1
		_Steps ("Max number of steps", Range(1,1024)) = 128 // should ideally be as large as data resolution, strongly affects frame rate

		_Atlas0("Atlas0", 2D) = "black" {}
		_Atlas1("Atlas1", 2D) = "black" {}
		_Atlas2("Atlas2", 2D) = "black" {}
		_Atlas3("Atlas3", 2D) = "black" {}
		_Atlas4("Atlas4", 2D) = "black" {}
		_Atlas5("Atlas5", 2D) = "black" {}
		_Atlas6("Atlas6", 2D) = "black" {}
		_Atlas7("Atlas7", 2D) = "black" {}
	}

	SubShader {
		
		Tags { "Queue" = "Transparent" }

		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZTest Always // Check this...
			ZWrite Off // Off is correct. 
			Fog { Mode off }

			CGPROGRAM
	        #pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#define MAX_STEPS 768

			sampler2D _Atlas0, _Atlas1, _Atlas2, _Atlas3, _Atlas4, _Atlas5, _Atlas6, _Atlas7; 

			float _imageWidth;
			float _imageHeight;
			float _imageDepth;
			float _slicesPerAtlas;
			float _slicesPerRow;
			float _atlasWidth;
			float _atlasHeight;

			float4 _ClipPlane;
			float4 _CubeScale;
			float _DataMin, _DataMax;
			float _StretchPower;
			float _Opacity;
			float _Intensity;
			float _Steps;
			float _Interp;
			int _RenderMode;
			int _RainbowCube;

			// 2D atlas sampling, including interpolation
			float4 V(float X, float Y, float Z){
				float4 pos;
				pos.x = X; pos.y = Y; pos.z = Z;

				float slice = fmod(pos.z, 8.0f);
				float z = floor(pos.z / 8.0f);

				float xStart = fmod(z, _slicesPerRow) * _imageWidth; 
				float yStart = floor(z / _slicesPerRow) * _imageHeight;

				float x = xStart + pos.x;
				float y = yStart + pos.y; 

				float u = x/_atlasWidth;
				float v = y/_atlasHeight;

				float4 src;
				if (slice == 0){
					src = tex2D(_Atlas0, float2(u,v));
				} else if (slice == 1) {
					src = tex2D(_Atlas1, float2(u,v));
				} else if (slice == 2) {
					src = tex2D(_Atlas2, float2(u,v));
				} else if (slice == 3) {
					src = tex2D(_Atlas3, float2(u,v));
				} else if (slice == 4) {
					src = tex2D(_Atlas4, float2(u,v));
				} else if (slice == 5) {
					src = tex2D(_Atlas5, float2(u,v));
				} else if (slice == 6) {
					src = tex2D(_Atlas6, float2(u,v));
				} else if (slice == 7) {
					src = tex2D(_Atlas7, float2(u,v));
				} else {
					// Shouldn't end up here, but just in case...
					src = float4(0.0f, 0.0f, 0.0f, 0.0f);
				}
				return src;
			}

			float4 sample2D(float3 pos){
				// Take in pos in 0->1. Convert to 0--> XYZ
				float3 POS;
				POS.x = pos.x * (_imageWidth);
				POS.y = pos.y * (_imageHeight);
				POS.z = pos.z * (_imageDepth);

				float4 c;

				if (_Interp > 0.5)
				{
					// Set up corner coordinates around POS
					float x0 = floor(POS.x); 
					float x1 = ceil(POS.x);
					float y0 = floor(POS.y);
					float y1 = ceil(POS.y);
					float z0 = floor(POS.z);
					float z1 = ceil(POS.z);

					// Get interpolation fraction
					float xd = POS.x-x0; // Same thing as frac
					float yd = POS.y-y0;
					float zd = POS.z-z0;

					if (_Interp >1.5)
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

			// calculates intersection between a ray and a box
			// http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
			bool IntersectBox(float3 ray_o, float3 ray_d, float3 boxMin, float3 boxMax, out float tNear, out float tFar)
			{
			    // compute intersection of ray with all six bbox planes
			    float3 invR = 1.0 / ray_d;
			    float3 tBot = invR * (boxMin.xyz - ray_o);
			    float3 tTop = invR * (boxMax.xyz - ray_o);
			    // re-order intersections to find smallest and largest on each axis
			    float3 tMin = min (tTop, tBot);
			    float3 tMax = max (tTop, tBot);
			    // find the largest tMin and the smallest tMax
			    float2 t0 = max (tMin.xx, tMin.yz);
			    float largest_tMin = max (t0.x, t0.y);
			    t0 = min (tMax.xx, tMax.yz);
			    float smallest_tMax = min (t0.x, t0.y);
			    // check for hit
			    bool hit = (largest_tMin <= smallest_tMax);
			    tNear = largest_tMin;
			    tFar = smallest_tMax;
			    return hit;
			}

			struct vert_input {
			    float4 pos : POSITION;
			};

			struct frag_input {
			    float4 pos : SV_POSITION;
			    float3 ray_o : TEXCOORD1; // ray origin
			    float3 ray_d : TEXCOORD2; // ray direction
			};

			// vertex program
			frag_input vert(vert_input i)
			{
				frag_input o;

			    // calculate eye ray in object space
				o.ray_d = -ObjSpaceViewDir(i.pos);
				o.ray_o = i.pos.xyz - o.ray_d;
				// calculate position on screen (unused)
				o.pos = UnityObjectToClipPos(i.pos);

				return o;
			}

			// gets data value at a given position
			float4 get_data(float3 pos) {
				// sample texture (pos is normalized in [0,1])
				float4 data = sample2D(pos);
				float meanColor = (data.r + data.g + data.b) / 3.0f;

				// slice and threshold

				//float border = step(-_ClipPlane.w, dot(_ClipPlane, float4(pos-0.5, 0)));
				//data *= border;
				//border *= step(-_ClipPlane2.w, dot(_ClipPlane2, float4(pos-0.5, 0)));
				data *= step(_DataMin, meanColor);
				data *= step(meanColor, _DataMax);

				return data;
			}

			// fragment program
			float4 frag(frag_input i) : COLOR
			{
			    i.ray_d = normalize(i.ray_d);
			    // calculate eye ray intersection with cube bounding box
				float3 boxMin = { -0.5, -0.5, -0.5 };
				float3 boxMax = {  0.5,  0.5,  0.5 };
			    float tNear, tFar;
			    bool hit = IntersectBox(i.ray_o, i.ray_d, boxMin, boxMax, tNear, tFar);
			    if (!hit) discard;
			    if (tNear < 0.0) tNear = 0.0;
			    // calculate intersection points
			    float3 pNear = i.ray_o + i.ray_d*tNear;
			    float3 pFar  = i.ray_o + i.ray_d*tFar;
			    // convert to texture space
				pNear = pNear + 0.5;
				pFar  = pFar  + 0.5;
				
			    // march along ray inside the cube, accumulating color
				float3 ray_pos = pNear;
				float3 ray_dir = pFar - pNear;

				float3 ray_step = 1.7320508 * normalize(ray_dir) / _Steps;
				float mean_max_voxel = 0.0;
				float normalised_opacity = saturate(_Opacity * length(ray_step));
				//float iso_cutoff = _DataMin * (1+ _Opacity/8.0);
				float iso_cutoff = _Opacity/8.0; 
				bool rainbowDebug = (_RainbowCube == 1) ? true : false;

				float4 ray_col = 0;
				for(int k = 0; k < MAX_STEPS; k++)
				{
					if (k<_Steps){

						ray_pos = pNear + k * ray_step;
						//float4 worldRayPos = dot(float4(ray_pos-0.5f, 1.0f), _CubeScale);
						bool doClip = dot(_ClipPlane, float4(ray_pos-0.5f, 1.0f)) > 0.0f;

						if (!doClip && k<_Steps && ray_pos.x > 0 && ray_pos.y > 0 && ray_pos.z > 0
						  && ray_pos.x < 1 && ray_pos.y < 1 && ray_pos.z < 1 && ray_col.a < 1.0){
						  	float4 voxel_col;
						  	if (rainbowDebug){
						  		voxel_col.rgb = ray_pos;
						  		voxel_col.a = 0.9;
						  	} else {
						  		voxel_col = sample2D(ray_pos);
					  		}
						  	float mean_col = (voxel_col.r + voxel_col.g + voxel_col.b)/3.0f;

						  	if (_RenderMode == 0 && mean_col > _DataMin && mean_col < _DataMax){
						  		// Max Intensity Projection
						  		if (mean_col > mean_max_voxel){
						  			mean_max_voxel = mean_col;
						  			ray_col.rgb = voxel_col;
						  			ray_col.a = 0.99;
						  		}

						  	} else if (_RenderMode == 1 && mean_col > _DataMin){
						  		// Composting
						  		voxel_col.a *= normalised_opacity; //saturate(_Opacity * length(ray_step));
						  		voxel_col.rgb *= voxel_col.a;
						  		ray_col = ray_col + (1.0f-ray_col.a) * voxel_col;


						  	} else if (_RenderMode == 2 && voxel_col.a > _DataMin){
						  		// Composting built-in alpha - probably built-in alpha from Icy
						  		voxel_col.a *= normalised_opacity; //saturate(_Opacity * length(ray_step));
						  		voxel_col.rgb *= voxel_col.a;
						  		ray_col = ray_col + (1.0f-ray_col.a) * voxel_col;

						  		//voxel_col.a *= normalised_opacity; //saturate(_Opacity * length(ray_step));
						  		//voxel_col.rgb *= voxel_col.a; //* (1-0.5 * (tNear-k/_Steps) ); // This doesn't really work as it should. 
						  		//ray_col = ray_col + (1.0f-ray_col.a) * voxel_col;


						  	} else if (_RenderMode == 3 && mean_col > _DataMin && mean_col < iso_cutoff ){
						  		// 5% iso-volume - add shading?
						  		ray_col.rgb = voxel_col.rgb;
						  		ray_col.a = 1.0;
						  	} else if (_RenderMode == 4 && mean_col > _DataMin && mean_col < _DataMin * 1.1){
						  		// 10% iso-volume - add shading?
						  		ray_col.rgb = voxel_col.rgb;
						  		ray_col.a = 1.0;
						  	}

						  	// Need to sort border out still. 
							//float border = step(-_ClipPlane.w, dot(_ClipPlane, float4(ray_pos, 0)));

						}
					}
				}
				ray_col.rgb *= _Intensity;
		    	return ray_col;
			}

			ENDCG

		}

	}

	FallBack Off
}