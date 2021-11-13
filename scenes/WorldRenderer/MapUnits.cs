using DefaultEcs;
using DefaultEcs.System;
using Godot;
using System;
using System.Collections.Generic;

[With(typeof(UnitData))]
[With(typeof(TilePosition))]
[WhenChanged(typeof(TilePosition))]
class MapUnitsViewSystem : AEntitySetSystem<GameDate> {
	private Node2D node;
	private PackedScene unitIconScene;
	private Dictionary<Entity, UnitIcon> entityUnityIconMap = new Dictionary<Entity, UnitIcon>();

	public MapUnitsViewSystem(DefaultEcs.World world) : base (world) {
		this.unitIconScene = GD.Load<PackedScene>("res://scenes/WorldRenderer/UnitIcon.tscn");
	}

	private void ProcessEntity(Entity entity) {
		UnitIcon unitIcon;
		var unitData = entity.Get<UnitData>();
		if (entityUnityIconMap.TryGetValue(entity, out unitIcon)) {
			unitIcon.SetIcon(unitData.unitType.Icon);
		} else {
			unitIcon = (UnitIcon) unitIconScene.Instance();
			entityUnityIconMap[entity] = unitIcon;
			node.AddChild(unitIcon);
		}
		var tilePosition = entity.Get<TilePosition>();
		var pos = Hex.HexUtils.HexToPixelCenter(tilePosition.position);
		unitIcon.SetPosition(pos - new Vector2(12, 12));
	}

	public void AttachNode(Node2D node) {
		this.node = node;

		foreach (var entity in this.World.GetEntities().With<UnitData>().With<TilePosition>().AsEnumerable()) {
			ProcessEntity(entity);
		}
	}

	protected override void Update(GameDate date, in Entity entity) {
		ProcessEntity(entity);
	}
}

public class MapUnits : Node2D {
	[GameController] private GameController game;
	[AttachViewSystem] private MapUnitsViewSystem viewSystem;

	public override void _Ready() {
		base._Ready();
		this.ResolveDependencies();
	}

    public override void _ExitTree() {
        base._ExitTree();
		game.gameLoop.UnregisterViewSystem(viewSystem);
    }

    [GameInitHandler]
	private void OnGameInit() {
		viewSystem.AttachNode(this);
	}
}
