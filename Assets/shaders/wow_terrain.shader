
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth();
}

COMMON
{
	#include "common/shared.hlsl"

	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float3 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vColor : COLOR0;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vColor = v.vColor;
		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( Anisotropic ); AddressU( EXTEND ); AddressV( EXTEND ); >;

	// Diffuse textures
	CreateInputTexture2D( Layer0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tLayer0 < Channel( RGBA, Box( Layer0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Layer1, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tLayer1 < Channel( RGBA, Box( Layer1 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Layer2, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tLayer2 < Channel( RGBA, Box( Layer2 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Layer3, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tLayer3 < Channel( RGBA, Box( Layer3 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	// Height textures
	CreateInputTexture2D( Height0, Linear, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tHeight0 < Channel( RGBA, Box( Height0 ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;

	CreateInputTexture2D( Height1, Linear, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tHeight1 < Channel( RGBA, Box( Height1 ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;

	CreateInputTexture2D( Height2, Linear, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tHeight2 < Channel( RGBA, Box( Height2 ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;

	CreateInputTexture2D( Height3, Linear, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tHeight3 < Channel( RGBA, Box( Height3 ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;

	// Blend textures
	CreateInputTexture2D( Blend0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tBlend0 < Channel( RGBA, Box( Blend0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Blend1, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tBlend1 < Channel( RGBA, Box( Blend1 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Blend2, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tBlend2 < Channel( RGBA, Box( Blend2 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	// Some helper functions to replicate GLSL type-UV math
	float2 Mod1(float2 x)
	{
		return x - floor(x);
	}

	float2 SafeUV(float2 uv, float2 texSize)
	{
		float2 wrapped = Mod1(uv);
		float2 halfTexel = 0.5 / texSize;
		return clamp(wrapped, halfTexel, 1.0 - halfTexel);
	}

	float4 pc_heightScale;   // height scale per layer
	float4 pc_heightOffset;  // height offset per layer
	float layerScale0;
	float layerScale1;
	float layerScale2;
	float layerScale3;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();

		float2 tc0 = i.vTextureCoords * (8.0 / layerScale0);
		float2 tc1 = i.vTextureCoords * (8.0 / layerScale1);
		float2 tc2 = i.vTextureCoords * (8.0 / layerScale2);
		float2 tc3 = i.vTextureCoords * (8.0 / layerScale3);

		float2 uvWrapped = i.vTextureCoords - floor(i.vTextureCoords);
		float2 uvClamped = saturate(uvWrapped * 0.998 + 0.001);

		float2 blendSize = float2(64, 64);
		float blendTex0 = g_tBlend0.Sample(g_sSampler0, SafeUV(i.vTextureCoords, blendSize)).a;
		float blendTex1 = g_tBlend1.Sample(g_sSampler0, SafeUV(i.vTextureCoords, blendSize)).a;
		float blendTex2 = g_tBlend2.Sample(g_sSampler0, SafeUV(i.vTextureCoords, blendSize)).a;

		float3 blendTex = float3(blendTex0, blendTex1, blendTex2);
		float4 layerWeights = float4(1.0 - saturate(dot(float3(1.0, 1.0, 1.0), blendTex)), blendTex);

		float4 layerPct = float4(
			layerWeights.x * (g_tHeight0.Sample(g_sSampler0, tc0).a * pc_heightScale[0] + pc_heightOffset[0]),
			layerWeights.y * (g_tHeight1.Sample(g_sSampler0, tc1).a * pc_heightScale[1] + pc_heightOffset[1]),
			layerWeights.z * (g_tHeight2.Sample(g_sSampler0, tc2).a * pc_heightScale[2] + pc_heightOffset[2]),
			layerWeights.w * (g_tHeight3.Sample(g_sSampler0, tc3).a * pc_heightScale[3] + pc_heightOffset[3])
		);

		float maxVal = max(max(layerPct.x, layerPct.y), max(layerPct.z, layerPct.w));
		float4 layerPctMax = maxVal.xxxx; 

		layerPct = layerPct * (float4(1.0, 1.0, 1.0, 1.0) - saturate(layerPctMax - layerPct));

		float sumVal = dot(float4(1.0, 1.0, 1.0, 1.0), layerPct);
		layerPct = layerPct / sumVal.xxxx;

		float4 weightedLayer_0 = g_tLayer0.Sample(g_sSampler0, tc0) * layerPct.x;
		float4 weightedLayer_1 = g_tLayer1.Sample(g_sSampler0, tc1) * layerPct.y;
		float4 weightedLayer_2 = g_tLayer2.Sample(g_sSampler0, tc2) * layerPct.z;
		float4 weightedLayer_3 = g_tLayer3.Sample(g_sSampler0, tc3) * layerPct.w;

		// TODO: Replace float3(1.0, 1.0, 1.0) with MCCV
		m.Albedo = (weightedLayer_0.rgb + weightedLayer_1.rgb + weightedLayer_2.rgb + weightedLayer_3.rgb) * i.vColor.rgb * 2.0;
		m.Normal = i.vNormalWs;
		m.TextureCoords = i.vTextureCoords.xy;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = 0;
		m.Transmission = 0;

		return ShadingModelStandard::Shade( i, m );
	}
}
