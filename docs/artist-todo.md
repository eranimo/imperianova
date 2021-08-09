# Terrain transitions
notation: PRIMARY -> SECONDARY (TERTIARY)
Ocean -> Grassland
Ocean -> Forest
Ocean -> Desert
Grassland -> Desert
Grassland -> Forest
Forest -> Desert

# Plan
- Terrain layer (hex sections)
    for each terrain type:
    - base tileset
    - 2-transition: primary being this terrain, secondary being transitioning terrain types
        1x per transitioning terrain
        e.g. Ocean -> Grassland
    - 3-transition
        1x per 2-transition whose secondary terrain type ALSO has a 2-transition with that secondary terrain type
            except if hexes of these two terrains would never meet (like Deep Ocean and Grassland)
        e.g. Ocean -> Grassland (Desert) because Ocean -> Grassland, Ocean -> Desert, and Grassland -> Desert
- Feature layer (hex sections)
    e.g. tall grass, trees, roads, etc (anything that can be removed or added)
    - base tileset
    - 2-transition
    - 3-transition
- Buildings layer (75% full hexagon with gaps at the edges for transitions underneath)
    - farm

# Immediate

# Future
- new terrain types:
    - tropical
        - Ocean -> Tropical Grassland
        - Ocean -> Tropical Rainforest
        - Grassland -> Tropical Grassland 
        - Forest -> Tropical Rainforest
        - Desert -> Tropical Grassland
        - Desert -> Tropical Rainforest
    - taiga
        - Ocean -> Taiga
        - Taiga -> Tundra
    - tundra
        - Ocean -> Tundra
        - Glacial -> Taiga
        - Glacial -> Tundra
    - glacial
        - Ocean -> Glacial
- rivers
- hills and mountains
- roads
- variations (feature layer):
    - sand dunes on desert
    - coral reefs on ocean
    - marshland on ocean
- buildings
    - village
    - farm
    - city
    - walls (feature layer)
- units
    - scout
    - swordsman
    - galley
    - land transport
    - sea transport
- icons
    - small icons (24x24): inline UI icons
        - population, (a portrait of a non-descript person)
        - labor (a hammer)
        - money (single gold coin)
        - food (a bushel of wheat)
        - world (a globe, should not be Earth)
        - housing (a house)
        - influence (a few dots surrounded by a border)
        - culture (open to ideas)
        - religion (open to ideas, not related to real religions)
    - large icons (48x48): action icons, job icons
        - attack (two crossed swords)
        - farmer
        - warrior
        - craftsman
        - slave
        - aristocrat
        - 
