using DefaultEcs;
using GameWorld;
using Hex;


public struct PopData {
	public int size;
	public float growthRate;

    public PopData(int size, float growthRate) {
        this.size = size;
        this.growthRate = growthRate;
    }
}
