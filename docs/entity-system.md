# Entity System

## World
- Each Tile can have several Pops, Plants, and Animals
  - Ocean Tiles cannot have Pops

## Design
Each tick in the simulation is one day in the game world. Entities are software agents in the game simulation. Systems run on certain sets of Entities

- Entities (Object)
  - Has an entity type
  - Has a set of properties
  - Serialize / deserialize functions
  - Can have multiple components, one of each type, or none at all
- Components (Object)
  - Has a component type
  - Has a set of properties
  - Serialize / deserialize functions
- EntityEvent (Object)
  - event type, event data
- Systems (Node)
  - Updates entities based on signals and timers

# Entities
Entities (with their properties in parenthesis) and components in each entity

- Pops (social class)
  - TilePosition
  - Consumption
  - Migration
  - LifeformGroup
- Plants (type)
  - TilePosition
  - LifeformGroup
  - Consumable
- Animals (type)
  - TilePosition
  - Consumption
  - Migration
  - LifeformGroup
  - Consumable
- Unit (type)
  - TilePosition
  - Transport
  - MapIcon
  - Movement
- Deposit (type, size)
  - TilePosition
- Country (name, color, list of Territory)
- Resource (size)
  - TilePosition
  - Itemized
  - Consumable
- Development (type)
  - TilePosition
- Territory (owner Country)
  - TilePosition

# Components
- TilePosition
- MapIcon: svg icon + text label (will replace with Sprite)
- Health: health points, FSM (healthy, sick, starving)
- Consumption: consumes resources, impacts Health and Growth
  - list of consumption (entities with Consumable component)
- Migration: can migrate to find resources when lacking
- LifeformGroup: represents a group of lifeforms
  - group size
  - growth rate
  - health state and HP
- Movement: allows moving to other Tile
  - target location
  - movement queue
- TerritoryData
- Itemized
  - allows transport
- Transport
  - allows loading entities with Itemized component

# Systems
Rendering:
- MapRenderSystem: TilePosition, MapIcon
- UnitRenderSystem: TilePosition, UnitIcon

Logic:
- UnitMovementSystem: TilePosition, UnitMoveQueue
- GrowthSystem: LifeformGroup

UI:
- MapSelectionSystem: 

# Entity Types (OLD)

Tile specific:
- Pops: Human population on a Tile
- Plants: The plant life on a Tile
  - Trees
  - Grass
- Animals: all living non-Human animals on a tile (Ocean or Land)
  - Domesticated animals: e.g. cattle
  - Wildlife (e.g. Aurochs, Deer, Fowl)
- Deposit: Finite supply of metal Resources on a Tile
- Developments
  - Districts: e.g. city center, neighborhood, settlement, fort, camp
  - Improvements: e.g. farms, mines, plantations

Other:
- Countries
- Cultures
- Religions