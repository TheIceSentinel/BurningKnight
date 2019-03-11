#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D SpriteTexture;
sampler s0;
float blur;

sampler2D SpriteTextureSampler = sampler_state {
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput {
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR {
	float mx = 1.0f / 320 * blur;
	float my = 1.0f / 180 * blur;
	float4 color = tex2D(s0, input.TextureCoordinates.xy);
	float v = smoothstep(0.75f, 0.3f, length(input.TextureCoordinates - float2(0.5f, 0.5f)));

	if (blur > 0.001f) {		
		for (int xx = -2; xx < 3; xx++) {
			for (int yy = -2; yy < 3; yy++) {
				if (xx != 0 || yy != 0) {
					color += tex2D(s0, input.TextureCoordinates + float2(mx * xx , my * yy)) / sqrt(xx * xx + yy * yy);
				}
			}
		}
	
	  color /= 14.8203f;
	}	

	return color * v;
}

technique SpriteDrawing {
	pass P0 {
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};