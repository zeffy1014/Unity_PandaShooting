﻿// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Default_Custom"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex SpriteVert
				#pragma fragment SpriteFrag_Custom  // 呼び出される関数を変える
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_local _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				#include "UnitySprites.cginc"

				// 基本的にはUnitySprites.cgincからコピー
				sampler2D _MainTex_Custom;
				fixed4 SpriteFrag_Custom(v2f IN) : SV_Target
				{
					// とりあえず指定色をかける
					fixed4 c = SampleSpriteTexture(IN.texcoord) * _Color;
					// 指定色のRGB各値が0.5なら元の色、0.5を超えると白に近づくように調整
					c.rgb = c.rgb * 2.0 + max(fixed3(0,0,0), _Color.rgb - 0.5) * 2.0;
					c.rgb *= c.a;
					return c;
				}

			ENDCG
			}
		}
}

