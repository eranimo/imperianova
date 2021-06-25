# Entity System

## Design
Each tick in the simulation is one day in the game world. Entities are software agents in the game simulation. Systems run on certain sets of Entities

Entities have the following:
- Data represented in local variables
- `import` and `export` functions, which serialize to a JSON-compatible Dictionary the state of the entity


# Entity Types

Tile specific:
- Pops: Parts of human population on a Tile
- Plants: The forests, grass, crops on a Tile
- Deposit: Finite supply of metal Resources on a Tile
- Animals: all living non-Human animals on a tile (Ocean or Land)
  - Domesticated animals: e.g. cattle
  - Wildlife (e.g. Aurochs, Deer, )
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
