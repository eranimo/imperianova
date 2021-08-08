using Godot;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;


/**

Maintains a list of entities based on a predicate function
This list automatically updates

*/
public class Query {
    private GameState gameState;
    private HashSet<String> withTypes = new HashSet<string>();
    private HashSet<String> withIDs = new HashSet<string>();
    private List<Func<Entity, bool>> withFilters = new List<Func<Entity, bool>>();
    public HashSet<Entity> items = new HashSet<Entity>();

    public Query(GameState gameState_) {
        gameState = gameState_;
    }


    /// <summary>Filters entities by Type</summary>
    public Query WithType(String type) {
        withTypes.Add(type);
        return this;
    }

    /// <summary>Filters entities by ID</summary>
    public Query GetID(String id) {
        return this;
    }

    /// <summary>Filters entities by a function</summary>
    public Query Filter(Func<Entity, bool> func) {
        return this;
    }

    /// <summary>Checks if an entity should be in this query</summary>
    public bool Check(Entity entity) {
        if (withTypes.Count > 0 && !withTypes.Contains(entity.GetType().Name)) {
            return false;
        }

        if (withIDs.Count > 0 && !withIDs.Contains(entity.id)) {
            return false;
        }

        if (withFilters.Count > 0) {
            foreach (Func<Entity, bool> func in withFilters) {
                if (!func(entity)) {
                    return false;
                }
            }
        }

        return true;
    }

    public Query Done() {
        gameState.FinalizeQuery(this);
        return this;
    }
}