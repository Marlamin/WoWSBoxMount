using Sandbox;

public struct M2Vertex
{
	[VertexLayout.Position]
	public Vector3 position;

	[VertexLayout.Normal]
	public Vector3 normal;

	[VertexLayout.TexCoord]
	public Vector2 texcoord;

	public static readonly VertexAttribute[] Layout = new VertexAttribute[3]
	{
		new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2)
	};

	public M2Vertex( Vector3 position, Vector3 normal, Vector2 texcoord )
	{
		this.position = position;
		this.normal = normal;
		this.texcoord = texcoord;
	}
}
