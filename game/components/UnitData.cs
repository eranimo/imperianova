using DefaultEcs;
using GameData;
using GameWorld;
using Hex;


public struct UnitData {
	public Entity[] pops;
	public UnitType unitType;

    public UnitData(Entity[] pops, UnitType unitType) {
        this.pops = pops;
		this.unitType = unitType;
    }
}
