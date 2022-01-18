using Godot;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Primitive;
using System.Reactive.Subjects;
using Hex;

public class HexMesh : Spatial {
    private readonly ChunkMeshData meshData;
    public ShaderMaterial material;
	public Subject<Vector2> MeshClickPos = new Subject<Vector2>(); 
	public Subject<Vector2> MeshHoverPos = new Subject<Vector2>(); 

	public HexMesh(
		string name,
		ChunkMeshData meshData,
		ShaderMaterial material
	) {
		this.Name = name;
        this.meshData = meshData;
        this.material = material;
	}

	public void Clear() {
		meshData.Clear();

		foreach (Node n in GetChildren()) {
			RemoveChild(n);
		}
	}

	public void GenerateMesh() {
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshData.surface);
		// GD.PrintT(indexArray.Length, uvArray.Length, colorArray.Length, vertexArray.Length, normalArray.Length);
		mesh.SurfaceSetMaterial(0, material);

		var meshInstance = new MeshInstance();
		meshInstance.Name = $"{Name}MeshInstance";
		meshInstance.Mesh = mesh;
		AddChild(meshInstance);

		if (meshData.hasCollision) {
			var staticBody = new StaticBody();
			staticBody.Name = $"{Name}StaticBody";
			var collision = new CollisionShape();
			collision.Name = $"{Name}Collision";
			collision.Shape = mesh.CreateTrimeshShape();
			staticBody.AddChild(collision);
			staticBody.Connect("input_event", this, nameof(_handle_input));
			AddChild(staticBody);
		}
	}

	private void _handle_input(Camera camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx) {
		var o = this.GlobalTransform.origin;
		var hexPosition = new Vector2(o.x, o.z) + new Vector2(position.x, position.z) - Hex.HexUtils.HexCenter;
		var hex = Hex.HexUtils.PixelToHexOffset(hexPosition);

		if (@event.IsActionPressed("ui_select")) {
			MeshClickPos.OnNext(hexPosition);
		} else {
			MeshHoverPos.OnNext(hexPosition);
		}
	}
}
