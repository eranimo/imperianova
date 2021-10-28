# Gameplay
The player controls a political entity in a procedurally generated historical simulation of a random Earth-like planet. Events happen within the simulation in a similar manner that they did in Earth's history but on a planet with a completely random geography.

The player controls a Polity.

Important characters throughout history exist in a detailed political simulation. Combined these characters form Organizations which greatly influential to history.

Cultures are groups of Pops and Cultures with a distinct identity.

Groups of people represented in cultures, religions, and organizations like armies, companies, and families.

## Polities
Polities are political entities that represent groups of Pops and Characters that have shared political allegiance.

Polities control Territories, which are groups of Tiles with a name.

Polities are controlled by the AI or the player.

Polities can be either alive or dead.

Polities are created by other polities.

Polities control Organizations, which have Jobs that Characters are assigned to.

Polities have a Government (Organization) which controls it.

## Cultures
A Culture is a group of Pops and Characters that have a common shared identity.

Cultures have a name and parent and children cultures.

A culture with no Pops and Characters is dead.

Cultures have Traditions

### Traditions
Traditions are attributes that Cultures earn.

## World (Tiles)
The world is split into a hexagon grid with cells called Tile entities.

## Population (Pops)
Population is represented as Pop entities, which live in Tiles and belong to Cultures.

Population can move to neighboring Tiles in a Migration event. 

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
- Efficiency: defines how good Characters are at performing tasks at a given time. This is influenced by other stats.
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

### Subsidiary
Organizations have subsidiary organizations, which they control. Subsidiaries need not be of the same type.

### Types
- Government (controlled by a group of Pops)
- Military
  - Army (controlled by Polity)
  - Unit (controlled by Army)
  - Militia (controlled by a group of Pops)
  - Mercenary company
  - Criminal organization
- Economic
  - Company
  - Guild
- Population
  - Family

#### Government
Government is the primary organization in a Polity and the primary way it is controlled.

#### Unit
Units belonging to Organizations controlled by the Player are visible on the map.

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

## Game Logic

Events are messages associated with a Date, a MessageType, and a Message

### Data types
#### Resource
- name
- technology: technology that unlocks this resource
- needType

### Entities
- Unit
  - TileLocation
  - UnitData
  - Moveable
  - Health
- Tile
  - TileLocation
  - TileData
- Pop
  - TileLocation
  - PopData
  - Moveable
  - Consumption
  - Health
- Territory
  - TerritoryData
- Improvement
  - TileLocation
  - ImprovementData
- Country
  - CountryData
- Organization
  - OrganizationData

### Components
- TileLocation
  - location
- UnitData
  - unitType
  - health
- Movable
  - currentTarget
  - movementSpeed
  - movementQueue
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
- TileData
  - height
  - temperature
  - rainfall
  - terrainType
  - riverMap
  - hasRoad
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

### Engines
- UnitMovementEngine (UnitData, TileLocation)
- 