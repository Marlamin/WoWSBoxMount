using BLPSharp;
using Sandbox.Mounting;
using System;
using WoWFormatLib.FileReaders;

namespace WoWSBoxMount
{
	internal class WowWMO : ResourceLoader<WowMount>
	{
		public uint FileDataID { get; set; }
		public string BaseName { get; set; }

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

				if ( (int)wmo.materials[i].shader == 23 )
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
					var material = Material.Create( "wmo", "simple_color" );

					int matID = 0;

					if ( group.mogp.renderBatches[i].flags == 2 )
						matID = group.mogp.renderBatches[i].possibleBox2_3;
					else
						matID = group.mogp.renderBatches[i].materialID;

					//wmoBatch.wmoRenderBatch[rb].shader = wmo.materials[matID].shader;

					var textureFileDataID = materials[matID][0];

					material = Material.Create( textureFileDataID.ToString(), "simple_color" );
					Sandbox.Texture texture = null;

					if ( textureFileDataID == 0 )
					{
						texture = Sandbox.Texture.White;
					}
					else
					{
						var blp = new BLPFile( base.Host.GetFileByID( textureFileDataID ) );
						var pixels = blp.GetPixels( 0, out var width, out var height );
						texture = Sandbox.Texture.Create( width, height, ImageFormat.BGRA8888 ).WithData( pixels ).Finish();
					}

					material.Set( "Color", texture );

					var mesh = new Mesh( material );

					mesh.CreateVertexBuffer( wmovertices.Count, WMOVertex.Layout, wmovertices );
					mesh.CreateIndexBuffer( indiceArr.Length, indiceArr );
					mesh.SetIndexRange( (int)group.mogp.renderBatches[i].firstFace, (int)group.mogp.renderBatches[i].numFaces );
					mesh.Bounds = BBox.FromPoints( wmovertices.Select( ( WMOVertex x ) => x.position ), 0f );

					//wmoBatch.wmoRenderBatch[rb].blendType = wmo.materials[matID].blendMode;
					//wmoBatch.wmoRenderBatch[rb].groupID = (uint)g;

					meshList.Add( mesh );
				}
			}

			//var meshList = new List<Mesh>();
			//for (var submeshIndex = 0; submeshIndex < m2.skins[0].submeshes.Length; submeshIndex++)
			//{
			//    var submesh = m2.skins[0].submeshes[submeshIndex];
			//    Log.Info($"Submesh ID: {submesh.submeshID}, Vertices: {submesh.nVertices}, Triangles: {submesh.nTriangles}");

			//    Material material = null;

			//    for (var tu = 0; tu < m2.skins[0].textureunit.Length; tu++)
			//    {
			//        if (m2.skins[0].textureunit[tu].submeshIndex == submeshIndex)
			//        {
			//            var textureFileDataID = m2.textureFileDataIDs[m2.texlookup[m2.skins[0].textureunit[tu].texture].textureID];

			//            Log.Info($"Texture Unit {tu}: Flags: {m2.skins[0].textureunit[tu].flags}, Shading: {m2.skins[0].textureunit[tu].shading}, Submesh Index: {m2.skins[0].textureunit[tu].submeshIndex}, Texture File Data ID: {textureFileDataID}");

			//            material = Material.Create(textureFileDataID.ToString(), "simple_color");
			//            Sandbox.Texture texture = null;

			//            if (textureFileDataID == 0)
			//            {
			//                texture = Sandbox.Texture.White;
			//            }
			//            else
			//            {
			//                var blp = new BLPFile(File.OpenRead(System.IO.Path.Combine(base.Host.BaseDirectory, "files", textureFileDataID.ToString() + ".blp")));
			//                var pixels = blp.GetPixels(0, out var width, out var height);
			//                texture = Sandbox.Texture.Create(width, height, ImageFormat.BGRA8888).WithData(pixels).Finish();
			//            }

			//            material.Set("Color", texture);
			//        }
			//    }

			//    var mesh = new Mesh(material);

			//    mesh.CreateVertexBuffer(verticeList.Count, SimpleVertex.Layout, verticeList);

			//    var indiceArr = indiceList.ToArray();
			//    mesh.CreateIndexBuffer(indiceArr.Length, indiceArr);
			//    mesh.SetIndexRange((int)submesh.startTriangle * 3, (int)submesh.nTriangles * 3);
			//    mesh.Bounds = BBox.FromPoints(verticeList.Select((SimpleVertex x) => x.position), 0f);
			//    meshList.Add(mesh);
			//}

			//model = ;
			//Log.Info("WoW model loaded successfully.");
			return Model.Builder.WithName( BaseName ).AddMeshes( [.. meshList] ).Create();
		}
	}
}
