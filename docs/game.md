# Game architecture
Godot using C# for heavy logic

## Game Logic


### Classes
- GameState
  - global Node
- Entity
  - has an ID
- Manager
  - properties
    - tick rate
  - methods
    - update
    - init
- Message
  - pieces of data that describe how
- Interface
  - Godot Node added as a child to UI nodes
  - receives Messages and computes internal UI state
  - emits signals that other UI nodes can listen to

## UI