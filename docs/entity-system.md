# Entity System

## Design
Each tick in the simulation is one day in the game world. Entities are software agents in the game simulation. Systems run on certain sets of Entities

- Entities (Object)
  - Have a specific Type
  - Can contain references to other entities
  - Has data represented in local variables
  - `static from_dict` and `to_dict` functions, which serialize to a JSON-compatible Dictionary the state of the entity
- EntityEvent (Object)
  - entity type, event type, event data
- Systems (Node)
  - Updates entities based on signals and timers
- EntityReader (Node)
  - Listens to one or more entities and optional map function
  - Sends update signal when entities change

# Entity Types

Tile specific:
- Pops: Parts of human population on a Tile
- Plants: The forests, grass, crops on a Tile
- Deposit: Finite supply of metal Resources on a Tile
- Animals: all living non-Human animals on a tile (Ocean or Land)
  - Domesticated animals: e.g. cattle
  - Wildlife (e.g. Aurochs, Deer, Fowl)
- TileDevelopments
  - Districts: e.g. city center, neighborhood, settlement, fort, camp
  - Improvements: e.g. farms, mines, plantations

Other:
- Countries
- Cultures
- Religions

# Systems
Every day:
- PlantGrowth: grows plants 
- AnimalGrowth: handles consumption and handles calculating growth
- PopGrowth: handles consumption and handles calculating growth

First of every month:
