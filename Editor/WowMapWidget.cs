using Editor.Inspectors;
using Sandbox;
using Sandbox.Internal;
using System.IO;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Structs.ADT;
using WoWFormatLib.Structs.M2;
using WoWFormatLib.Structs.WMO;
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

		itemWidget.AddItem( new MapData() { FileDataID = 775971, Name = "Azeroth"} );
		itemWidget.AddItem( new MapData() { FileDataID = 782779, Name = "Kalimdor" } );
		itemWidget.AddItem( new MapData() { FileDataID = 1522385, Name = "Winter AB" } );

		itemWidget.ItemSelected += ( item ) =>
		{
			var selectedItem = item as MapData;
			Log.Info( "Selected item: " + selectedItem.FileDataID + " (" + selectedItem.Name + ")");
			this.LoadMap( selectedItem.FileDataID);
		};

		Layout.Add( itemWidget );
	}

	public void SpawnModel(string modelName, Vector3 position, Angles rotation, Vector3 scale, GameObject parent )
	{
		var model = Model.Load(modelName);

		if (model == null)
		{
			Log.Error($"Model {modelName} could not be loaded.");
			return;
		}

		var go = new GameObject(Path.GetFileNameWithoutExtension(modelName));
		var modelRenderer = go.Components.Create<ModelRenderer>();
		modelRenderer.Model = model;

		go.LocalPosition = position;
		go.LocalRotation = rotation;
		go.LocalScale = scale;

		go.Parent = parent;
		go.Enabled = true;

		//Log.Info($"Spawned model {modelName} at {position} with rotation {rotation}.");
	}

	public GameObject SpawnADT(byte x, byte y, ADT adt, uint mapTextureFDID, GameObject parent)
	{
		var model = LoadADTMesh(adt, mapTextureFDID);
		var go = new GameObject("ADT_" + x + "_" + y);
		var modelRenderer = go.Components.Create<ModelRenderer>();
		modelRenderer.Model = model;

		go.Parent = parent;
		go.Enabled = true;

		return go;
	}

	public void LoadMap(uint fileDataID )
	{
		var loadModels = true;

		if (!wowMount.FileExistsByID(fileDataID))
		{
			Log.Error($"WDT file data ID {fileDataID} not found in WoW mount.");
			return;
		}
		var wdtReader = new WDTReader(wowMount);
		wdtReader.LoadWDT(fileDataID);

		var adtReader = new ADTReader(wowMount, wdtReader.wdtfile);

		SceneEditorSession.Active.Scene.Push();

		var parentGO = new GameObject( "WoWMap" );
		parentGO.Parent = SceneEditorSession.Active.Scene;
		parentGO.Enabled = true;

		var modelDict = new Dictionary<uint, Model>();

		foreach (var tile in wdtReader.wdtfile.tileFiles)
		{
			if ( tile.Value.rootADT == 0 )
				continue;

			Log.Info($"Processing tile at ({tile.Key.Item1}, {tile.Key.Item2}) with root ADT ID {tile.Value.rootADT}");

			adtReader.LoadADT(
				tile.Value.rootADT,
				tile.Value.obj0ADT,
				tile.Value.tex0ADT
			);

			var adtGO = SpawnADT(tile.Key.Item1, tile.Key.Item2, adtReader.adtfile, tile.Value.mapTexture, parentGO);

			if ( !loadModels )
				continue;

			foreach (var wmo in adtReader.adtfile.objects.worldModels.entries )
			{
				SpawnModel(
					wowMount.GetMountNameByID(wmo.mwidEntry),
					(new Vector3((wmo.position.z - 17066 ) * -1, (wmo.position.x - 17066) * -1, wmo.position.y) * SBoxScale),
					new Angles(wmo.rotation.x, wmo.rotation.y - 180f, wmo.rotation.z),
					new Vector3(wmo.scale / 1024f, wmo.scale / 1024f, wmo.scale / 1024f),
					adtGO
				);
			}

			foreach ( var m2 in adtReader.adtfile.objects.models.entries )
			{
				SpawnModel(
					wowMount.GetMountNameByID(m2.mmidEntry),
					(new Vector3((m2.position.z - 17066) * -1, ( m2.position.x - 17066) * -1, m2.position.y) * SBoxScale),
					new Angles(m2.rotation.x, m2.rotation.y - 180f, m2.rotation.z),
					new Vector3(m2.scale / 1024f, m2.scale / 1024f, m2.scale / 1024f ),
					adtGO
				);
			}
		}
	}


	// TODO: We should use a proper resource loader for this
	private Model LoadADTMesh(ADT adt, uint mapTextureFDID)
	{
		var TileSize = 1600.0f / 3.0f; //533.333
		var ChunkSize = TileSize / 16.0f; //33.333
		var UnitSize = ChunkSize / 8.0f; //4.166666
		var MapMidPoint = 32.0f / ChunkSize;

		Material material = Material.Create( "ADT", "simple_color" );
		material.Set( "Color", wowMount.LoadTexture( mapTextureFDID ) );
		var firstChunk = adt.chunks[0].header;
		var firstChunkX = firstChunk.position.x;
		var firstChunkY = firstChunk.position.y;
		var meshList = new List<Mesh>();
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
					var vx = chunk.header.position.y - (j * UnitSize);
					var vz = chunk.header.position.x - (i * (UnitSize / 2));

					var v = new SimpleVertex
					{
						// TODO: MCCV
						//if (chunk.vertexShading.red != null)
						//    v.Color = new Vector4(chunk.vertexShading.blue[idx] / 255.0f, chunk.vertexShading.green[idx] / 255.0f, chunk.vertexShading.red[idx] / 255.0f, chunk.vertexShading.alpha[idx] / 255.0f);
						//else
						//    v.Color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

						normal = new Vector3( chunk.normals.normal_0[idx], chunk.normals.normal_1[idx], chunk.normals.normal_2[idx] ),
						//texcoord = new Vector2( (j + (((i % 2) != 0) ? 0.5f : 0f)) / 8f, (i * 0.5f) / 8f ),

						texcoord = new Vector2( -(vx - firstChunkX) / TileSize, -( vz - firstChunkY) / TileSize),
						position = new Vector3( chunk.header.position.x - (i * UnitSize * 0.5f), chunk.header.position.y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.z ),
					};

					if ( (i % 2) != 0 )
						v.position.y -= 0.5f * UnitSize;

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

			mesh.CreateVertexBuffer( verticelist.Count, SimpleVertex.Layout, verticelist );

			var indiceArr = indicelist.ToArray();
			mesh.CreateIndexBuffer( indiceArr.Length, indiceArr );
			mesh.Bounds = BBox.FromPoints( verticelist.Select( ( SimpleVertex x ) => x.position ), 0f );
			meshList.Add( mesh );
		}

		return Model.Builder.WithName( "WoWMap" ).AddMeshes( [.. meshList] ).Create();
	}
}
