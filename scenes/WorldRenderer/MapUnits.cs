using DefaultEcs;
using DefaultEcs.System;
using Godot;
using System;
using System.Collections.Generic;

[With(typeof(UnitData))]
[WhenChanged(typeof(TilePosition))]
class MapUnitsViewSystem : EntityViewSystem {
	private Node2D node;
	private PackedScene unitIconScene;
	private Dictionary<Entity, UnitIcon> entityUnityIconMap = new Dictionary<Entity, UnitIcon>();

	public MapUnitsViewSystem(DefaultEcs.World world, Node2D node) : base (world) {
		this.node = node;
		this.unitIconScene = GD.Load<PackedScene>("res://scenes/WorldRenderer/UnitIcon.tscn");
	}

	protected override void ProcessEntity(Entity unit) {
		UnitIcon unitIcon;
		var unitData = unit.Get<UnitData>();
		if (entityUnityIconMap.TryGetValue(unit, out unitIcon)) {
			unitIcon.SetIcon(unitData.unitType.Icon);
		} else {
			unitIcon = (UnitIcon) unitIconScene.Instance();
			entityUnityIconMap[unit] = unitIcon;
			node.AddChild(unitIcon);
		}
	}

}

public class MapUnits : Node2D {
	public override void _Ready() {
		
	}
}
