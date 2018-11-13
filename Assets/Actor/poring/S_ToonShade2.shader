// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_ToonShade2"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.0005
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		[KeywordEnum(Solidcolor,Albedo)] _AlbedoType("Albedo Type", Float) = 0
		_SolidColor("SolidColor", Color) = (0.4862745,0.4862745,0.4862745,0)
		_Texture("Albedo", 2D) = "gray" {}
		_RampTexture("Ramp Texture", 2D) = "white" {}
		_RampPower("RampPower", Range( 0 , 1.5)) = 0
		_PoringMask("Poring Mask", 2D) = "white" {}
		[Toggle(_INNERGLOW_ON)] _InnerGlow("InnerGlow", Float) = 1
		_InnerColor("Inner Color", Color) = (1,1,1,0)
		_InnerOffset("Inner Offset", Range( 0 , 1)) = 0
		_InnerPower("Inner Power", Range( 0 , 1)) = 1
		[Toggle(_RIMLIGHT_ON)] _RimLight("Rim Light", Float) = 0
		[HDR]_RimColor("Rim Color", Color) = (0,1,0.8758622,0)
		_RimOffset("Rim Offset", Range( 0 , 10)) = 0.4
		_RimPower("Rim Power", Range( 0 , 10)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		uniform fixed4 _ASEOutlineColor;
		uniform fixed _ASEOutlineWidth;
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += ( v.normal * _ASEOutlineWidth );
		}
		inline fixed4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return fixed4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _ASEOutlineColor.rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _ALBEDOTYPE_SOLIDCOLOR _ALBEDOTYPE_ALBEDO
		#pragma shader_feature _RIMLIGHT_ON
		#pragma shader_feature _INNERGLOW_ON
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			float3 worldPos;
			float3 viewDir;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float4 _SolidColor;
		uniform sampler2D _Texture;
		uniform float4 _Texture_ST;
		uniform sampler2D _RampTexture;
		uniform float _RampPower;
		uniform float _RimOffset;
		uniform float _RimPower;
		uniform float4 _RimColor;
		uniform float _InnerOffset;
		uniform float _InnerPower;
		uniform float4 _InnerColor;
		uniform sampler2D _PoringMask;
		uniform float4 _PoringMask_ST;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#if DIRECTIONAL
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			float2 uv_Texture = i.uv_texcoord * _Texture_ST.xy + _Texture_ST.zw;
			#if defined(_ALBEDOTYPE_SOLIDCOLOR)
				float4 staticSwitch171 = _SolidColor;
			#elif defined(_ALBEDOTYPE_ALBEDO)
				float4 staticSwitch171 = tex2D( _Texture, uv_Texture );
			#else
				float4 staticSwitch171 = _SolidColor;
			#endif
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 temp_output_169_0 = ( staticSwitch171 * ase_lightColor );
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3 = dot( ase_worldNormal , ase_worldlightDir );
			float2 temp_cast_0 = (saturate( (dotResult3*0.5 + 0.5) )).xx;
			float4 temp_output_164_0 = ( tex2D( _RampTexture, temp_cast_0 ) * _RampPower );
			float2 temp_cast_1 = (saturate( (dotResult3*0.5 + 0.5) )).xx;
			float dotResult38 = dot( ase_worldNormal , i.viewDir );
			#ifdef _RIMLIGHT_ON
				float4 staticSwitch184 = ( saturate( ( ( ase_lightAtten * dotResult3 ) * pow( ( 1.0 - saturate( ( dotResult38 + _RimOffset ) ) ) , _RimPower ) ) ) * ( _RimColor * ase_lightColor ) );
			#else
				float4 staticSwitch184 = float4( 0,0,0,0 );
			#endif
			float4 temp_output_88_0 = ( ( min( temp_output_169_0 , temp_output_164_0 ) + max( temp_output_169_0 , temp_output_164_0 ) ) + staticSwitch184 );
			float2 temp_cast_2 = (saturate( (dotResult3*0.5 + 0.5) )).xx;
			float2 temp_cast_3 = (saturate( (dotResult3*0.5 + 0.5) )).xx;
			float dotResult93 = dot( ase_worldNormal , i.viewDir );
			float temp_output_108_0 = saturate( pow( ( 1.0 - saturate( (dotResult93*1.0 + _InnerOffset) ) ) , _InnerPower ) );
			float4 temp_cast_4 = (temp_output_108_0).xxxx;
			float4 lerpResult126 = lerp( temp_cast_4 , ( _InnerColor * ase_lightColor ) , temp_output_108_0);
			#ifdef _INNERGLOW_ON
				float4 staticSwitch185 = lerpResult126;
			#else
				float4 staticSwitch185 = temp_output_88_0;
			#endif
			float2 uv_PoringMask = i.uv_texcoord * _PoringMask_ST.xy + _PoringMask_ST.zw;
			float4 lerpResult187 = lerp( temp_output_88_0 , ( staticSwitch185 * tex2D( _PoringMask, uv_PoringMask ) ) , temp_output_108_0);
			c.rgb = lerpResult187.rgb;
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
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred 

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
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
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
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15301
1921;23;1918;1016;3250.726;1099.638;2.865285;True;True
Node;AmplifyShaderEditor.CommentaryNode;49;-1988.439,562.4507;Float;False;507.201;385.7996;Comment;3;36;37;38;N . V;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;50;-1340.1,774.4999;Float;False;1617.938;553.8222;;3;28;24;34;Rim Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;37;-1948.22,755.2253;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;36;-1942.162,610.4507;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;48;-1997.052,52.67056;Float;False;540.401;320.6003;Comment;3;1;3;2;N . L;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;38;-1695.006,650.8324;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1255.423,1075.649;Float;False;Property;_RimOffset;Rim Offset;12;0;Create;True;0;0;False;0;0.4;0.45;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;2;-1996.786,174.5431;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;51;-1143.099,-115.6995;Float;False;723.599;290;Also know as Lambert Wrap or Half Lambert;3;5;15;4;Diffuse Wrap;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-1883.33,90.33527;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-1052.1,934.4999;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;3;-1643.561,142.2774;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;91;-323.9021,-806.6646;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;27;-892.1,934.4999;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;97;-317.5257,-656.6913;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;5;-1117.081,28.4001;Float;False;Constant;_Wrapper;Wrapper;7;0;Create;True;0;0;False;0;0.5;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;172;-373.902,-856.6647;Float;False;1485.493;690.6901;Inner Glow;7;103;98;104;108;126;106;107;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;52;-914.1411,395.395;Float;False;812;304;Comment;5;8;11;12;7;10;Attenuation and Ambient;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;173;-1134.321,-752.2557;Float;False;728.6089;573.2542;Albedo;4;54;127;171;168;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LightAttenuation;7;-852.2765,516.8029;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-760.5799,1075.064;Float;False;Property;_RimPower;Rim Power;13;0;Create;True;0;0;False;0;0;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;4;-827.6974,-65.69949;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;29;-716.1,934.4999;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;93;-101.2512,-797.0776;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-106.6327,-572.6749;Float;False;Property;_InnerOffset;Inner Offset;8;0;Create;True;0;0;False;0;0;0.677;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;54;-1030.032,-702.2557;Float;False;Property;_SolidColor;SolidColor;1;0;Create;True;0;0;False;0;0.4862745,0.4862745,0.4862745,0;1,0.6397059,0.7632353,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;127;-1084.321,-532.0815;Float;True;Property;_Texture;Albedo;2;0;Create;False;0;0;False;0;None;cf4a4eec750d252459964e0300539bb5;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;15;-594.4999,-58.89988;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-540.1,822.4999;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;99;41.78559,-791.3214;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;30;-524.0999,934.4999;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;168;-961.9782,-335.0016;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;6;-343.3965,-16.59763;Float;True;Property;_RampTexture;Ramp Texture;3;0;Create;True;0;0;False;0;None;4ad083452d3645d49b783cd271e41ab8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;163;-329.6497,191.9675;Float;False;Property;_RampPower;RampPower;4;0;Create;True;0;0;False;0;0;0;0;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;100;249.0061,-770.4028;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-284.1001,902.4999;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;171;-730.6167,-682.767;Float;False;Property;_AlbedoType;Albedo Type;0;0;Create;True;0;0;False;0;0;0;1;True;;KeywordEnum;2;Solidcolor;Albedo;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;34;-364.1001,1046.5;Float;False;Property;_RimColor;Rim Color;11;1;[HDR];Create;True;0;0;False;0;0,1,0.8758622,0;1,0,0,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;47;-252.1,1222.5;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SaturateNode;32;-92.09998,902.4999;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;101;392.8018,-770.7438;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;-574.7111,-429.1086;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;164;17.13867,126.9399;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-60.1,1030.5;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;103;262.441,-668.6432;Float;False;Property;_InnerPower;Inner Power;9;0;Create;True;0;0;False;0;1;0.128;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;104;343.6575,-548.4335;Float;False;Property;_InnerColor;Inner Color;7;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;102;554.1354,-775.0917;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;106;346.7541,-364.9753;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;158.4,901.2;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;192;302.879,-142.3851;Float;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;194;309.0154,73.77109;Float;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;184;467.7033,406.9677;Float;False;Property;_RimLight;Rim Light;10;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;678.4343,-546.8902;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;195;583.0154,8.771088;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;108;731.0187,-773.9102;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;927.0784,-67.70786;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;126;928.544,-774.4852;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;185;1297.303,-667.3466;Float;False;Property;_InnerGlow;InnerGlow;6;0;Create;True;0;0;False;0;0;1;1;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;196;1196.493,18.0787;Float;True;Property;_PoringMask;Poring Mask;5;0;Create;True;0;0;False;0;None;89aad79dd71eeab45b72e7b14e9d373b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;1545.493,103.0787;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-426.6764,492.8029;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;162;97.5011,-101.7015;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;8;-824.5342,404.6666;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.IndirectDiffuseLighting;11;-663.6539,482.3186;Float;False;Tangent;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;78;16.75501,401.2959;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0.4411765,0.4411765,0.4411765,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-329.2684,417.7891;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;187;1461.117,-385.2124;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1758.444,63.92109;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;S_ToonShade2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0.0005;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;38;0;36;0
WireConnection;38;1;37;0
WireConnection;25;0;38;0
WireConnection;25;1;24;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;27;0;25;0
WireConnection;4;0;3;0
WireConnection;4;1;5;0
WireConnection;4;2;5;0
WireConnection;29;0;27;0
WireConnection;93;0;91;0
WireConnection;93;1;97;0
WireConnection;15;0;4;0
WireConnection;35;0;7;0
WireConnection;35;1;3;0
WireConnection;99;0;93;0
WireConnection;99;2;98;0
WireConnection;30;0;29;0
WireConnection;30;1;28;0
WireConnection;6;1;15;0
WireConnection;100;0;99;0
WireConnection;31;0;35;0
WireConnection;31;1;30;0
WireConnection;171;1;54;0
WireConnection;171;0;127;0
WireConnection;32;0;31;0
WireConnection;101;0;100;0
WireConnection;169;0;171;0
WireConnection;169;1;168;0
WireConnection;164;0;6;0
WireConnection;164;1;163;0
WireConnection;46;0;34;0
WireConnection;46;1;47;0
WireConnection;102;0;101;0
WireConnection;102;1;103;0
WireConnection;33;0;32;0
WireConnection;33;1;46;0
WireConnection;192;0;169;0
WireConnection;192;1;164;0
WireConnection;194;0;169;0
WireConnection;194;1;164;0
WireConnection;184;0;33;0
WireConnection;107;0;104;0
WireConnection;107;1;106;0
WireConnection;195;0;192;0
WireConnection;195;1;194;0
WireConnection;108;0;102;0
WireConnection;88;0;195;0
WireConnection;88;1;184;0
WireConnection;126;0;108;0
WireConnection;126;1;107;0
WireConnection;126;2;108;0
WireConnection;185;1;88;0
WireConnection;185;0;126;0
WireConnection;197;0;185;0
WireConnection;197;1;196;0
WireConnection;12;0;11;0
WireConnection;12;1;7;0
WireConnection;78;0;10;0
WireConnection;10;0;8;0
WireConnection;10;1;12;0
WireConnection;187;0;88;0
WireConnection;187;1;197;0
WireConnection;187;2;108;0
WireConnection;0;13;187;0
ASEEND*/
//CHKSM=16C11B6CABA5DAB681E2F1B2CDD573527479C893