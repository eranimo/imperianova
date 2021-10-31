# Gameplay
The player controls a political entity in a procedurally generated historical simulation of a random Earth-like planet. Events happen within the simulation in a similar manner that they did in Earth's history but on a planet with a completely random geography.

Important characters throughout history exist in a detailed political simulation. These characters combine to form a wide array of Organizations that greatly influence events in the game.

Cultures are groups of Pops and Cultures with a distinct identity.

Groups of people represented in cultures, religions, and organizations like armies, companies, and families.

## Polities
Polities are political entities that represent groups of Pops and Characters that have shared political allegiance.

Polities control Territories, which are groups of Tiles with a name.

Polities are controlled by the AI or the player.

Polities can be subservient to other polities. The Player might play a polity that is subservient to an AI controlled polity.

Polities can be either alive or dead. A dead polity is one that has no Organizations.

Polities are created by other polities.

Polities control Organizations, which have Jobs that Characters are assigned to.

Polities have a Government (Organization) which controls it.

## Cultures
A Culture is a group of Pops and Characters that have a common shared identity.

Cultures have a name and parent and children cultures.

A culture with no Pops and Characters is dead.

### Traditions
Traditions are attributes that Cultures earn through regular actions. Each tradition type has a list of required actions and a count of the number of times they must be performed to unlock the tradition. Traditions give bonuses to gameplay.

Traditions have a decay rate and a reinforcement rate. Traditions that were unlocked by actions no longer performed will eventually be removed.



## Units
Units are groups of Pops that exist on the map and can move. They are led by Characters and belong to Organizations.

Units may be disbanded, which removes them from the map. The constituent Pops and Characters are not deleted.

Units have cohorts, to which several Pops are assigned. Each Cohort type has a max pop size.

### Unit types
Unit types are preselected groups of cohorts. The Player can create new unit types.

### Cohort Types
Military:
- Archer
- Slinger
- Spearman
- Calvary
- Chariot
- Horse Archer

Civilian:
- Caravan (transports resources)
- Laborers: performs civilian Construction
- Engineers: performs military construction
- Craftsman: performs Production

### Raising units
TODO

### Orders
Units are given orders by their Organization (or the Player, if the Organization is controlled by the Player)

- Move
- Disband
- Attack
- Migrate

### Supply
Units consume Resources, the amount of which depends on the Pops in the Unit.

## Battles
Two units on the same Tile that are enemies initiates a battle.

## Resources
ResourceTypes are distinct types of resources (e.g. Corn)
ResourceCategories are classes of resources (e.g. Food)

- Food
  - Corn
  - Wheat
  - Berries
  - Vegetables
  - Mushrooms
  - Meat
- Materials
  - Wood
  - Stone
  - Flint
- Ore
  - Gold ore
  - Tin ore
  - Copper ore
  - Iron ore
- Metal
  - Gold
  - Tin
  - Copper
  - Bronze
  - Iron
  - Steel
- Animal
  - Horses

### Resource stats
- Weight: determines storage, movement speed of Caravans
- Nutrition: amount this resource provides as a food item to pops

### Resource Nodes
Resource Nodes exist on Tiles, they are responsible for generating Resources (besides Improvements and Districts)
Resource Nodes have a maximum size and an optional replenish rate (depending on the resource)

### Production
Some Resources require other resources to produce.

## World (Tiles)
The world is split into a hexagon grid with cells called Tile entities.

Tiles can have one District or one Improvement, which allows Tiles to be exploited.

## Improvements


### Improvement types
- Farm (must be build on tile with fertility)
- Mine (must be built on Resource Node)

## Cities Districts
A District can have multiple Buildings. Each district can only have one of each building type.

Cities are a group of neighboring districts, each District belongs to one city. Cities have Government Organizations.

District types and buildings:
- Fortress
  - Keep
- Settlement
  - Houses


District stats:
- Housing
- Storage


#### Building limit
A District has a maximum number of buildings. This is determined by the district type.

## Population (Pops)
Population is represented as Pop entities, which live in Tiles and belong to Cultures.

Population can move to neighboring Tiles by *Migration*.

### Needs
There are three categories of needs:
- Basic Needs: required to *survive*
- Productive Needs: required to be *productive*
- Luxury Needs: required to be *happy*

Need types:
- FoodNeed (survival)
- Belonging (survival)
- Survival (survival)
- Health (productive)
- Entertainment (happiness)
- Luxury (happiness)

## Characters
Characters are entities representing individual people in Pops, usually important people.

Characters have a citizenship with a Polity.

Characters perform Jobs for Organizations.

Characters have stats, which influence how they perform Jobs.

### Stats
Modelled on DND

- Health: current health points
- MaxHealth: max health the character can have
- HealthRate: change in health per day
- Efficiency: defines how good Characters are at performing Jobs. This is influenced by other stats.
- Strength
- Dexterity
- Constitution 
- Intelligence
- Wisdom
- Charisma

### Technology
Characters have a set of Technologies that they are aware of. This allows them to perform certain actions, and influences their stats.

Characters spread technology while performing other actions. They spread them naturally to their relatives by a certain rate per month.

Characters can forget technologies when the actions that influenced them stop being reinforced. All characters have a TechnologyDecayRate for each technology that defines how quickly they forget the given technology.

Characters have a TechnologyTransferRate, which determines how often this character spreads this technology.

## Organization
Organizations are groups of Characters.

They have a type, which defines their behavior.

A Character can belong to multiple Organizations.

Organizations issue Orders, which allow Organizations to perform actions in the simulation. They are assigned to Jobs. When controlled by a Polity that is controlled by the player, Orders are performed by the player.

Organizations own Buildings.

### Subsidiary
Organizations have subsidiary organizations, which they control. Subsidiaries need not be of the same type.

### Types
- Government (controlled by a group of Pops)
- Military
  - Army (controlled by Polity)
  - Unit (controlled by Army)
  - Militia (controlled by a group of Pops)
  - Mercenary company
- Economic
  - Company
  - Guild
- Population
  - Family

#### Government
Government is the primary Organization in a Polity and the primary way it is controlled.

#### Unit
Units belonging to Organizations controlled by the Player can be directly controlled.

### Jobs
Organizations have Jobs, which are assigned to Characters. They have a type, which defines their actions.

Examples of Jobs:
- Ruler (Government)
- Commander (Army)
- Officer (Company)
- 
A Job is worth PowerPoints, which is multiplied by the Efficiency of the character assigned to this Job.

#### OrganizationPower
The total number of PowerPoints for all jobs in this organization is called the OrganizationPower

OrganizationPower = Sum of all PowerPoints for all Jobs in this Organization +
                    Sum of all OrganizationPower in Subsidiary Organizations

# Game architecture

## World
The world is split into Tile classes, representing hexes in a rectangular grid.

Tiles have a Temperature and Rainfall.

Tiles have a *TerrainType*, determined by Temperature and Rainfall. For example, Desert and Grasslands.

The TerrainType determines *MovementCost*, which is an integer value representing how hard it is to move across this particular Tile.

## Game Logic

Events are messages associated with a Date, a MessageType, and a Message

### Data types
#### Resource
- name
- technology: technology that unlocks this resource
- needType

### Entities
- Unit
  - TilePosition
  - UnitData
  - Moveable
  - Health
- Pop
  - TilePosition
  - PopData
  - Moveable
  - Consumption
  - Health
- Territory
  - TerritoryData
- Improvement
  - TilePosition
  - ImprovementData
- Country
  - CountryData
- Organization
  - OrganizationData

### Components
- TilePosition
  - location
- UnitData
  - unitType
  - health
- Movable
  - currentTarget: world Tile ref
  - movementType: land or water
  - movementSpeed: movement cost that can be spent per day
  - movementQueue: list of tiles to move to next
- Consumption: an entity that consumes resources
  - requirements
  - status
- Health
  - health
  - maxHealth
  - healthRate
- PopData
  - size
  - popType
- ImprovementData
  - improvementType
  - isConstructed
  - constructionProgress
- TerritoryData
  - name
  - tiles
- CountryData
  - name
  - isPlayer
  - territories

### Systems
- MovementSystem (Movable, TilePosition)
- 