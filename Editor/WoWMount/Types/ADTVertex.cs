public struct ADTVertex
{
	[VertexLayout.Position]
	public Vector3 position;

	[VertexLayout.Normal]
	public Vector3 normal;

	[VertexLayout.TexCoord]
	public Vector2 texcoord;

	[VertexLayout.Color]
	public Vector4 color;

	public static readonly VertexAttribute[] Layout =
	[
		new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2),
		new VertexAttribute(VertexAttributeType.Color, VertexAttributeFormat.Float32, 4)
	];

	public ADTVertex( Vector3 position, Vector3 normal, Vector2 texcoord, Vector4 color )
	{
		this.position = position;
		this.normal = normal;
		this.texcoord = texcoord;
		this.color = color;
	}
}
