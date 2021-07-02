# ImperiaNova Project Plan


## Versions

v0.1.0
- Pops: size, social class, location
- Plants: size, species, location
- Animals: size, species, location
- Deposits: size, location, ore type
- Countries: name, list of tiles in territory, dominant cultures
  - One player controlled only
- Tile developments: instant build
  - Districts
    - list of districts:
      - Camp
      - Hamlet
      - Village
    - stats
      - housing capacity (positive integer)
  - Improvements
    - Farm
    - Mine
- Units: only one unit per category per tile
  - Economic
    - Builder
    - Transport
  - Exploration
    - Scout
- Unit actions
  - Move: move to other tile
  - Build: build development on current tile

v0.2.0
- Combat: stats-based combat system with attack, defense
- Roads: built by builder
- Unit movement cost
- Tile developments: construction period
- Countries
  - AI countries
- Units
  - Economic
    - Hunting party
    - Harvesting party
  - Military
    - War band
    - Galley
- Unit actions
  - Trade Route: create trade route to tile
  - Attack: follow unit and enter combat when both are on the same tile
  - Build road: moves and builds road to tile
    - Building efficiency: multiplier on build cost
  - Hunt: move within 1 day travel radius, hunting animals and returning 
    - Hunting efficiency: percent of animals in a tile that are hunted

v0.3.0
- Culture: name, parent culture, child cultures
- Technology: name, requirements, prerequisites, unlocked development
- Religion: name, type, gods