// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_Speed("Speed", Vector) = (0.1,0.1,0,0)
		_FlowMap("FlowMap", 2D) = "white" {}
		_Tiling("Tiling", Float) = 1
		_FlorIntencity("FlorIntencity", Range( 0 , 1)) = 0.93
		_FlowSpeed("FlowSpeed", Float) = -0.1
		_Range("Range", Float) = 0
		_Distance("Distance", Float) = 0
		_FallOff("FallOff", Float) = 0
		_speed("speed", Float) = 0
		_freq("freq", Float) = 0
		_Amplitude("Amplitude", Range( 0 , 0.1)) = 0
		_Height("Height", Range( 0 , 2)) = 0
		_Direction("Direction", Vector) = (1,0,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float _speed;
		uniform float _freq;
		uniform float _Amplitude;
		uniform float3 _Direction;
		uniform float _Height;
		uniform sampler2D _MainTex;
		uniform float2 _Speed;
		uniform float _Tiling;
		uniform sampler2D _FlowMap;
		uniform float _FlowSpeed;
		uniform float _FlorIntencity;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Distance;
		uniform float _Range;
		uniform float _FallOff;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float mulTime5 = _Time.y * _speed;
			v.vertex.xyz += saturate( ( sin( ( ( distance( ase_vertex3Pos , float3( 0,0,0 ) ) + mulTime5 ) * _freq ) ) * _Amplitude * _Direction * _Height ) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color30 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float2 temp_cast_0 = (_Tiling).xx;
			float2 uv_TexCoord10 = i.uv_texcoord * temp_cast_0;
			float2 temp_cast_2 = (_FlowSpeed).xx;
			float2 panner7 = ( 1.0 * _Time.y * temp_cast_2 + i.uv_texcoord);
			float4 tex2DNode16 = tex2D( _FlowMap, panner7 );
			float4 lerpResult17 = lerp( float4( uv_TexCoord10, 0.0 , 0.0 ) , tex2DNode16 , _FlorIntencity);
			float2 panner25 = ( 1.0 * _Time.y * _Speed + lerpResult17.rg);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth13 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth13 = abs( ( screenDepth13 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Distance ) );
			float temp_output_31_0 = saturate( pow( ( distanceDepth13 / _Range ) , _FallOff ) );
			float4 lerpResult32 = lerp( color30 , tex2D( _MainTex, panner25 ) , temp_output_31_0);
			o.Emission = lerpResult32.rgb;
			o.Alpha = temp_output_31_0;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
252.8;566.4;1282.2;203.6;1382.918;-760.7939;1.215541;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-2745.3,-2.252401;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;2;-2718.073,-71.65508;Inherit;False;Property;_FlowSpeed;FlowSpeed;5;0;Create;True;0;0;0;False;0;False;-0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;4;-1939.317,637.4623;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-1704.798,900.7809;Inherit;False;Property;_speed;speed;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-2906.606,95.6999;Inherit;False;Property;_Tiling;Tiling;3;0;Create;True;0;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;5;-1549.039,857.0593;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1306.113,375.1302;Inherit;False;Property;_Distance;Distance;7;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;9;-1585.929,693.1026;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;7;-2435.724,-28.31796;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-2684.791,172.5024;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;13;-1117.113,344.1302;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-2284.561,-110.167;Inherit;True;Property;_FlowMap;FlowMap;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-1997.254,334.6629;Inherit;False;Property;_FlorIntencity;FlorIntencity;4;0;Create;True;0;0;0;False;0;False;0.93;0.192;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-920.1123,447.1302;Inherit;False;Property;_Range;Range;6;0;Create;True;0;0;0;False;0;False;0;0.81;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1372.35,944.5168;Inherit;False;Property;_freq;freq;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-1333.163,740.9233;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1076.132,766.5605;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-719.1122,486.1302;Inherit;False;Property;_FallOff;FallOff;8;0;Create;True;0;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;19;-1506.938,227.914;Inherit;False;Property;_Speed;Speed;1;0;Create;True;0;0;0;False;0;False;0.1,0.1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-757.1122,345.1302;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;-1778.054,189.0629;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;25;-1282.938,92.914;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-797.9341,811.1076;Inherit;False;Property;_Amplitude;Amplitude;11;0;Create;True;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;24;-831.7232,694.477;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;23;-577.1122,376.1302;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-712.3348,1127.6;Inherit;False;Property;_Height;Height;12;0;Create;True;0;0;0;False;0;False;0;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;27;-707.8298,909.0963;Inherit;False;Property;_Direction;Direction;13;0;Create;True;0;0;0;False;0;False;1,0,1;1,0,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;30;-737.9216,-149.4122;Inherit;False;Constant;_Color0;Color 0;9;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;28;-1094.939,72.914;Inherit;True;Property;_MainTex;Main Tex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;31;-354.0239,366.83;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-467.9266,725.5084;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;32;-243.3717,10.63157;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;33;-242.6657,740.1505;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-1827.439,-26.15734;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT2;0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;3;0
WireConnection;9;0;4;0
WireConnection;7;0;1;0
WireConnection;7;2;2;0
WireConnection;10;0;8;0
WireConnection;13;0;6;0
WireConnection;16;1;7;0
WireConnection;12;0;9;0
WireConnection;12;1;5;0
WireConnection;18;0;12;0
WireConnection;18;1;11;0
WireConnection;21;0;13;0
WireConnection;21;1;14;0
WireConnection;17;0;10;0
WireConnection;17;1;16;0
WireConnection;17;2;15;0
WireConnection;25;0;17;0
WireConnection;25;2;19;0
WireConnection;24;0;18;0
WireConnection;23;0;21;0
WireConnection;23;1;20;0
WireConnection;28;1;25;0
WireConnection;31;0;23;0
WireConnection;29;0;24;0
WireConnection;29;1;22;0
WireConnection;29;2;27;0
WireConnection;29;3;26;0
WireConnection;32;0;30;0
WireConnection;32;1;28;0
WireConnection;32;2;31;0
WireConnection;33;0;29;0
WireConnection;34;0;16;0
WireConnection;34;1;10;0
WireConnection;0;2;32;0
WireConnection;0;9;31;0
WireConnection;0;11;33;0
ASEEND*/
//CHKSM=1FE50E638817AC2634A75CCF43D92A8D1F940AA5