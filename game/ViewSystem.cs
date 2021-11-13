using System;
using DefaultEcs;
using DefaultEcs.System;

class EntityViewSystem : AEntitySetSystem<GameDate> {
    public EntityViewSystem(DefaultEcs.World world) : base(world) {
        BeforeUpdate();
        var entities = this.Set.GetEntities();
        Process(entities);
        foreach(var entity in entities) {
            ProcessEntity(entity);
        }
        AfterUpdate();
    }

    public bool FilterEntities(Entity entity) {
        return true;
    }

    protected virtual void BeforeUpdate() {}
    protected virtual void AfterUpdate() {}
    protected virtual void Process(ReadOnlySpan<Entity> entities) {}
	protected virtual void ProcessEntity(Entity entity) {}

    protected override void PreUpdate(GameDate state) {
		BeforeUpdate();
	}

	protected override void Update(GameDate state, ReadOnlySpan<Entity> entities) {
		Process(entities);
	}

	protected override void Update(GameDate state, in Entity entity) {
		ProcessEntity(entity);
	}

	protected override void PostUpdate(GameDate state) {
		AfterUpdate();
	}
}
