# GAEA-Only Map Creation Setup Guide

## Required UI Structure

Create this simple UI in your Create Map scene:

```
Canvas
├── ImportMapButton (Button)
│   └── Text: "Import Map"
├── ImportPopup (Panel) - Initially inactive
│   ├── Background (Image)
│   ├── Title (Text): "Import GAEA Map"
│   ├── ImportImageButton (Button)
│   │   └── Text: "Import Image"
│   ├── ImportObjButton (Button)
│   │   └── Text: "Import .obj"
│   ├── ConfirmImportButton (Button)
│   │   └── Text: "Confirm"
│   └── StatusText (Text)
│       └── Text: "Select image and 3D object files"
└── [Your existing object placement UI]
```

## Component Setup

### GAEAMapCreator Component
Attach to a GameObject and assign:

**UI Elements:**
- Import Map Button → ImportMapButton
- Import Popup → ImportPopup
- Import Image Button → ImportImageButton
- Import Obj Button → ImportObjButton
- Confirm Import Button → ConfirmImportButton
- Status Text → StatusText

**Map Components:**
- Camera Mover → CameraMover (for auto-scaling)

### GAEAMapLoader Component (Optional)
For loading saved maps:
- Map Creator → GAEAMapCreator

## Workflow

1. **Import Map Button** → Opens import popup
2. **Import Image** → Select PNG/JPG texture file
3. **Import .obj** → Select 3D object file
4. **Confirm** → Creates map with texture applied
5. **Auto-scaling** → Map fits camera boundaries
6. **Save/Load** → Map data preserved in save files

## Features

✅ **Simple Import** - Just image + 3D object + confirm
✅ **Auto-centering** - Maps positioned at world origin
✅ **Auto-scaling** - Maps fit camera view automatically
✅ **File Validation** - Checks file existence before loading
✅ **Status Updates** - User feedback throughout process
✅ **Save Integration** - Works with existing save system

## File Requirements

- **Images**: PNG, JPG, JPEG formats
- **3D Objects**: .obj files with proper mesh data
- Files are loaded directly (not copied to project)

This simplified system removes all tile-based functionality while keeping the existing object placement system intact.