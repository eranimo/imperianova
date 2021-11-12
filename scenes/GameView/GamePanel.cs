using Godot;
using System;
using DefaultEcs.System;
using DefaultEcs;
using System.Reactive.Subjects;

[With(typeof(PopData))]
class GamePanelViewSystem : AEntitySetSystem<GameDate> {
	public GamePanelViewSystem(DefaultEcs.World world) : base(world) {
        int count = 0;
        foreach(var entity in this.Set.GetEntities()) {
            var popData = entity.Get<PopData>();
            count += popData.size;
        }
        populationSize.OnNext(count);
    }

	public BehaviorSubject<int> populationSize = new BehaviorSubject<int>(0);
	private int _populationSize;

	protected override void PreUpdate(GameDate state) {
		_populationSize = 0;
	}

	protected override void Update(GameDate state, in Entity entity) {
		ref PopData popData = ref entity.Get<PopData>();
		_populationSize += popData.size;
	}

	protected override void PostUpdate(GameDate state) {
        GD.PrintS("Pop total size", _populationSize);
		populationSize.OnNext(_populationSize);
	}
}

public class GamePanel : Panel {
	private GameController game;
	private GamePanelViewSystem viewSystem;
	private Label label;

	public override void _Ready() {
		this.game = (GameController) GetTree().CurrentScene.FindNode("Game");
		this.game.Connect("GameInit", this, nameof(_on_game_init));
	}

	public override void _ExitTree() {
		this.game.gameLoop.UnregisterViewSystem(viewSystem);
	}

	private void _on_game_init() {
        GD.PrintS("On game init");
		viewSystem = new GamePanelViewSystem(this.game.gameLoop.entityManager);
		this.label = (Label) FindNode("StatPopulationSize");
		this.game.gameLoop.RegisterViewSystem(viewSystem);

		viewSystem.populationSize.Subscribe((int populationSize) => {
			this.label.Text = populationSize.ToString();
		});
	}
}
