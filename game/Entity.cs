using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>Game entity</summary>
public class Entity {

	[Serializable]
	public class Value<T> : ISerializable {
		public Value() {}
		public Value(T initialValue) {
			_value = initialValue;
		}
		public Value(SerializationInfo info, StreamingContext context) {
			_value = (T) info.GetValue("_value", typeof(string));
		}

		private T _value;
		public event EventHandler<ValueChangedEventArgs> ValueChanged;

		public class ValueChangedEventArgs : EventArgs {
			public T newValue;
			public T oldValue;

			public ValueChangedEventArgs(T _newValue, T _oldValue) {
				newValue = _newValue;
				oldValue = _oldValue;
			}
		}

		public T Get() {
			return _value;
		}

		public void Set(T newValue) {
			if (!EqualityComparer<T>.Default.Equals(_value, newValue)) {
				var oldValue = _value;
				_value = newValue;
				ValueChanged.Invoke(this, new ValueChangedEventArgs(newValue, oldValue));
			}
		}

		public static implicit operator T(Value<T> value) {
			return value.Get();
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("_value", _value, typeof(string));
		}
	}

    /// <summary>Create a game entity</summary>
    public Entity(String id_ = null) {
		if (id_ == null) {
			id = Guid.NewGuid().ToString();
		} else {
			id = id_;
		}
	}
	
	public Entity(SerializationInfo info, StreamingContext context) {
		// id = (string) info.GetValue("id", typeof(string));
	}

	public string id;
	internal GameState gameState;

	public void Attach(GameState _gameState) {
		gameState = _gameState;
	}

	public class EntityEventArgs : EventArgs {
		public EntityEventArgs(Entity entity) {
			this.entity = entity;
		}

    	public Entity entity { set; get; }
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context) {
		info.AddValue("__type", this.GetType(), typeof(Type));
	}
}
