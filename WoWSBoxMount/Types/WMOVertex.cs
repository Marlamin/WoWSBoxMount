public struct WMOVertex
{
	[VertexLayout.Position]
	public Vector3 position;

	[VertexLayout.Normal]
	public Vector3 normal;

	[VertexLayout.TexCoord(0)]
	public Vector2 texcoord0;

	[VertexLayout.TexCoord(1)]
	public Vector2 texcoord1;

	[VertexLayout.TexCoord(2)]
	public Vector2 texcoord2;

	[VertexLayout.TexCoord(3)]
	public Vector2 texcoord3;

	public static readonly VertexAttribute[] Layout =
	[
		new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2, 0),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2, 1),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2, 2),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2, 3)
	];

	public WMOVertex( Vector3 position, Vector3 normal, Vector2 texcoord0, Vector2 texcoord1, Vector2 texcoord2, Vector2 texcoord3 )
	{
		this.position = position;
		this.normal = normal;
		this.texcoord0 = texcoord0;
		this.texcoord1 = texcoord1;
		this.texcoord2 = texcoord2;
		this.texcoord3 = texcoord3;
	}
}
