using System;
using System.IO;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Structs.ADT;
using WoWSBoxMount;

[Dock( "Editor", "WoW Mount Dock", "local_fire_department" )]
public class WowMapWidget : Widget
{
	public record MapData
	{
		public string Name { get; set; }
		public uint FileDataID { get; set; }

		public override string ToString()
		{
			return $"{Name}";
		}
	}

	private WowMount wowMount;
	private float SBoxScale = 40f;
	private Texture emptyTexture;

	public WowMapWidget( Widget parent ) : base( parent, false )
	{
		wowMount = (WowMount)Sandbox.Mounting.Directory.Get( "wow" );
		if ( wowMount == null )
		{
			Log.Error( "WoW mount not found" );
			return;
		}

		if ( !wowMount.IsMounted )
			Sandbox.Mounting.Directory.Mount( "wow" );

		// Create a Column Layout
		Layout = Layout.Column();

		var itemWidget = new TreeView( this );

		itemWidget.AddItem( new MapData() { FileDataID = 775971, Name = "Azeroth" } );
		itemWidget.AddItem( new MapData() { FileDataID = 782779, Name = "Kalimdor" } );
		itemWidget.AddItem( new MapData() { FileDataID = 1522385, Name = "Winter AB" } );
		itemWidget.AddItem( new MapData() { FileDataID = 5105580, Name = "Dragon Isles" } );
		itemWidget.AddItem( new MapData() { FileDataID = 5665807, Name = "K'aresh" } );

		itemWidget.ItemSelected += ( item ) =>
		{
			var selectedItem = item as MapData;
			Log.Info( "Selected item: " + selectedItem.FileDataID + " (" + selectedItem.Name + ")" );
			this.LoadMap( selectedItem.FileDataID );
		};

		Layout.Add( itemWidget );
	}

	public void SpawnModel( string modelName, Vector3 position, Angles rotation, Vector3 scale, GameObject parent )
	{
		var model = Model.Load( modelName );

		if ( model == null )
		{
			Log.Error( $"Model {modelName} could not be loaded." );
			return;
		}

		var go = new GameObject( Path.GetFileNameWithoutExtension( modelName ) );
		var modelRenderer = go.Components.Create<ModelRenderer>();
		modelRenderer.Model = model;

		go.LocalPosition = position;
		go.LocalRotation = rotation;
		go.LocalScale = scale;

		go.Parent = parent;
		go.Enabled = true;

		//Log.Info($"Spawned model {modelName} at {position} with rotation {rotation}.");
	}

	public GameObject SpawnADT( byte x, byte y, ADT adt, uint mapTextureFDID, GameObject parent )
	{
		var model = LoadADTMesh( adt, mapTextureFDID );
		var go = new GameObject( "ADT_" + x + "_" + y );
		var modelRenderer = go.Components.Create<ModelRenderer>();
		modelRenderer.Model = model;

		go.Parent = parent;
		go.Enabled = true;

		return go;
	}

	public void LoadMap( uint fileDataID )
	{
		var loadModels = true;

		if ( !wowMount.FileExistsByID( fileDataID ) )
		{
			Log.Error( $"WDT file data ID {fileDataID} not found in WoW mount." );
			return;
		}
		var wdtReader = new WDTReader( wowMount );
		wdtReader.LoadWDT( fileDataID );

		var adtReader = new ADTReader( wowMount, wdtReader.wdtfile );

		emptyTexture = Texture.Create( 2, 2, ImageFormat.A8 ).Finish();

		SceneEditorSession.Active.Scene.Push();

		var parentGO = new GameObject( "WoWMap" );
		parentGO.Parent = SceneEditorSession.Active.Scene;
		parentGO.Enabled = true;

		var modelDict = new Dictionary<uint, Model>();

		foreach ( var tile in wdtReader.wdtfile.tileFiles )
		{
			if ( tile.Value.rootADT == 0 )
				continue;

			if ( tile.Key.Item1 < 32 || tile.Key.Item1 > 34 )
				continue;

			if ( tile.Key.Item2 < 30 || tile.Key.Item2 > 32 )
				continue;

			Log.Info( $"Processing tile at ({tile.Key.Item1}, {tile.Key.Item2}) with root ADT ID {tile.Value.rootADT}" );

			adtReader.LoadADT(
				tile.Value.rootADT,
				tile.Value.obj0ADT,
				tile.Value.tex0ADT
			);

			var adtGO = SpawnADT( tile.Key.Item1, tile.Key.Item2, adtReader.adtfile, tile.Value.mapTexture, parentGO );

			if ( !loadModels )
				continue;

			foreach ( var wmo in adtReader.adtfile.objects.worldModels.entries )
			{
				SpawnModel(
					wowMount.GetMountNameByID( wmo.mwidEntry ),
					(new Vector3( (wmo.position.z - 17066) * -1, (wmo.position.x - 17066) * -1, wmo.position.y ) * SBoxScale),
					new Angles( wmo.rotation.x, wmo.rotation.y - 180f, wmo.rotation.z ),
					new Vector3( wmo.scale / 1024f, wmo.scale / 1024f, wmo.scale / 1024f ),
					adtGO
				);
			}

			foreach ( var m2 in adtReader.adtfile.objects.models.entries )
			{
				SpawnModel(
					wowMount.GetMountNameByID( m2.mmidEntry ),
					(new Vector3( (m2.position.z - 17066) * -1, (m2.position.x - 17066) * -1, m2.position.y ) * SBoxScale),
					new Angles( m2.rotation.x, m2.rotation.y - 180f, m2.rotation.z ),
					new Vector3( m2.scale / 1024f, m2.scale / 1024f, m2.scale / 1024f ),
					adtGO
				);
			}
		}
	}

	// TODO: We should use a proper resource loader for this
	private Model LoadADTMesh( ADT adt, uint mapTextureFDID )
	{
		var useBakedTextures = false;

		var TileSize = 1600.0f / 3.0f; //533.333
		var ChunkSize = TileSize / 16.0f; //33.333
		var UnitSize = ChunkSize / 8.0f; //4.166666
		var MapMidPoint = 32.0f / ChunkSize;
	

		var firstChunk = adt.chunks[0].header;
		var firstChunkX = firstChunk.position.x;
		var firstChunkY = firstChunk.position.y;
		var meshList = new List<Mesh>();
		for ( uint c = 0; c < 256; c++ )
		{
			Material material;

			if ( useBakedTextures )
			{
				material = Material.Create( "ADT", "simple_color" );
				material.Set( "Color0", wowMount.LoadTexture( mapTextureFDID ) );
			}
			else
			{
				material = Material.Create( "ADT", "wow_terrain" );
				for(int i = 0; i < 4; i++)
				{
					material.Set( $"Layer{i}",  emptyTexture );
					material.Set( $"Height{i}", emptyTexture );

					if ( i != 0 )
						material.Set( $"Blend{i - 1}", emptyTexture );

					material.Set( $"layerScale{i}", 1.0f );
				}
			}

			var verticelist = new List<ADTVertex>();
			var indicelist = new List<int>();

			var chunk = adt.chunks[c];

			if ( !useBakedTextures )
			{
				var heightScales = new List<float>();
				var heightOffsets = new List<float>();
				for ( int i = 0; i < 4; i++ )
				{
					if ( i > chunk.layers.Length - 1 )
					{
						heightScales.Add( 0.0f );
						heightOffsets.Add( 1.0f );
					}
					else
					{
						var ti = chunk.layers[i].textureId;
						heightScales.Add( adt.texParams[ti].height );
						heightOffsets.Add( adt.texParams[ti].offset );
					}
				}

				material.Set( "pc_heightScale", new Vector4( heightScales[0], heightScales[1], heightScales[2], heightScales[3] ) );
				material.Set( "pc_heightOffset", new Vector4( heightOffsets[0], heightOffsets[1], heightOffsets[2], heightOffsets[3] ) );

				//Log.Info( $"Setting height scales: {heightScales[0]}, {heightScales[1]}, {heightScales[2]}, {heightScales[3]}" );
				//Log.Info( $"Setting height offsets: {heightOffsets[0]}, {heightOffsets[1]}, {heightOffsets[2]}, {heightOffsets[3]}" );

				for ( int i = 0; i < chunk.layers.Length; i++ )
				{
					var ti = chunk.layers[i].textureId;

					var diffuseTexture = wowMount.LoadTexture( adt.diffuseTextureFileDataIDs[ti] );
					material.Set( $"Layer{i}", diffuseTexture );

					var heightTexture = wowMount.LoadTexture( adt.heightTextureFileDataIDs[ti] == 0 ? adt.diffuseTextureFileDataIDs[ti] : adt.heightTextureFileDataIDs[ti] );
					material.Set( $"Height{i}", heightTexture );

					if ( i != 0 )
					{
						var blendTexture = Texture.Create( 64, 64, ImageFormat.A8 ).WithData( chunk.alphaLayer[i].layer ).Finish();
						material.Set( $"Blend{i - 1}", blendTexture );
					}

					material.Set( $"layerScale{i}", (float)Math.Pow( 2, (adt.texParams[ti].flags & 0xF0) >> 4 ) );
				}
			}

			// var off = verticelist.Count();
			var off = 0;
			for ( int i = 0, idx = 0; i < 17; i++ )
			{
				for ( var j = 0; j < (((i % 2) != 0) ? 8 : 9); j++ )
				{
					var vx = chunk.header.position.y - (j * UnitSize);
					var vz = chunk.header.position.x - (i * (UnitSize / 2));

					var v = new ADTVertex
					{
						color = (chunk.vertexShading.red != null
							? new Vector4( chunk.vertexShading.blue[idx] / 255.0f, chunk.vertexShading.green[idx] / 255.0f, chunk.vertexShading.red[idx] / 255.0f, chunk.vertexShading.alpha[idx] / 255.0f )
							: new Vector4( 0.5f, 0.5f, 0.5f, 1.0f )
						),
						normal = new Vector3( chunk.normals.normal_0[idx], chunk.normals.normal_1[idx], chunk.normals.normal_2[idx] ),
						texcoord = (useBakedTextures ? new Vector2( -(vx - firstChunkX) / TileSize, -(vz - firstChunkY) / TileSize ) : new Vector2( (j + (((i % 2) != 0) ? 0.5f : 0f)) / 8f, (i * 0.5f) / 8f )),
						position = new Vector3( chunk.header.position.x - (i * 2.08333125f), chunk.header.position.y - (j * 4.1666625f), chunk.vertices.vertices[idx++] + chunk.header.position.z ),
					};

					if ( (i % 2) != 0 )
						v.position.y -= 2.08333125f;

					v.position *= SBoxScale;

					verticelist.Add( v );
				}
			}

			for ( var j = 9; j < 145; j++ )
			{
				indicelist.AddRange( [off + j - 9, off + j + 8, off + j] );
				indicelist.AddRange( [off + j - 8, off + j - 9, off + j] );
				indicelist.AddRange( [off + j + 9, off + j - 8, off + j] );
				indicelist.AddRange( [off + j + 8, off + j + 9, off + j] );
				if ( (j + 1) % (9 + 8) == 0 ) j += 9;
			}

			var mesh = new Mesh( material );

			mesh.CreateVertexBuffer( verticelist.Count, ADTVertex.Layout, verticelist );

			var indiceArr = indicelist.ToArray();
			mesh.CreateIndexBuffer( indiceArr.Length, indiceArr );
			mesh.Bounds = BBox.FromPoints( verticelist.Select( ( ADTVertex x ) => x.position ), 0f );
			meshList.Add( mesh );
		}

		return Model.Builder.WithName( "WoWMap" ).AddMeshes( [.. meshList] ).Create();
	}
}
