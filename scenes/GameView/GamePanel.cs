using Godot;
using System;
using DefaultEcs.System;
using DefaultEcs;
using System.Collections.Generic;
using System.Reactive.Subjects;

[With(typeof(PopData))]
[WhenChanged()]
class GamePanelViewSystem : EntityViewSystem {
	public GamePanelViewSystem(DefaultEcs.World world) : base(world) {}

	public BehaviorSubject<int> populationSize = new BehaviorSubject<int>(0);
	private int _populationSize;

    protected override void Process(ReadOnlySpan<Entity> entities) {
        _populationSize = 0;
		foreach (var entity in entities) {
			_populationSize += entity.Get<PopData>().size;
		}
    }

	protected override void AfterUpdate() {
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
