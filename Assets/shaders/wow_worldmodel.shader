
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
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( Anisotropic ); AddressU( WRAP ); AddressV( WRAP ); >;

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
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 19 )
		{
						// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
		}
		else if ( WMOPixelShader == 20 )
		{
			// TODO 
			m.Albedo = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).rgb;
			m.Opacity = g_tColor0.Sample(g_sSampler0, i.vTextureCoords.xy ).a;
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
