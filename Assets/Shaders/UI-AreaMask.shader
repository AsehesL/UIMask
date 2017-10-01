// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/AreaMask"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_MaskTex ("MaskTex", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float4 worldPosition : TEXCOORD0;
				float2 clipPosition : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			float4x4 internalWorldToMaskMatrix;
			half2 internalClipAtten;

			sampler2D _MaskTex;
			float4 _Offset;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
				#endif
				
				OUT.color = IN.color * _Color;

				float4 clipPos = mul(internalWorldToMaskMatrix, IN.vertex);
				OUT.clipPosition = clipPos.xy / clipPos.w;
				return OUT;
			}

			//sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{

				half4 color = IN.color;

				half2 atten = 1-saturate((abs(IN.clipPosition.xy - half2(0.5, 0.5)) - internalClipAtten.x) / (0.5 - internalClipAtten.x));

				//fixed4 colP = tex2D(_MaskTex, IN.clipPosition.xy*_Offset.xy + _Offset.zw);
				//colP.rgb = 1 - colP.rgb;
				//colP.a = colP.a * IN.color.a;
				color.a *= 1-saturate(atten.x*atten.y* tex2D(_MaskTex, IN.clipPosition.xy*_Offset.xy+_Offset.zw).a)*internalClipAtten.y;
				
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
				//return mul(_Object2World, IN.worldPosition);
				//return fixed4(IN.clipPosition.xyz, 1);
				//return IN.worldPosition;
			}
		ENDCG
		}
	}
}
