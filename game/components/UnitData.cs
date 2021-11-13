using DefaultEcs;
using GameWorld;
using Hex;


public struct UnitData {
	public Entity[] pops;

    public UnitData(Entity[] pops) {
        this.pops = pops;
    }
}
