using System;
using System.Linq;
using System.Reflection;
using DefaultEcs.System;
using Godot;

public static class NodeExtensions {
	public static void ResolveDependencies(this Node node) {
		// GameController attribute
		var game = (GameController) node.GetTree().CurrentScene.FindNode("Game");
		var gameAttributes = node.GetType()
			.GetRuntimeFields()
			.Where(f => f.GetCustomAttributes(typeof(GameControllerAttribute), true).Any());

		foreach(var field in gameAttributes) {
			field.SetValue(node, game);
		}
		
		// view system attributes
		var viewSystemAttributes = node.GetType()
			.GetRuntimeFields()
			.Where(f => f.GetCustomAttributes(typeof(AttachViewSystemAttribute), true).Any());
		
		game.OnInit(() => {
			foreach(var field in viewSystemAttributes) {
				var viewSystem = (ISystem<GameDate>) Activator.CreateInstance(
					field.FieldType,
					new object[]{game.gameLoop.entityManager}
				);
				game.gameLoop.RegisterViewSystem(viewSystem);
				field.SetValue(node, viewSystem);
			}
		});

		// game init attributes
		var gameInitAttributes = node.GetType()
			.GetRuntimeMethods()
			.Where(f => f.GetCustomAttributes(typeof(GameInitHandlerAttribute), true).Any());

		foreach(var method in gameInitAttributes) {
			GD.PrintS(method.Name, game);
			game.OnInit(() => method.Invoke(node, new object[]{}));
		}
	}
}
