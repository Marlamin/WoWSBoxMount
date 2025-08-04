using Sandbox.Mounting;
using System;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Structs.WMO;

namespace WoWSBoxMount
{
	internal class WowWMO : ResourceLoader<WowMount>
	{
		public uint FileDataID { get; set; }
		public string BaseName { get; set; }
		private enum WMOVertexShader : int
		{
			None = -1,
			MapObjDiffuse_T1 = 0,
			MapObjDiffuse_T1_Refl = 1,
			MapObjDiffuse_T1_Env_T2 = 2,
			MapObjSpecular_T1 = 3,
			MapObjDiffuse_Comp = 4,
			MapObjDiffuse_Comp_Refl = 5,
			MapObjDiffuse_Comp_Terrain = 6,
			MapObjDiffuse_CompAlpha = 7,
			MapObjParallax = 8
		}

		private enum WMOPixelShader : int
		{
			None = -1,
			MapObjDiffuse = 0,
			MapObjSpecular = 1,
			MapObjMetal = 2,
			MapObjEnv = 3,
			MapObjOpaque = 4,
			MapObjEnvMetal = 5,
			MapObjTwoLayerDiffuse = 6,
			MapObjTwoLayerEnvMetal = 7,
			MapObjTwoLayerTerrain = 8,
			MapObjDiffuseEmissive = 9,
			MapObjMaskedEnvMetal = 10,
			MapObjEnvMetalEmissive = 11,
			MapObjTwoLayerDiffuseOpaque = 12,
			MapObjTwoLayerDiffuseEmissive = 13,
			MapObjAdditiveMaskedEnvMetal = 14,
			MapObjTwoLayerDiffuseMod2x = 15,
			MapObjTwoLayerDiffuseMod2xNA = 16,
			MapObjTwoLayerDiffuseAlpha = 17,
			MapObjLod = 18,
			MapObjParallax = 19,
			MapObjDFShader = 20
		}

		private readonly Dictionary<MOMTShader, (WMOVertexShader VertexShader, WMOPixelShader PixelShader)> WMOShaderMap = new()
		{
			{ MOMTShader.Diffuse, ( WMOVertexShader.MapObjDiffuse_T1, WMOPixelShader.MapObjDiffuse ) },							// 0: 0, 0
			{ MOMTShader.Specular, ( WMOVertexShader.MapObjSpecular_T1, WMOPixelShader.MapObjSpecular) },						// 1: 3, 1
			{ MOMTShader.Metal, ( WMOVertexShader.MapObjSpecular_T1, WMOPixelShader.MapObjMetal) },								// 2: 3, 2
			{ MOMTShader.Env, ( WMOVertexShader.MapObjDiffuse_T1_Refl, WMOPixelShader.MapObjEnv) },								// 3: 1, 3
			{ MOMTShader.Opaque, (WMOVertexShader.MapObjDiffuse_T1, WMOPixelShader.MapObjOpaque) },								// 4: 0, 4
			{ MOMTShader.EnvMetal, (WMOVertexShader.MapObjDiffuse_T1_Refl, WMOPixelShader.MapObjEnvMetal) },					// 5: 1, 5
			{ MOMTShader.TwoLayerDiffuse, (WMOVertexShader.MapObjDiffuse_Comp, WMOPixelShader.MapObjTwoLayerDiffuse) },			// 6: 4, 6
			{ MOMTShader.TwoLayerEnvMetal, (WMOVertexShader.MapObjDiffuse_T1, WMOPixelShader.MapObjTwoLayerEnvMetal) },			// 7: 0, 7
			{ MOMTShader.TwoLayerTerrain, (WMOVertexShader.MapObjDiffuse_Comp_Terrain, WMOPixelShader.MapObjTwoLayerTerrain) }, // 8: 6, 8
			{ MOMTShader.DiffuseEmissive, (WMOVertexShader.MapObjDiffuse_Comp, WMOPixelShader.MapObjDiffuseEmissive) },			// 9: 4, 9
			{ MOMTShader.WaterWindow, (WMOVertexShader.None, WMOPixelShader.None) },											// 10: -1, -1: SMOMaterial::SH_WATERWINDOW -- automatically generates MOTA
			{ MOMTShader.MaskedEnvMetal, (WMOVertexShader.MapObjDiffuse_T1_Env_T2, WMOPixelShader.MapObjMaskedEnvMetal) },		// 11: 2, 10
			{ MOMTShader.EnvMetalEmissive, (WMOVertexShader.MapObjDiffuse_T1_Env_T2, WMOPixelShader.MapObjEnvMetalEmissive) },  // 12: 2, 11
			{ MOMTShader.TwoLayerDiffuseOpaque, (WMOVertexShader.MapObjDiffuse_Comp, WMOPixelShader.MapObjTwoLayerDiffuseOpaque) }, // 13: 4, 12
			{ MOMTShader.SubmarineWindow, (WMOVertexShader.None, WMOPixelShader.None) },										// 14: -1, -1: SMOMaterial::SH_SUBMARINEWINDOW -- automatically generates MOTA
			{ MOMTShader.TwoLayerDiffuseEmissive, (WMOVertexShader.MapObjDiffuse_Comp, WMOPixelShader.MapObjTwoLayerDiffuseEmissive) }, // 15: 4, 13
			{ MOMTShader.DiffuseTerrain, (WMOVertexShader.MapObjDiffuse_T1, WMOPixelShader.MapObjDiffuse) },					// 16: 0, 0: SMOMaterial::SH_DIFFUSE_TERRAIN -- "Blend Material": used for blending WMO with terrain (dynamic blend batches)
			{ MOMTShader.AdditiveMaskedEnvMetal, (WMOVertexShader.MapObjDiffuse_T1_Env_T2, WMOPixelShader.MapObjAdditiveMaskedEnvMetal) }, // 17: 2, 14
			{ MOMTShader.TwoLayerDiffuseMod2x, (WMOVertexShader.MapObjDiffuse_CompAlpha, WMOPixelShader.MapObjTwoLayerDiffuseMod2x) }, // 18: 7, 15
			{ MOMTShader.TwoLayerDiffuseMod2xNA, (WMOVertexShader.MapObjDiffuse_Comp, WMOPixelShader.MapObjTwoLayerDiffuseMod2xNA) }, // 19: 4, 16
			{ MOMTShader.TwoLayerDiffuseAlpha, (WMOVertexShader.MapObjDiffuse_CompAlpha, WMOPixelShader.MapObjTwoLayerDiffuseAlpha) }, // 20: 7, 17
			{ MOMTShader.Lod, (WMOVertexShader.MapObjDiffuse_T1, WMOPixelShader.MapObjLod) },									// 21: 0, 18
			{ MOMTShader.Parallax, ( WMOVertexShader.MapObjParallax, WMOPixelShader.MapObjParallax ) },							// 22: 8, 19
			{ MOMTShader.DF_MoreTexture_Unknown, ( WMOVertexShader.MapObjDiffuse_T1, WMOPixelShader.MapObjDFShader ) },			// 23: 0, 20

		};
		protected override object Load()
		{
			Log.Info( "Loading WoW WMO " + BaseName + "..." );
			var wmoReader = new WMOReader( base.Host );
			WoWFormatLib.Structs.WMO.WMO wmo;

			using ( var fs = base.Host.GetFileByID( FileDataID ) )
			{
				wmo = wmoReader.LoadWMO( fs );
			}

			var materials = new Dictionary<int, List<uint>>();
			for ( var i = 0; i < wmo.materials.Length; i++ )
			{
				var textureList = new List<uint>
				{
					wmo.materials[i].texture1,
					wmo.materials[i].texture2,
					wmo.materials[i].texture3
				};

				if ( WMOShaderMap[wmo.materials[i].shader].PixelShader == WMOPixelShader.MapObjParallax )
				{
					textureList.Add( wmo.materials[i].color2 );
					textureList.Add( wmo.materials[i].flags3 );
					textureList.Add( wmo.materials[i].runtimeData0 );
				}
				else if ( WMOShaderMap[wmo.materials[i].shader].PixelShader == WMOPixelShader.MapObjDFShader )
				{
					textureList.Add( wmo.materials[i].color3 );
					textureList.Add( wmo.materials[i].runtimeData0 );
					textureList.Add( wmo.materials[i].runtimeData1 );
					textureList.Add( wmo.materials[i].runtimeData2 );
					textureList.Add( wmo.materials[i].runtimeData3 );
				}

				materials.Add( i, textureList );
			}

			var meshList = new List<Mesh>();
			for ( var g = 0; g < wmo.group.Length; g++ )
			{
				var group = wmo.group[g];

				string groupName = null;

				for ( var i = 0; i < wmo.groupNames.Length; i++ )
					if ( wmo.group[g].mogp.nameOffset == wmo.groupNames[i].offset )
						groupName = wmo.groupNames[i].name.Replace( " ", "_" );

				if ( groupName == "antiportal" )
				{
					Console.WriteLine( "Skipping group " + groupName + " because it is an antiportal" );
					continue;
				}

				if ( group.mogp.renderBatches == null )
				{
					Console.WriteLine( "Skipping group " + groupName + " because it has no renderbatches" );
					continue;
				}

				if ( group.mogp.vertices == null )
				{
					Console.WriteLine( "Skipping group " + groupName + " because it has no vertices" );
					continue;
				}

				var meshName = groupName;

				var wmovertices = new List<WMOVertex>( wmo.group[g].mogp.vertices.Length );
				var vectorList = new List<Vector3>();
				for ( var i = 0; i < wmo.group[g].mogp.vertices.Length; i++ )
				{
					var wmovertex = new WMOVertex
					{
						position = new Vector3( wmo.group[g].mogp.vertices[i].vector.x, wmo.group[g].mogp.vertices[i].vector.y, wmo.group[g].mogp.vertices[i].vector.z ) * 40f,
						normal = new Vector3( wmo.group[g].mogp.normals[i].normal.x, wmo.group[g].mogp.normals[i].normal.y, wmo.group[g].mogp.normals[i].normal.z )
					};

					if ( wmo.group[g].mogp.textureCoords[0] == null )
						wmovertex.texcoord0 = new Vector2( 0.0f, 0.0f );
					else
						wmovertex.texcoord0 = new Vector2( wmo.group[g].mogp.textureCoords[0][i].X, wmo.group[g].mogp.textureCoords[0][i].Y );

					if ( wmo.group[g].mogp.textureCoords[1] == null )
						wmovertex.texcoord1 = new Vector2( 0.0f, 0.0f );
					else
						wmovertex.texcoord1 = new Vector2( wmo.group[g].mogp.textureCoords[1][i].X, wmo.group[g].mogp.textureCoords[1][i].Y );

					if ( wmo.group[g].mogp.textureCoords[2] == null )
						wmovertex.texcoord2 = new Vector2( 0.0f, 0.0f );
					else
						wmovertex.texcoord2 = new Vector2( wmo.group[g].mogp.textureCoords[2][i].X, wmo.group[g].mogp.textureCoords[2][i].Y );

					if ( wmo.group[g].mogp.textureCoords[3] == null )
						wmovertex.texcoord3 = new Vector2( 0.0f, 0.0f );
					else
						wmovertex.texcoord3 = new Vector2( wmo.group[g].mogp.textureCoords[3][i].X, wmo.group[g].mogp.textureCoords[3][i].Y );

					vectorList.Add( wmovertex.position );
					wmovertices.Add( wmovertex );
				}

				var indiceList = new List<int>();
				for ( var i = 0; i < wmo.group[g].mogp.indices.Length; i++ )
					indiceList.Add( wmo.group[g].mogp.indices[i] );

				var indiceArr = indiceList.ToArray();

				for ( var i = 0; i < group.mogp.renderBatches.Length; i++ )
				{
					int matID = 0;

					if ( group.mogp.renderBatches[i].flags == 2 )
						matID = group.mogp.renderBatches[i].possibleBox2_3;
					else
						matID = group.mogp.renderBatches[i].materialID;

					//wmoBatch.wmoRenderBatch[rb].shader = wmo.materials[matID].shader;

					var material = Material.Create( "wmo_" + FileDataID + "_" + matID, "wow_worldmodel" );
					material.Set( "WMOVertexShader", (int)WMOShaderMap[wmo.materials[matID].shader].VertexShader );
					material.Set( "WMOPixelShader", (int)WMOShaderMap[wmo.materials[matID].shader].PixelShader ); // Do not use "PixelShader", it is reserved.
					material.Set( "WMOBlendMode", (int)wmo.materials[matID].blendMode );

					for ( var j = 0; j < materials[matID].Count; j++ )
						if ( materials[matID][j] != 0 )
							material.Set( "Color" + j, base.Host.LoadTexture( materials[matID][j] ) );

					var mesh = new Mesh( material );

					mesh.CreateVertexBuffer( wmovertices.Count, WMOVertex.Layout, wmovertices );
					mesh.CreateIndexBuffer( indiceArr.Length, indiceArr );
					mesh.SetIndexRange( (int)group.mogp.renderBatches[i].firstFace, (int)group.mogp.renderBatches[i].numFaces );
					mesh.Bounds = BBox.FromPoints( wmovertices.Select( x => x.position ), 0f );

					//wmoBatch.wmoRenderBatch[rb].blendType = wmo.materials[matID].blendMode;
					//wmoBatch.wmoRenderBatch[rb].groupID = (uint)g;

					meshList.Add( mesh );
				}
			}

			var modelBuilder = Model.Builder.WithName( BaseName );
			modelBuilder.AddMeshes( [.. meshList] );
			var model = modelBuilder.Create();
			return model;
		}
	}
}
