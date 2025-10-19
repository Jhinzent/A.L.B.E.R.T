# GAEA Terrain Integration Guide

## Overview
This system allows your Unity combat simulation to use either the existing tile-based maps OR GAEA terrain maps, with full compatibility for both game master and player views.

## Setup Instructions

### 1. Scene Setup
Add these components to your scenes:

**Create Session Scene:**
- Add `UnifiedMapManager` to a GameObject
- Add `GAEATerrainManager` as a child component
- Add `MapTypeSelector` to your UI
- Connect references in the inspector

**Main Game Scene:**
- Add `UnifiedMapManager` to a GameObject  
- Add `GAEATerrainManager` as a child component
- Add `GAEAPlayerMapConverter` to a GameObject
- Update `PlayerMapLoader` references in inspector

### 2. GAEA Terrain Preparation
1. Export your GAEA terrain as a prefab
2. Place the prefab in `Resources/` folder
3. Ensure the prefab has a `Terrain` component
4. Configure texture mapping in `GAEATerrainManager.ConvertTextureIndexToTerrainType()`

### 3. UI Integration
Add to your map creation UI:
```
- Toggle for "Tile-Based Map"
- Toggle for "GAEA Terrain Map"  
- Input field for terrain path
- Load button for GAEA terrain
```

### 4. Inspector Connections
**UnifiedMapManager:**
- Tile Based: Reference to GameMasterMapManager
- Gaea Terrain: Reference to GAEATerrainManager

**PlayerMapLoader:**
- Unified Map Manager: Reference to UnifiedMapManager
- Gaea Converter: Reference to GAEAPlayerMapConverter

**GameMasterMapLoader:**
- Unified Map Manager: Reference to UnifiedMapManager

## Usage

### Creating Maps
1. **Tile-Based (existing):** Select tile-based toggle, create as normal
2. **GAEA Terrain:** Select GAEA toggle, enter terrain path, click load

### Loading Maps
- System automatically detects map type from save file
- Legacy saves default to tile-based
- Objects (units, items) work on both map types

### Player Maps
- **Tile-Based:** Uses existing 2D conversion
- **GAEA:** Samples terrain at configurable resolution, creates 2D representation

## Key Features

### Unified Interface
- `GetHeightAtPosition(Vector3)` - Works for both map types
- `GetTerrainTypeAtPosition(Vector3)` - Consistent terrain type detection
- `GetCurrentMapType()` - Returns active map type

### Save Compatibility
- New saves include map configuration
- Legacy saves automatically work as tile-based
- Objects save/load independently of map type

### Performance
- GAEA sampling resolution configurable (default: 64x64)
- Player maps generated once per session
- Terrain queries optimized for real-time use

## Customization

### Terrain Type Mapping
Edit `GAEATerrainManager.ConvertTextureIndexToTerrainType()` to match your GAEA terrain textures to game terrain types.

### Player Map Resolution
Adjust `sampleResolution` in `GAEAPlayerMapConverter` for quality vs performance balance.

### Visual Styling
Modify `GetColorForTerrainType()` in `GAEAPlayerMapConverter` for custom player map appearance.

## Troubleshooting

**GAEA terrain not loading:**
- Check terrain path is correct
- Ensure prefab is in Resources folder
- Verify prefab has Terrain component

**Player maps not generating:**
- Check GAEAPlayerMapConverter is assigned
- Verify TerrainTilePrefab exists in Resources
- Check console for sampling errors

**Objects not placing correctly:**
- GAEA terrain provides height sampling for proper object placement
- Existing object placement logic remains unchanged