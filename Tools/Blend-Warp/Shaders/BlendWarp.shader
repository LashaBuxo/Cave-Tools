// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/BlendWarp"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BrightnessAmount("Brightness Amount", Range(0.0, 1.0)) = 1.0
		_SaturationAmount("Saturation Amount", Range(0.0, 1.0)) = 1.0
		_ContrastAmount("Contrast Amount", Range(0.0, 1.0)) = 1.0

		_BlendingLeft("Left Blending size", Range(0.0, 1.0)) = 0
		_BlendingRight("Right Blending size", Range(0.0, 1.0)) = 0
		_BlendingTop("Top Blending size", Range(0.0, 1.0)) = 0
		_BlendingBottom("Bottom Blending size", Range(0.0, 1.0)) = 0
		_Transparency("Transparency", Range(0.0,0.5)) = 0.25

		_FunctionDegree("Blending Function Degree", Range(0.1, 5.0)) = 1.0
	}
		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				// declare corresponding CGPROGRAM variables
				uniform sampler2D _MainTex;
				fixed _BrightnessAmount;
				fixed _SaturationAmount;
				fixed _ContrastAmount;

				fixed _BlendingLeft;
				fixed _BlendingRight;
				fixed _BlendingTop;
				fixed _BlendingBottom;
				fixed _Transparency;
				fixed _FunctionDegree;

				float4 _MainTex_TexelSize;

				float3 BrightnessSaturationContrast(float3 color, float brightness, float saturation, float contrast)
				{
					// adjust these values to adjust R, G, B colors separately
					float avgLumR = 0.5;
					float avgLumG = 0.5;
					float avgLumB = 0.5;

					// luminance coefficient for getting luminance from the image
					float3 luminanceCoeff = float3(0.2125, 0.7154, 0.0721);

					// Brightness calculation
					float3 avgLum = float3(avgLumR, avgLumG, avgLumB);
					float3 brightnessColor = color * brightness;
					float intensityf = dot(brightnessColor, luminanceCoeff);
					float3 intensity = float3(intensityf, intensityf, intensityf);

					// Saturation calculation
					float3 saturationColor = lerp(intensity, brightnessColor, saturation);

					// Contrast calculation
					float3 contrastColor = lerp(avgLum, saturationColor, contrast);

					return contrastColor;
				}

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				sampler2D _Alpha;
				fixed4 frag(v2f i) : COLOR
				{
					// Get the colors from the RenderTexture and the UVs from the v2f_img struct
					fixed4 renderTex = tex2D(_MainTex, i.uv);
				// Apply the Brightness, Saturation, and Contrast calculations
				float cur_x = i.uv.x;

				if (cur_x > 1 - _BlendingRight) {
					float bright_val = _BrightnessAmount + pow(1 - cur_x, _FunctionDegree) / (_BlendingRight)*(1 - _BrightnessAmount);
					renderTex.rgb = BrightnessSaturationContrast(renderTex.rgb, bright_val, _SaturationAmount, _ContrastAmount);
				}

				if (cur_x < _BlendingLeft) {
					float bright_val = _BrightnessAmount + pow(cur_x, _FunctionDegree) / (_BlendingLeft)*(1 - _BrightnessAmount);
					renderTex.rgb = BrightnessSaturationContrast(renderTex.rgb, bright_val, _SaturationAmount, _ContrastAmount);
				}

				if (i.uv.y < _BlendingBottom) renderTex.rgb = BrightnessSaturationContrast(renderTex.rgb, _BrightnessAmount, _SaturationAmount, _ContrastAmount);
				if (i.uv.y > 1 - _BlendingTop) renderTex.rgb = BrightnessSaturationContrast(renderTex.rgb, _BrightnessAmount, _SaturationAmount, _ContrastAmount);
				return  renderTex;
			}
			struct appdata
			{
				float4 vertex : POSITION; // vertex position
				float2 uv : TEXCOORD0; // texture coordinate
			};
			v2f vert(appdata v)
			{
				v2f o;
				// transform position to clip space
				// (multiply with model*view*projection matrix)
				o.pos = UnityObjectToClipPos(v.vertex);
				// just pass the texture coordinate
				o.uv = v.uv;
				return o;
			}

			ENDCG
		}
		}
}
