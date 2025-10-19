# EditMap Scene UI Setup Guide

## Required UI Elements

Add these UI elements to your EditMap scene:

### Map Type Selection Panel
```
MapTypePanel (Panel)
├── TileBasedToggle (Toggle)
│   └── Label: "Tile-Based Map"
├── GAEATerrainToggle (Toggle)  
│   └── Label: "GAEA Terrain Map"
└── GAEAPanel (Panel) - Only active when GAEA selected
    ├── PathInputField (InputField)
    │   └── Placeholder: "Selected terrain file..."
    ├── BrowseButton (Button)
    │   └── Text: "Browse..."
    ├── LoadButton (Button)
    │   └── Text: "Load Terrain"
    └── StatusText (Text)
        └── Text: "Select map type to begin"
```

## Component Setup

### MapTypeSelector Component
Attach to a GameObject in the scene and assign:

**UI Elements:**
- Tile Based Toggle → TileBasedToggle
- Gaea Terrain Toggle → GAEATerrainToggle  
- Gaea Path Input → PathInputField
- Browse GAEA Button → BrowseButton
- Load GAEA Button → LoadButton
- Status Text → StatusText

**Map Managers:**
- Unified Map Manager → Reference to UnifiedMapManager in scene

### UnifiedMapManager Component
Attach to a GameObject and assign:
- Tile Based → Reference to existing GameMasterMapManager
- Gaea Terrain → Reference to GAEATerrainManager component

### GAEATerrainManager Component
Attach as child of UnifiedMapManager (no additional setup required)

## Usage Flow

1. **User selects map type** via toggles
2. **For GAEA terrain:**
   - Click "Browse..." button
   - Select .prefab or .asset file from PC
   - File is copied to Resources/GAEATerrains/
   - Click "Load Terrain" to initialize
3. **For tile-based:** Works as existing system

## File Requirements

**GAEA Terrain Files:**
- Must be Unity prefabs (.prefab) or terrain assets (.asset)
- Should contain a Terrain component
- Will be copied to Assets/Resources/GAEATerrains/ folder

## Integration with Existing System

The MapTypeSelector integrates with your existing:
- GameMasterMapManager (for tile-based maps)
- Save/Load system (saves map type with session)
- Object placement system (works on both map types)