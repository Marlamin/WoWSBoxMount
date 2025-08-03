public struct M2Vertex
{
	[VertexLayout.Position]
	public Vector3 position;

	[VertexLayout.Normal]
	public Vector3 normal;

	[VertexLayout.TexCoord]
	public Vector2 texcoord;

	[VertexLayout.BlendIndices]
	public Vector4 blendIndices;

	[VertexLayout.BlendWeight]
	public Vector4 blendWeights;

	public static readonly VertexAttribute[] Layout =
	[
		new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2),
		new VertexAttribute(VertexAttributeType.BlendIndices, VertexAttributeFormat.Float32, 4),
		new VertexAttribute(VertexAttributeType.BlendWeights, VertexAttributeFormat.Float32, 4)
	];

	public M2Vertex( Vector3 position, Vector3 normal, Vector2 texcoord, Vector4 blendIndices, Vector4 blendWeights )
	{
		this.position = position;
		this.normal = normal;
		this.texcoord = texcoord;
		this.blendIndices = blendIndices;
		this.blendWeights = blendWeights;
	}
}
