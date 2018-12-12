Shader "peti446/BuidlingShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)

		_BuildingTextureIndex("Building Texture Index", Int) = 0
		_BurningTextureIndex("Burning Texture Index", Int) = 0
		_MaxBuildingTexturesCount("Max Base Textures", Int) = 0
		_MaxBurningTexturesCount("Max Burning Textures", Int) = 0

		_BuildingTextures ("Base Texturees", 2DArray) = "" {}
		_BurningTextures("Burning Textures", 2DArray) = "" {}

		_Occlusion("Occlusion", 2D) = "white" {}
		_OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0
		#include "UnityCG.cginc"

		struct Input {
			float2 uv_BuildingTextures: TEXCOORD0;
			float2 uv2_BurningTextures: TEXCOORD1;
			float2 uv3_Occlusion : TEXCOORD2;
		};

		UNITY_DECLARE_TEX2DARRAY(_BuildingTextures);
		UNITY_DECLARE_TEX2DARRAY(_BurningTextures);
		int _MaxBuildingTexturesCount;
		int _MaxBurningTexturesCount;
		half _OcclusionStrength;
		sampler2D _Occlusion;
		half _Glossiness;
		half _Metallic;
		float4 _Color;
		int _BuildingTextureIndex;
		int _BurningTextureIndex;


		//Function to combine the fire texture with the base textures
		fixed4 CombineBurning(fixed4 base, fixed4 burning)
		{
			//Get the resulting colour using alpha blending
			fixed3 combinedColour = lerp(base.rgb, burning.rgb, burning.a);
			//We will be using the base alpha as the resulting alpha
			return float4(combinedColour, base.a);
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			//Check if all the indexes ar valid
			//Check building index
			if(_BuildingTextureIndex >= _MaxBuildingTexturesCount)
			{
				_BuildingTextureIndex = _MaxBuildingTexturesCount - 1;
			}
			else if(_BuildingTextureIndex < 0)
			{
				_BuildingTextureIndex = 0;
			}
			//Check the burning index
			if (_BurningTextureIndex > _MaxBurningTexturesCount)
			{
				_BurningTextureIndex = _MaxBurningTexturesCount;
			}
			else if (_BurningTextureIndex < 0)
			{
				_BurningTextureIndex = 0;
			}


			//Sample the array texture to get the colour for the bulding
			fixed4 buildingColor = UNITY_SAMPLE_TEX2DARRAY(_BuildingTextures, float3(IN.uv_BuildingTextures, _BuildingTextureIndex));
			//By default we dont have a burning texture on it
			fixed4 buildingFire = float4(0, 0, 0, 0);
			//Only sample the burning texture if the index is higer then 0
			if (_BurningTextureIndex > 0)
			{
				//Sample the texture array to get the the burning colour, the given texture array does not have a non burning texture, but we treat 0 as a non texture,
				//so we need to substract 1 from the actually index to get the correct position in the array
				buildingFire = UNITY_SAMPLE_TEX2DARRAY(_BurningTextures, float3(IN.uv2_BurningTextures, _BurningTextureIndex - 1 < 0 ? 0 : _BurningTextureIndex));
			}
			//Get thhe combined texture colour
			fixed4 finalColor = CombineBurning(buildingColor, buildingFire) *  _Color;
			//Apply all the variables to the surface shader output
			o.Albedo = finalColor.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Occlusion = tex2D(_Occlusion, IN.uv3_Occlusion).r;
			o.Alpha = finalColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
