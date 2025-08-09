
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
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	int WMOVertexShader;

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		return FinalizeVertex( i );
	}
}

PS
{
	// Largely based on https://github.com/Deamon87/WebWowViewerCpp/blob/cf3c4bb75d96c6d33056160db57018d63ce92575/wowViewerLib/shaders/glsl/forwardRendering/wmoShader.frag
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler1 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler2 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler3 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler4 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler5 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler6 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler7 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler8 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler9 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;

	CreateInputTexture2D( Color0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor0 < Channel( RGBA, Box( Color0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color1, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor1 < Channel( RGBA, Box( Color1 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color2, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor2 < Channel( RGBA, Box( Color2 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color3, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor3 < Channel( RGBA, Box( Color3 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color4, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor4 < Channel( RGBA, Box( Color4 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color5, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor5 < Channel( RGBA, Box( Color5 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color6, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor6 < Channel( RGBA, Box( Color6 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color7, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor7 < Channel( RGBA, Box( Color7 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	CreateInputTexture2D( Color8, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor8 < Channel( RGBA, Box( Color8 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); >;

	int WMOPixelShader;
	int WMOBlendMode;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float distFade = 1.0f;

		Material m = Material::Init();
		// defaults
		m.Albedo = float3( 1.00, 0.00, 0.00 ); // red = bad
		m.Opacity = 1.0f;
		m.Emission = 0.0f;

		if ( WMOPixelShader == 0 ) // MapObjDiffuse
		{
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		} 
		else if( WMOPixelShader == 1 ) // MapObjSpecular
		{	m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			// todo: spec
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 2 ) // MapObjMetal
		{
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			// todo: spec
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 3 ) //MapObjEnv
		{
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Emission = g_tColor1.Sample(g_sSampler0, i.vTextureCoords.zw ).rgb * g_tColor0.Sample(g_sSampler0, i.vTextureCoords.zw ).a * distFade;
			m.Opacity = 1.0f;
		}
		else if ( WMOPixelShader == 4 )
		{
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = 1.0f;
		}
		else if ( WMOPixelShader == 5 )
		{
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Emission = ((g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb * g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a) *  g_tColor1.Sample(g_sSampler0, i.vTextureCoords.zw ).a)* distFade;
			m.Opacity = 1.0f;
		}
		else if ( WMOPixelShader == 6 )
		{
			float3 layer1 = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			float3 layer2 = lerp(layer1, g_tColor1.Sample(g_sSampler0, i.vTextureCoords.zw ).rgb, g_tColor1.Sample(g_sSampler0, i.vTextureCoords.zw ).a);
			m.Albedo = lerp(layer2, layer1, 1.0f); // TODO: 1.0f = vColor2.a equivalent
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 7 )
		{
			float4 mixedColor = lerp(g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ), g_tColor1.Sample(g_sSampler0, i.vTextureCoords.zw ), 1.0f); // TODO: 1.0f = vColor2.a equivalent
			m.Albedo = mixedColor.rgb;
			float3 tex3 = g_tColor2.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Emission = (mixedColor.rgb * mixedColor.a) * tex3 * distFade;
			m.Opacity = mixedColor.a;
		}
		else if ( WMOPixelShader == 8 )
		{
			// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 9 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 10 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 11 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 12 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 13 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 14 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 15 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 16 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 17 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 18 )
		{
						// TODO 
			m.Albedo = float3(1.0, 0.0, 0.0);
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 19 )
		{
						// TODO 
			m.Albedo = float3(0.0, 1.0, 0.0);
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 20 )
		{
			float4 tex_1 = g_tColor0.Sample( g_sSampler0, float2(0.0, 1.0)); // ph for  posToTexCoord( vertexPosInView.xyz, normalInView.xyz )
			float4 tex_2 = g_tColor1.Sample( g_sSampler1, i.vTextureCoords.xy );
			float4 tex_3 = g_tColor2.Sample( g_sSampler2, i.vTextureCoords.zw );
			float4 tex_4 = g_tColor3.Sample( g_sSampler3, float2(0.0, 1.0) ); //ph for vtexcoord2
			float4 tex_5 = g_tColor4.Sample( g_sSampler4, float2(0.0, 1.0) ); //ph for vtexcoord2

			float4 tex_6 = g_tColor5.Sample( g_sSampler5, i.vTextureCoords.xy );
			float4 tex_7 = g_tColor6.Sample( g_sSampler6, i.vTextureCoords.zw );
			float4 tex_8 = g_tColor7.Sample( g_sSampler7, float2(0.0, 1.0) ); //ph for vtexcoord2
			float4 tex_9 = g_tColor8.Sample( g_sSampler8, float2(0.0, 1.0) ); //ph for vtexcoord2

			// TODO: Colors
			// float secondColorSum = dot( vColorSecond.bgr, float3( 1.0, 1.0, 1.0 ) );
			// float4 weights = float4( vColorSecond.bgr, 1.0 - saturate( secondColorSum ) );
			float4 vColorSecond = float4(0.5, 0.5, 0.5, 0.5);
			float4 weights = float4(0.0, 0.0, 0.0, 0.0);

			// Heights from alpha channels
			float4 heights = max( float4( tex_6.a, tex_7.a, tex_8.a, tex_9.a ), 0.004 );
			float4 alphaVec = weights * heights;

			float weightsMax = max( alphaVec.r, max( alphaVec.g, max( alphaVec.b, alphaVec.a ) ) );
			float4 alphaVec2 = (1.0 - saturate( float4( weightsMax, weightsMax, weightsMax, weightsMax ) - alphaVec ));
			alphaVec2 *= alphaVec;

			// Normalize alpha vector
			float normFactor = 1.0 / dot( alphaVec2, float4( 1.0, 1.0, 1.0, 1.0 ) );
			float4 alphaVec2Normalized = alphaVec2 * normFactor;

			// Blend textures
			float4 texMixed =
				tex_2 * alphaVec2Normalized.r +
				tex_3 * alphaVec2Normalized.g +
				tex_4 * alphaVec2Normalized.b +
				tex_5 * alphaVec2Normalized.a;

			// Emissive color
			//emissive = (texMixed.w * tex_1.rgb) * texMixed.rgb;

			// Ambient occlusion placeholder
			float3 ambientOcclusionColor = float3( 0.0, 0.0, 0.0 );
			m.Albedo = lerp( texMixed.rgb, ambientOcclusionColor, vColorSecond.a );

			m.Opacity = 1;
		}

		m.Normal = i.vNormalWs;
		m.TextureCoords = i.vTextureCoords.xy;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Transmission = 0;
		return ShadingModelStandard::Shade( i, m );
	}
}
