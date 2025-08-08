using BLPSharp;
using Sandbox.Mounting;
using System;
using System.IO;
using System.Threading.Tasks;
using TACTSharp;

namespace WoWSBoxMount
{
	public class WowMount : BaseGameMount
	{
		public override string Ident => "wow";

		public override string Title => "World of Warcraft";

		public string InstallDirectory;
		public string Product => "wow";

		private BuildInstance buildInstance;

		private Dictionary<uint, Texture> textureCache = new();

		private Listfile listfile;

		protected override void Initialize( InitializeContext context )
		{
			base.Log.Info( "Initializing World of Warcraft mount..." );

			var wowPath = Microsoft.Win32.Registry.GetValue( @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blizzard Entertainment\World of Warcraft", "InstallPath", null ) as string;
			if ( wowPath == null )
			{
				base.Log.Info( "World of Warcraft install path not found in registry." );
				base.IsInstalled = false;
				return;
			}
			else
			{
				if ( wowPath.EndsWith( "_\\" ) )
				{
					var pathInfo = new DirectoryInfo( wowPath );
					wowPath = pathInfo.Parent!.FullName;
				}

				base.IsInstalled = true;
				base.Log.Info( $"World of Warcraft install path found: {wowPath}" );

				InstallDirectory = wowPath;
			}

			base.Log.Info( "World of Warcraft mount initialized." );
		}

		protected override Task Mount( MountContext context )
		{
			base.Log.Info( "Mounting World of Warcraft..." );

			// Load from build.info
			var buildInfoPath = Path.Combine( InstallDirectory, ".build.info" );
			if ( !File.Exists( buildInfoPath ) )
				throw new Exception( "No build.info found in base directory: " + InstallDirectory );

			buildInstance = new BuildInstance();

			buildInstance.Settings.BaseDir = InstallDirectory;
			var buildInfo = new BuildInfo( buildInfoPath, buildInstance.Settings, buildInstance.cdn );

			if ( !buildInfo.Entries.Any( x => x.Product == Product ) )
			{
				base.Log.Error( "No .build.info found for product " + Product + ", and online mode is NYI." );
				return Task.CompletedTask;
			}
			else
			{
				var build = buildInfo.Entries.First( x => x.Product == Product );

				if ( buildInstance.Settings.BuildConfig == null )
					buildInstance.Settings.BuildConfig = build.BuildConfig;

				if ( buildInstance.Settings.CDNConfig == null )
					buildInstance.Settings.CDNConfig = build.CDNConfig;
			}

			buildInstance.cdn.ProductDirectory = "tpr/wow";
			buildInstance.Settings.AdditionalCDNs = new List<string>() { "cdn.wago.tools", "cdn.arctium.tools" };

			buildInstance.cdn.OpenLocal();

			buildInstance.LoadConfigs( buildInstance.Settings.BuildConfig, buildInstance.Settings.CDNConfig );
			buildInstance.Load();

			base.Log.Info( "Adding listfile models..." );

			listfile = new Listfile();
			listfile.Initialize( buildInstance.cdn, buildInstance.Settings );
			listfile.GetFilename( 1 ); // trigger name load

			var limit = 5;
			var count = 0;

			var regex = "_\\d{1,3}(_lod\\d+)?\\.wmo";

			var reverseListfile = listfile.fdidToName.Reverse().ToDictionary( x => x.Key, x => x.Value );
			foreach ( var file in reverseListfile )
			{
				if ( !FileExistsByID( file.Key ) )
					continue;

				var filename = file.Value.ToLowerInvariant();
				if ( filename.EndsWith( ".m2" ) )
				{
					if ( filename.StartsWith( "item" ) )
						continue;

					context.Add( ResourceType.Model, file.Value, new WowModel
					{
						FileDataID = file.Key,
						BaseName = Path.GetFileNameWithoutExtension( file.Value )
					} );

					count++;
				}
				else if ( filename.EndsWith( ".wmo" ) )
				{
					if ( System.Text.RegularExpressions.Regex.IsMatch( filename, regex ) )
					{
						//Log.Info(filename + " is a group WMO, skipping.");
						continue;
					}

					context.Add( ResourceType.Model, file.Value, new WowWMO
					{
						FileDataID = file.Key,
						BaseName = Path.GetFileNameWithoutExtension( file.Value )
					} );

					count++;
				}

				//if (count > limit)
				//{
				//    base.Log.Info("Stopping model loading after " + limit + " items to prevent flooding.");
				//    break;
				//}
			}

			base.Log.Info( "Build loaded: " + buildInstance.BuildConfig!.Values["build-name"][0] );

			base.IsMounted = true;
			return Task.CompletedTask;
		}

		public Stream GetFileByID( uint fileDataID )
		{
			return new MemoryStream( buildInstance.OpenFileByFDID( fileDataID ) );
		}

		public bool FileExistsByID( uint fileDataID )
		{
			return buildInstance.Root!.FileExists( fileDataID );
		}

		public Sandbox.Texture LoadTexture( uint fileDataID )
		{
			if(fileDataID == 0 )
				return Sandbox.Texture.White;

			if ( !FileExistsByID( fileDataID ) )
			{
				base.Log.Warning( $"Texture file with ID {fileDataID} does not exist." );
				return Sandbox.Texture.White;
			}

			if ( textureCache.TryGetValue( fileDataID, out var cachedTexture ) )
				return cachedTexture;

			var blp = new BLPFile( GetFileByID( fileDataID ) );
			var pixels = blp.GetPixels( 0, out var width, out var height );
			var texture = Sandbox.Texture.Create( width, height, ImageFormat.BGRA8888 ).WithData( pixels ).Finish();
			textureCache[fileDataID] = texture;
			return texture;
		}

		public string? GetMountNameByID(uint fileDataID)
		{
			if ( !FileExistsByID( fileDataID ) )
			{
				Log.Warning( $"Model file with ID {fileDataID} does not exist." );
				return null;
			}
		
			if( listfile.fdidToName.TryGetValue( fileDataID, out var name ) )
			{
				var ext = (name.EndsWith(".wmo") || name.EndsWith(".m2")) ? ".vmdl" : throw new Exception("Requested mounted name for non-WMO/M2!");

				return "mount://wow/" + name.ToLowerInvariant() + ext;
			}
			else
			{
				Log.Warning( $"Model file with ID {fileDataID} not found in listfile." );
				return null;
			}
		}
	}
}
