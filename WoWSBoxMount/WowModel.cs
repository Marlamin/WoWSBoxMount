using BLPSharp;
using Sandbox;
using Sandbox.Mounting;
using WoWFormatLib.FileReaders;

namespace WoWSBoxMount
{
    internal class WowModel : ResourceLoader<WowMount>
    {
        public uint FileDataID { get; set; }

        protected override object Load()
        {
            object model;

            Log.Info("Loading WoW M2 " + FileDataID + "...");

            var m2Reader = new M2Reader(base.Host);
            using (var fs = base.Host.GetFileByID(FileDataID))
            {
                m2Reader.LoadM2(fs);
            }

            var m2 = m2Reader.model;
            var verticeList = new List<SimpleVertex>();
            var vectorList = new List<Vector3>();
            foreach (var vertice in m2.vertices)
            {

                var position = new Vector3(vertice.position.x, vertice.position.y, vertice.position.z);
                vectorList.Add(position);
                var normal = new Vector3(vertice.normal.x, vertice.normal.y, vertice.normal.z);
                var texCoord = new Vector2(vertice.textureCoordX, vertice.textureCoordY);
                verticeList.Add(new SimpleVertex(
                    position * 30f,
                    normal,
                    Vector3.Zero,
                    texCoord
                ));
            }

            var indiceList = new List<int>();
            foreach (var indice in m2.skins[0].triangles)
            {
                indiceList.Add(indice.pt1);
                indiceList.Add(indice.pt2);
                indiceList.Add(indice.pt3);
            }

            var meshList = new List<Mesh>();
            for (var submeshIndex = 0; submeshIndex < m2.skins[0].submeshes.Length; submeshIndex++)
            {
                var submesh = m2.skins[0].submeshes[submeshIndex];
                Log.Info($"Submesh ID: {submesh.submeshID}, Vertices: {submesh.nVertices}, Triangles: {submesh.nTriangles}");

                Material material = null;

                for (var tu = 0; tu < m2.skins[0].textureunit.Length; tu++)
                {
                    if (m2.skins[0].textureunit[tu].submeshIndex == submeshIndex)
                    {
                        var textureFileDataID = m2.textureFileDataIDs[m2.texlookup[m2.skins[0].textureunit[tu].texture].textureID];

                        Log.Info($"Texture Unit {tu}: Flags: {m2.skins[0].textureunit[tu].flags}, Shading: {m2.skins[0].textureunit[tu].shading}, Submesh Index: {m2.skins[0].textureunit[tu].submeshIndex}, Texture File Data ID: {textureFileDataID}");

                        material = Material.Create(textureFileDataID.ToString(), "simple_color");
                        Sandbox.Texture texture = null;

                        if (textureFileDataID == 0)
                        {
                            texture = Sandbox.Texture.White;
                        }
                        else
                        {
                            var blp = new BLPFile(base.Host.GetFileByID(textureFileDataID));
                            var pixels = blp.GetPixels(0, out var width, out var height);
                            texture = Sandbox.Texture.Create(width, height, ImageFormat.BGRA8888).WithData(pixels).Finish();
                        }

                        material.Set("Color", texture);
                    }
                }

                var mesh = new Mesh(material);

                mesh.CreateVertexBuffer(verticeList.Count, SimpleVertex.Layout, verticeList);

                var indiceArr = indiceList.ToArray();
                mesh.CreateIndexBuffer(indiceArr.Length, indiceArr);
                mesh.SetIndexRange((int)submesh.startTriangle * 3, (int)submesh.nTriangles * 3);
                mesh.Bounds = BBox.FromPoints(verticeList.Select((SimpleVertex x) => x.position), 0f);
                meshList.Add(mesh);
            }

            model = Model.Builder.WithName("WoWTest").AddMeshes([.. meshList]).Create();
            Log.Info("WoW model loaded successfully.");
            return model;
        }
    }
}
