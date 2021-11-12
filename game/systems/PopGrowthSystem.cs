using System;
using DefaultEcs;
using DefaultEcs.System;
using Godot;

class PopGrowthSystem : AEntitySetSystem<GameDate> {
	public PopGrowthSystem(DefaultEcs.World world)
		: base(world.GetEntities().With<PopData>().With<TilePosition>().AsSet()) {
	}

	protected override void Update(GameDate date, in Entity entity) {
		ref TilePosition tilePosition = ref entity.Get<TilePosition>();
		ref PopData popData = ref entity.Get<PopData>();

		popData.size += (int) Math.Ceiling(popData.size * (popData.growthRate));
		GD.PrintS("Pop size", popData.size);
	}
}