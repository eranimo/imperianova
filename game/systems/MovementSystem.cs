using System;
using DefaultEcs;
using DefaultEcs.System;
using Godot;

[With(typeof(Movement))]
[With(typeof(TilePosition))]
class MovementSystem : AEntitySetSystem<GameDate> {
	public MovementSystem(DefaultEcs.World world) : base(world) {}

	protected override void Update(GameDate date, in Entity entity) {
		ref TilePosition tilePosition = ref entity.Get<TilePosition>();
		ref Movement movement = ref entity.Get<Movement>();

		// entity.Set<TilePosition>(new TilePosition(new Hex.OffsetCoord(tilePosition.position.Col + 1, tilePosition.position.Row)));
		tilePosition.position = new Hex.OffsetCoord(tilePosition.position.Col + 1, tilePosition.position.Row);
		entity.NotifyChanged<TilePosition>();
	}
}