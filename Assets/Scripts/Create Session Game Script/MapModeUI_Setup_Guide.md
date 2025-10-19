# Map Mode UI Setup Guide

## Required UI Structure

Create this UI hierarchy in your Create Map scene:

```
Canvas
├── MapModeButton (Button)
│   └── Text: "Map Mode"
├── MapModePopup (Panel) - Initially inactive
│   ├── Background (Image)
│   ├── RegularMapButton (Button)
│   │   └── Text: "Regular Map Mode"
│   └── GAEAMapButton (Button)
│       └── Text: "GAEA Map Mode"
├── ImportMapButton (Button) - Initially inactive
│   └── Text: "Import Map"
├── ImportPopup (Panel) - Initially inactive
│   ├── Background (Image)
│   ├── ImportImageButton (Button)
│   │   └── Text: "Import Image"
│   ├── ImportObjButton (Button)
│   │   └── Text: "Import .obj"
│   └── ConfirmImportButton (Button)
│       └── Text: "Confirm"
├── RegularMapUI (Panel) - Contains existing tile-based UI
│   └── [Your existing tile creation UI]
└── GAEAMapUI (Panel) - Contains GAEA-specific UI
    └── [GAEA-specific controls]
```

## Component Setup

### MapModeManager Component
Attach to a GameObject and assign:

**UI Elements:**
- Map Mode Button → MapModeButton
- Map Mode Popup → MapModePopup
- Regular Map Button → RegularMapButton
- Gaea Map Button → GAEAMapButton
- Import Map Button → ImportMapButton
- Import Popup → ImportPopup
- Import Image Button → ImportImageButton
- Import Obj Button → ImportObjButton
- Confirm Import Button → ConfirmImportButton

**UI Panels:**
- Regular Map UI → RegularMapUI (panel with existing UI)
- Gaea Map UI → GAEAMapUI (panel with GAEA UI)

**Map Components:**
- Regular Map Manager → GameMasterMapManager
- Camera Mover → CameraMover

## Default State
- Scene starts in GAEA Map Mode
- ImportMapButton is active only in GAEA mode
- RegularMapUI is hidden, GAEAMapUI is visible

## Workflow
1. **Map Mode Button** → Opens mode selection popup
2. **Regular Map Mode** → Switches immediately, loads tiles
3. **GAEA Map Mode** → Switches to GAEA, shows import button
4. **Import Map** → Opens import popup
5. **Import Image + Import .obj + Confirm** → Creates GAEA map

## Save Integration
- Both map modes are saved simultaneously
- Current mode is remembered
- Switching modes preserves progress temporarily