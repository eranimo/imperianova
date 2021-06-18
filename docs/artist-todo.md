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
- desert changes:
    - remove shrubs from desert transition tilesets
    - move shrubs/rocks/cactus to their own layer like forests
- forest trees:
    - missing the base edge tileset for forest trees
    - base feature tilesets should be one variant per row, 7 columns with first being center, etc
    - only one tree layer variant required for 2-transition and 3-transition feature tilesets
        - the one provided does not match the 2-transition

# Future
- new terrain types:
    - deep ocean
        - Deep Ocean -> Ocean
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
    - warrior
    - transport