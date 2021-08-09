using Godot;
using System;

namespace Entities {
    public class Unit : Entity {
        public Unit(Vector2 initial_position) {
            position = initial_position;
        }

        public class UnitMoved : Message {
            public Unit Unit { set; get; }
            public Vector2 NewLocation { set; get; }
        }

        public Vector2 position { set; get; }

        public void Move(Vector2 newLocation) {
            gameState.SendMessage(new UnitMoved() {
                Unit = this,
                NewLocation = new Vector2(0, 0)
            });
        }
    }
}