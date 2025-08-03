public struct M2Vertex
{
	[VertexLayout.Position]
	public Vector3 position;

	[VertexLayout.Normal]
	public Vector3 normal;

	[VertexLayout.TexCoord]
	public Vector2 texcoord;

	[VertexLayout.BlendIndices]
	public Color32 blendIndices;

	[VertexLayout.BlendWeight]
	public Color32 blendWeights;

	public static readonly VertexAttribute[] Layout =
	[
		new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
		new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2),
		new VertexAttribute(VertexAttributeType.BlendIndices, VertexAttributeFormat.UInt8, 4),
		new VertexAttribute(VertexAttributeType.BlendWeights, VertexAttributeFormat.UInt8, 4)
	];

	public M2Vertex( Vector3 position, Vector3 normal, Vector2 texcoord, Color32 blendIndices, Color32 blendWeights )
	{
		this.position = position;
		this.normal = normal;
		this.texcoord = texcoord;
		this.blendIndices = blendIndices;
		this.blendWeights = blendWeights;
	}
}
