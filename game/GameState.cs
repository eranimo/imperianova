using Godot;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

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

	// public class KnownTypesBinder : ISerializationBinder {
	// 	public Dictionary<string, Type> KnownTypes { get; set; }

	// 	public Type BindToType(string assemblyName, string typeName) {
	// 		return KnownTypes[typeName];
	// 	}

	// 	public void BindToName(Type serializedType, out string assemblyName, out string typeName) {
	// 		assemblyName = null;
	// 		typeName = KnownTypes.FirstOrDefault(t => t.Value.Name == serializedType.Name).Key;
	// 	}
	// }

	public class EntityConverter : JsonConverter {
		public override bool CanConvert(Type objectType) {
			return objectType.BaseType.Name == "Entity";
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			JToken t = JToken.FromObject(value);
			Entity entity = (Entity) value;
			JObject o = new JObject();
			var properties = ((JObject) t).Properties();
			// o.AddFirst(new JProperty("id", entity.id));

			foreach (var prop in entity.GetType().GetProperties()) {
				var propValue = prop.GetValue(entity);
				GD.PrintS(prop.Name, propValue.GetType().Name, propValue.GetType().Equals(typeof(Entity.Value<>)));
				// if (propValue.GetType().Name == 'Entity.Value') {
				o.AddFirst(new JProperty(prop.Name, 0));
			}

			// foreach (JProperty prop in properties) {
			// 	var propType = entity.GetType().GetProperty(prop.Name);
			// 	GD.PrintS(prop.Name, propType.GetValue(entity));
			// 	o.AddFirst(new JProperty(prop.Name, prop.Value));
			// }
			o.AddFirst(new JProperty("__type", value.GetType().Name));
			o.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override bool CanRead {
			get { return false; }
		}
		public override string ToString() {
			return base.ToString();
		}		
	}

  	JsonSerializerSettings serializerSettings = new JsonSerializerSettings() {
		TypeNameHandling = TypeNameHandling.Auto,
		Formatting = Formatting.Indented, // TODO: debug mode only
		PreserveReferencesHandling = PreserveReferencesHandling.Objects,
		// SerializationBinder = new KnownTypesBinder {
		// 	KnownTypes = new Dictionary<string, Type> {
		// 		{ "Entity", typeof(Entity) },
		// 		{ "Unit", typeof(Unit) },
		// 		{ "Tile", typeof(Tile) },
		// 	},
		// },
		Converters = new List<JsonConverter>() {
			{ new EntityConverter() }
		}
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
		try {
			List<Entity> importedEntities = JsonConvert.DeserializeObject<List<Entity>>(data, serializerSettings);
			foreach(Entity entity in importedEntities) {
				AddEntity(entity);
			}
		} catch (Exception err) {
			GD.PushError(err.ToString());
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
