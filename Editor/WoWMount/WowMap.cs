using Sandbox;
using Sandbox.Mounting;
using System.Formats.Asn1;
using WoWFormatLib.FileReaders;

namespace WoWSBoxMount
{
	internal class WowMap : ResourceLoader<WowMount>
	{
		public uint WDTFileDataID { get; set; }

		protected override object Load()
		{
			Log.Info( "Loading WoW WDT " + WDTFileDataID + "..." );

			var TileSize = 1600.0f / 3.0f; //533.333
			var ChunkSize = TileSize / 16.0f; //33.333
			var UnitSize = ChunkSize / 8.0f; //4.166666
			var MapMidPoint = 32.0f / ChunkSize;

			var meshList = new List<Mesh>();

			var wdtReader = new WDTReader( base.Host );
			wdtReader.LoadWDT( WDTFileDataID );
			Material material = Material.Create( "ADT", "simple_color" );
			Sandbox.Texture texture = Sandbox.Texture.White;
			material.Set( "Color", texture );

			var adtReader = new ADTReader(  base.Host, wdtReader.wdtfile );

			foreach ( var tile in wdtReader.wdtfile.tileFiles )
			{
				if ( tile.Key.Item1 == 32 && tile.Key.Item2 == 32 )
				{

				}
				else
				{
					continue;
				}

				Log.Info( $"Processing tile at ({tile.Key.Item1}, {tile.Key.Item2})" );

				var adtFileDataID = tile.Value.rootADT;
				if ( adtFileDataID == 0 )
					continue;

				var maptextureFileDataID = tile.Value.mapTexture;

				adtReader.LoadADT( tile.Value.rootADT, tile.Value.obj0ADT, tile.Value.tex0ADT );
				var adt = adtReader.adtfile;

				for ( uint c = 0; c < 256; c++ )
				{
					var verticelist = new List<SimpleVertex>();
					var indicelist = new List<int>();

					var chunk = adt.chunks[c];

					// var off = verticelist.Count();
					var off = 0;
					for ( int i = 0, idx = 0; i < 17; i++ )
					{
						for ( var j = 0; j < (((i % 2) != 0) ? 8 : 9); j++ )
						{
							var v = new SimpleVertex
							{
								// TODO: MCCV
								//if (chunk.vertexShading.red != null)
								//    v.Color = new Vector4(chunk.vertexShading.blue[idx] / 255.0f, chunk.vertexShading.green[idx] / 255.0f, chunk.vertexShading.red[idx] / 255.0f, chunk.vertexShading.alpha[idx] / 255.0f);
								//else
								//    v.Color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

								normal = new Vector3( chunk.normals.normal_0[idx], chunk.normals.normal_1[idx], chunk.normals.normal_2[idx] ),
								texcoord = new Vector2( (j + (((i % 2) != 0) ? 0.5f : 0f)) / 8f, (i * 0.5f) / 8f ),
								position = new Vector3( chunk.header.position.x - (i * UnitSize * 0.5f), chunk.header.position.y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.z ),
							};

							if ( (i % 2) != 0 )
								v.position.y -= 0.5f * UnitSize;

							verticelist.Add( v );
						}
					}

					for ( var j = 9; j < 145; j++ )
					{
						indicelist.AddRange( [off + j + 8, off + j - 9, off + j] );
						indicelist.AddRange( [off + j - 9, off + j - 8, off + j] );
						indicelist.AddRange( [off + j - 8, off + j + 9, off + j] );
						indicelist.AddRange( [off + j + 9, off + j + 8, off + j] );
						if ( (j + 1) % (9 + 8) == 0 ) j += 9;
					}

					var mesh = new Mesh( material );

					mesh.CreateVertexBuffer( verticelist.Count, SimpleVertex.Layout, verticelist );

					var indiceArr = indicelist.ToArray();
					mesh.CreateIndexBuffer( indiceArr.Length, indiceArr );
					mesh.Bounds = BBox.FromPoints( verticelist.Select( ( SimpleVertex x ) => x.position ), 0f );
					meshList.Add( mesh );
				}
			}

			return Model.Builder.WithName( "WoWMap" ).AddMeshes( [.. meshList] ).Create();
		}
	}
}
