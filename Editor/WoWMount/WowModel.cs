using BLPSharp;
using Sandbox.Mounting;
using WoWFormatLib.FileReaders;

namespace WoWSBoxMount
{
	internal class WowModel : ResourceLoader<WowMount>
	{
		public uint FileDataID { get; set; }
		public string BaseName { get; set; }

		protected override object Load()
		{
			object model;

			Log.Info( "Loading WoW M2 " + FileDataID + "..." );

			var m2Reader = new M2Reader( base.Host );
			using ( var fs = base.Host.GetFileByID( FileDataID ) )
			{
				m2Reader.LoadM2( fs );
			}

			var m2 = m2Reader.model;
			var verticeList = new List<M2Vertex>();
			var vectorList = new List<Vector3>();
			foreach ( var vertice in m2.vertices )
			{
				var position = new Vector3( vertice.position.x, vertice.position.y, vertice.position.z );
				vectorList.Add( position );
				var normal = new Vector3( vertice.normal.x, vertice.normal.y, vertice.normal.z );
				var texCoord = new Vector2( vertice.textureCoordX, vertice.textureCoordY );

				var blendWeights = new Vector4(vertice.boneWeight_0, vertice.boneWeight_1, vertice.boneWeight_2, vertice.boneWeight_3);
				var blendIndices = new Vector4(vertice.boneIndices_0, vertice.boneIndices_1, vertice.boneIndices_2, vertice.boneIndices_3);

				verticeList.Add( new M2Vertex(
					position * 30f,
					normal,
					texCoord,
					blendIndices,
					blendWeights
				) );
			}

			var indiceList = new List<int>();
			foreach ( var indice in m2.skins[0].triangles )
			{
				indiceList.Add( indice.pt1 );
				indiceList.Add( indice.pt2 );
				indiceList.Add( indice.pt3 );
			}

			var meshList = new List<Mesh>();
			for ( var submeshIndex = 0; submeshIndex < m2.skins[0].submeshes.Length; submeshIndex++ )
			{
				var submesh = m2.skins[0].submeshes[submeshIndex];
				Log.Info( $"Submesh ID: {submesh.submeshID}, Vertices: {submesh.nVertices}, Triangles: {submesh.nTriangles}" );

				Material material = null;

				for ( var tu = 0; tu < m2.skins[0].textureunit.Length; tu++ )
				{
					if ( m2.skins[0].textureunit[tu].submeshIndex == submeshIndex )
					{
						var textureFileDataID = m2.textureFileDataIDs[m2.texlookup[m2.skins[0].textureunit[tu].texture].textureID];

						Log.Info( $"Texture Unit {tu}: Flags: {m2.skins[0].textureunit[tu].flags}, Shading: {m2.skins[0].textureunit[tu].shading}, Submesh Index: {m2.skins[0].textureunit[tu].submeshIndex}, Texture File Data ID: {textureFileDataID}" );

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
					}
				}

				var mesh = new Mesh( material );

				mesh.CreateVertexBuffer( verticeList.Count, M2Vertex.Layout, verticeList );

				var indiceArr = indiceList.ToArray();
				mesh.CreateIndexBuffer( indiceArr.Length, indiceArr );
				mesh.SetIndexRange( (int)submesh.startTriangle, (int)submesh.nTriangles );
				mesh.Bounds = BBox.FromPoints( verticeList.Select( ( M2Vertex x ) => x.position ), 0f );
				meshList.Add( mesh );
			}

			model = Model.Builder.WithName( BaseName ).AddMeshes( [.. meshList] ).Create();
			Log.Info( "WoW model loaded successfully." );
			return model;
		}
	}
}
