using Godot;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

/**
Summary:
	Holds all the Entity state
*/
public class GameState : Node {
	private HashSet<Entity> entities = new HashSet<Entity>();
	private Dictionary<string, Entity> entitiesById;
	private Dictionary<string, HashSet<Entity>> entitiesByType;
	private List<Manager> managers = new List<Manager>();
	private HashSet<Interface> interfaces = new HashSet<Interface>();
	private Queue<Message> messageQueue = new Queue<Message>();
	private List<Query> queries = new List<Query>();

	JsonSerializerSettings serializerSettings = new JsonSerializerSettings() {
		TypeNameHandling = TypeNameHandling.Auto,
		Formatting = Formatting.Indented, // TODO: debug mode only
		PreserveReferencesHandling = PreserveReferencesHandling.Objects
	};

	// Setup

	public GameState() {
		entitiesById = new Dictionary<string, Entity>();
		entitiesByType = new Dictionary<string, HashSet<Entity>>();
	}

	public override void _Ready() {
		GD.Print("[GameState] init");

		foreach(Node child in GetChildren()) {
			RegisterManager((Manager) child);
		}
	}

	public override void _EnterTree() {
		base._EnterTree();
		Global global = GetNode<Global>("/root/Global");
		global.gameState = this;
	}

	public override void _ExitTree() {
		base._ExitTree();
		Global global = GetNode<Global>("/root/Global");
		global.gameState = null;
	}

	public void RegisterManager(Manager manager) {
		managers.Add(manager);
	}

	// Manager related

	public void Update(uint ticks) {
		foreach (Manager manager in managers) {
			manager.Update();
		}
	}

	public Query Query() {
		return new Query(this);
	}

	public void FinalizeQuery(Query query) {
		GD.Print("[GameState] finalized query");
		foreach (Entity entity in entities) {
			if (query.Check(entity)) {
				query.items.Add(entity);
			}
		}
		queries.Add(query);
	}

	// Message and Interface related

	public void AddInterface(Interface item) {
		item.Init();
		interfaces.Add(item);
	}

	public void RemoveInterface(Interface item) {
		interfaces.Remove(item);
	}

	public void SendMessage(Message message) {
		messageQueue.Enqueue(message);
	}

	public void Process() {
		while (messageQueue.Count < 0) {
			Message message = messageQueue.Dequeue();
			// TODO: send Message to relevant Interfaces
		}
	}

	// Entity related

	public void AddEntity(Entity entity) {
		var type = entity.GetType().Name;
		// GD.Print("[GameState] add entity ", entity);
		entities.Add(entity);
		entitiesById.Add(entity.id, entity);
		if (!entitiesByType.ContainsKey(type)) {
			entitiesByType[type] = new HashSet<Entity>();
		}
		entitiesByType[type].Add(entity);
		entity.Attach(this);

		foreach(Query query in queries) {
			if (query.Check(entity)) {
				query.OnEntityAdded(entity);
				query.items.Add(entity);
			}
		}
	}

	public void RemoveEntity(Entity entity) {
		var type = entity.GetType().Name;
		entities.Remove(entity);
		entitiesByType[type].Remove(entity);
		entitiesById.Remove(entity.id);

		foreach(Query query in queries) {
			if (query.items.Contains(entity) && !query.Check(entity)) {
				query.OnEntityRemoved(entity);
				query.items.Remove(entity);
			}
		}
	}

	public Entity GetEntity(string id) {
		return entitiesById[id];
	}

	public string export() {
		return JsonConvert.SerializeObject(entities, serializerSettings);
	}

	public void import(string data) {
		List<Entity> importedEntities = JsonConvert.DeserializeObject<List<Entity>>(data, serializerSettings);
		foreach(Entity entity in importedEntities) {
			AddEntity(entity);
		}
	}

	public void clear() {
		foreach(Entity entity in entities) {
			var type = entity.GetType().Name;
			entitiesByType[type].Remove(entity);
			entitiesById.Remove(entity.id);

			foreach(Query query in queries) {
				if (query.items.Contains(entity) && !query.Check(entity)) {
					query.OnEntityRemoved(entity);
					query.items.Remove(entity);
				}
			}
		}
		entities.Clear();
	}

	public void debug() {
		GD.PrintS("Entities:", entities.Count());
	}
}
