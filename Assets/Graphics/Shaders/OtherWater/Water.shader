// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		_NormalMap("Normal Map", 2D) = "white" {}
		_NormalMap2Strength("Normal Map 2 Strength", Float) = 1
		_AnimateUV1XYUV2WZ("Animate UV1 (XY) UV2 (WZ)", Vector) = (0,0,0,0)
		_UV1TilingXYScaleZW("UV 1 Tiling (XY) Scale (ZW)", Vector) = (1,1,1,1)
		_UV2TilingXYScaleZW("UV2 Tiling (XY) Scale (ZW)", Vector) = (1,1,1,1)
		_LerpStrength("Lerp Strength", Range( 0 , 1)) = 0.5
		_FresnelPowert("Fresnel Powert", Range( 0 , 4)) = 1
		_DepthFadeDistance("Depth Fade Distance", Float) = 0
		_CameraDepthFadeOFfset("Camera Depth Fade OFfset", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float eyeDepth;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _NormalMap;
		uniform float4 _AnimateUV1XYUV2WZ;
		uniform float4 _UV1TilingXYScaleZW;
		uniform float4 _UV2TilingXYScaleZW;
		uniform float _NormalMap2Strength;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFadeDistance;
		uniform float _LerpStrength;
		uniform float _FresnelPowert;
		uniform float _CameraDepthFadeOFfset;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float2 appendResult6 = (float2(ase_grabScreenPosNorm.r , ase_grabScreenPosNorm.g));
			float3 ase_worldPos = i.worldPos;
			float2 temp_output_20_0 = (ase_worldPos).xz;
			float2 appendResult25 = (float2(( _Time.x * _AnimateUV1XYUV2WZ.x ) , ( _Time.x * _AnimateUV1XYUV2WZ.y )));
			float2 appendResult31 = (float2(_UV1TilingXYScaleZW.x , _UV1TilingXYScaleZW.y));
			float2 appendResult32 = (float2(_UV1TilingXYScaleZW.z , _UV1TilingXYScaleZW.w));
			float2 UV133 = ( ( ( temp_output_20_0 + appendResult25 ) * appendResult31 ) / appendResult32 );
			float2 appendResult37 = (float2(( _Time.x * _AnimateUV1XYUV2WZ.z ) , ( _Time.x * _AnimateUV1XYUV2WZ.w )));
			float2 appendResult46 = (float2(_UV2TilingXYScaleZW.x , _UV2TilingXYScaleZW.y));
			float2 appendResult47 = (float2(_UV2TilingXYScaleZW.z , _UV2TilingXYScaleZW.w));
			float2 UV244 = ( ( ( temp_output_20_0 + appendResult37 ) * appendResult46 ) / appendResult47 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth79 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth79 = saturate( abs( ( screenDepth79 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFadeDistance ) ) );
			float DepthFade80 = distanceDepth79;
			float4 lerpResult7 = lerp( tex2D( _NormalMap, UV133 ) , float4( UnpackScaleNormal( tex2D( _NormalMap, UV244 ), ( _NormalMap2Strength * DepthFade80 ) ) , 0.0 ) , _LerpStrength);
			float4 NormalMapping9 = lerpResult7;
			float3 ScreenUV11 = ( float3( appendResult6 ,  0.0 ) - ( (NormalMapping9).rga * 0.1 ) );
			float4 screenColor1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,ScreenUV11.xy);
			float3 indirectNormal76 = WorldNormalVector( i , NormalMapping9.rgb );
			Unity_GlossyEnvironmentData g76 = UnityGlossyEnvironmentSetup( 1.0, data.worldViewDir, indirectNormal76, float3(0,0,0));
			float3 indirectSpecular76 = UnityGI_IndirectSpecular( data, 1.0, indirectNormal76, g76 );
			float4 color56 = IsGammaSpace() ? float4(0.01762192,0.4070616,0.4150943,0) : float4(0.001363926,0.1378713,0.1436938,0);
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float2 appendResult70 = (float2(ase_vertexNormal.x , ase_vertexNormal.y));
			float3 appendResult74 = (float3(( float3( appendResult70 ,  0.0 ) - (NormalMapping9).rga ).xy , ase_vertexNormal.z));
			float dotResult60 = dot( ase_worldViewDir , appendResult74 );
			float Fresnel66 = pow( ( 1.0 - saturate( abs( dotResult60 ) ) ) , _FresnelPowert );
			float cameraDepthFade86 = (( i.eyeDepth -_ProjectionParams.y - _CameraDepthFadeOFfset ) / 1.0);
			float CameraDepthFade89 = saturate( cameraDepthFade86 );
			float4 lerpResult54 = lerp( screenColor1 , ( float4( indirectSpecular76 , 0.0 ) + color56 ) , ( Fresnel66 * DepthFade80 * CameraDepthFade89 ));
			c.rgb = lerpResult54.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
0;549;1555;437;3141.709;1029.775;2.175948;True;True
Node;AmplifyShaderEditor.CommentaryNode;48;-3196.202,-41.22721;Inherit;False;1359.874;1077.846;Animated UVs;24;18;20;22;23;24;21;25;28;26;32;30;29;31;33;35;36;37;39;41;43;44;45;46;47;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;26;-3146.202,525.2612;Inherit;False;Property;_AnimateUV1XYUV2WZ;Animate UV1 (XY) UV2 (WZ);3;0;Create;True;0;0;0;False;0;False;0,0,0,0;10,10,10,10;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;22;-3070.218,373.1083;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-2816.392,619.9633;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-2813.177,720.1721;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2817.421,479.0118;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;18;-3131.844,192.8309;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-2817.421,380.4033;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;30;-2914.615,8.77277;Inherit;False;Property;_UV1TilingXYScaleZW;UV 1 Tiling (XY) Scale (ZW);4;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,10,10;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;37;-2650.111,654.0142;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;82;-1513.718,-682.3262;Inherit;False;830.2015;183.32;Comment;3;79;80;81;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;45;-3051.194,825.8705;Inherit;False;Property;_UV2TilingXYScaleZW;UV2 Tiling (XY) Scale (ZW);5;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,5,5;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;25;-2649.203,405.0554;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;20;-2884.418,210.6327;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-2543.127,34.49329;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-2535.414,240.4266;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;81;-1463.718,-618.0264;Inherit;False;Property;_DepthFadeDistance;Depth Fade Distance;8;0;Create;True;0;0;0;False;0;False;0;5.81;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-2521.426,495.4602;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-2649.575,797.2318;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-2360.759,303.8228;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;32;-2348.701,113.4298;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DepthFade;79;-1192.016,-632.3262;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;47;-2647.164,903.2994;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-2368.843,495.3203;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;29;-2195.514,304.9951;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;43;-2205.652,492.202;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;10;-1798.201,444.4554;Inherit;False;1085.17;640.0378;Normal Mapping;12;94;2;93;53;9;7;8;3;52;50;49;95;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-922.9193,-619.3264;Inherit;False;DepthFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;95;-1735.48,1002.512;Inherit;False;80;DepthFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-1789.907,912.3601;Inherit;False;Property;_NormalMap2Strength;Normal Map 2 Strength;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-2075.724,504.6748;Inherit;False;UV2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-2057.471,314.2458;Inherit;False;UV1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-1535.185,854.0451;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1717.513,523.0175;Inherit;False;33;UV1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-1722.983,770.0775;Inherit;False;44;UV2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;2;-1406.464,506.6383;Inherit;True;Property;_NormalMap;Normal Map;0;0;Create;True;0;0;0;False;0;False;-1;None;f0f09d2981ad01b4f853c72fbb05c2ac;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-1403.692,722.1864;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-1382.421,923.7669;Inherit;False;Property;_LerpStrength;Lerp Strength;6;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-1106.604,538.8213;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;67;-2397.712,-476.5224;Inherit;False;1718.007;400.908;Fresnel;14;57;58;65;60;61;62;63;64;66;70;71;72;74;75;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-944.5016,545.0186;Inherit;False;NormalMapping;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-2352.293,-218.8524;Inherit;False;9;NormalMapping;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;57;-2342.206,-399.5377;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;72;-2129.402,-218.8523;Inherit;False;True;True;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;70;-2122.69,-381.3204;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;75;-1897.115,-249.7348;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;58;-1866.58,-417.1234;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;74;-1718.533,-249.7347;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;60;-1670.548,-413.3653;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;12;-1798.128,-29.8039;Inherit;False;1117.821;410.9539;Screen UVs;8;15;13;17;14;11;5;6;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;90;-1857.444,-962.9595;Inherit;False;1164.041;256.3782;Camera Depth Fade;5;92;86;88;87;89;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;13;-1775.136,196.0088;Inherit;False;9;NormalMapping;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.AbsOpNode;61;-1532.427,-413.5282;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;62;-1394.769,-414.5563;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-1822,-905.4547;Inherit;False;Constant;_CameraDepthFadeLength;Camera Depth Fade Length;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;4;-1605.413,20.19608;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;17;-1540.166,191.9446;Inherit;False;True;True;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-1809.702,-812.3462;Inherit;False;Property;_CameraDepthFadeOFfset;Camera Depth Fade OFfset;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1510.928,280.9791;Inherit;False;Constant;_Constant01;Constant 0.1;2;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-1353.965,-342.4105;Inherit;False;Property;_FresnelPowert;Fresnel Powert;7;0;Create;True;0;0;0;False;0;False;1;1;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;86;-1439.028,-880.8598;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;63;-1244.937,-415.2418;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;6;-1308.067,60.18182;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1310.805,186.5375;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;92;-1140.897,-873.031;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;64;-1066.021,-416.6126;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;5;-1146.633,53.80938;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-948.8181,90.22099;Inherit;False;ScreenUV;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-893.5929,-419.3262;Inherit;False;Fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-619.6902,253.8603;Inherit;False;9;NormalMapping;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;89;-951.6837,-888.3649;Inherit;False;CameraDepthFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;56;-384.7915,375.3762;Inherit;False;Constant;_MainCOlor;Main COlor;7;0;Create;True;0;0;0;False;0;False;0.01762192,0.4070616,0.4150943,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IndirectSpecularLight;76;-403.5537,252.1448;Inherit;False;Tangent;3;0;FLOAT3;0,0,1;False;1;FLOAT;1;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-339.7217,671.5727;Inherit;False;80;DepthFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;-357.676,770.0411;Inherit;False;89;CameraDepthFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-331.8023,594.8389;Inherit;False;66;Fresnel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-515.4213,70.54327;Inherit;False;11;ScreenUV;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenColorNode;1;-326.1857,69.40522;Inherit;False;Global;_GrabScreen0;Grab Screen 0;0;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;78;-160.2244,295.0774;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-99.72168,551.5727;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-1775.398,634.2302;Inherit;False;Property;_NormalMap1Strength;Normal Map 1 Strength;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;-1534.557,601.0078;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;54;-28.31759,238.8497;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;132.2356,-46.67139;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;36;0;22;1
WireConnection;36;1;26;3
WireConnection;35;0;22;1
WireConnection;35;1;26;4
WireConnection;24;0;22;1
WireConnection;24;1;26;2
WireConnection;23;0;22;1
WireConnection;23;1;26;1
WireConnection;37;0;36;0
WireConnection;37;1;35;0
WireConnection;25;0;23;0
WireConnection;25;1;24;0
WireConnection;20;0;18;0
WireConnection;31;0;30;1
WireConnection;31;1;30;2
WireConnection;21;0;20;0
WireConnection;21;1;25;0
WireConnection;39;0;20;0
WireConnection;39;1;37;0
WireConnection;46;0;45;1
WireConnection;46;1;45;2
WireConnection;28;0;21;0
WireConnection;28;1;31;0
WireConnection;32;0;30;3
WireConnection;32;1;30;4
WireConnection;79;0;81;0
WireConnection;47;0;45;3
WireConnection;47;1;45;4
WireConnection;41;0;39;0
WireConnection;41;1;46;0
WireConnection;29;0;28;0
WireConnection;29;1;32;0
WireConnection;43;0;41;0
WireConnection;43;1;47;0
WireConnection;80;0;79;0
WireConnection;44;0;43;0
WireConnection;33;0;29;0
WireConnection;93;0;53;0
WireConnection;93;1;95;0
WireConnection;2;1;49;0
WireConnection;3;1;50;0
WireConnection;3;5;93;0
WireConnection;7;0;2;0
WireConnection;7;1;3;0
WireConnection;7;2;8;0
WireConnection;9;0;7;0
WireConnection;72;0;71;0
WireConnection;70;0;57;1
WireConnection;70;1;57;2
WireConnection;75;0;70;0
WireConnection;75;1;72;0
WireConnection;74;0;75;0
WireConnection;74;2;57;3
WireConnection;60;0;58;0
WireConnection;60;1;74;0
WireConnection;61;0;60;0
WireConnection;62;0;61;0
WireConnection;17;0;13;0
WireConnection;86;0;87;0
WireConnection;86;1;88;0
WireConnection;63;0;62;0
WireConnection;6;0;4;1
WireConnection;6;1;4;2
WireConnection;14;0;17;0
WireConnection;14;1;15;0
WireConnection;92;0;86;0
WireConnection;64;0;63;0
WireConnection;64;1;65;0
WireConnection;5;0;6;0
WireConnection;5;1;14;0
WireConnection;11;0;5;0
WireConnection;66;0;64;0
WireConnection;89;0;92;0
WireConnection;76;0;77;0
WireConnection;1;0;16;0
WireConnection;78;0;76;0
WireConnection;78;1;56;0
WireConnection;84;0;68;0
WireConnection;84;1;85;0
WireConnection;84;2;91;0
WireConnection;94;0;52;0
WireConnection;94;1;95;0
WireConnection;54;0;1;0
WireConnection;54;1;78;0
WireConnection;54;2;84;0
WireConnection;0;13;54;0
ASEEND*/
//CHKSM=5E88601C0DF7C3011DAA7973AC54106449C81730