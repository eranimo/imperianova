using Godot;
using System;
using DefaultEcs.System;
using DefaultEcs;
using System.Collections.Generic;
using System.Reactive.Subjects;

[With(typeof(PopData))]
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
	private Label label;
	[GameController] private GameController game;
	[AttachViewSystem] private GamePanelViewSystem viewSystem;

	public override void _Ready() {
		this.label = (Label) FindNode("StatPopulationSize");
		this.ResolveDependencies();
	}

	public override void _ExitTree() {
		GD.PrintS("GamePanel exited");
		game.gameLoop.UnregisterViewSystem(viewSystem);
	}

	[GameInitHandler]
	void OnGameInit() {
		GD.PrintS("On game init");
		viewSystem.populationSize.Subscribe((int populationSize) => {
			this.label.Text = populationSize.ToString();
		});
	}
}
