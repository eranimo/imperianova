using Godot;
using System;

public class MapLabels : Node2D {
	public override void _Ready() {
		var labelsRoot = GetNode("LabelsRoot");
		var mapLabelScene = GD.Load<PackedScene>("res://scenes/WorldRenderer/MapLabel.tscn");
		var l1 = (MapLabel) mapLabelScene.Instance();
		labelsRoot.AddChild(l1);
		l1.Label = "First Label";
		l1.UpdatePosition(new Vector2(0, 0));
	}
}
