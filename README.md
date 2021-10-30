# HaloWarsTools
WIP library to read Halo Wars Definitive Edition files and convert them to common formats for exporting to game engines, 3d modeling software, etc. Based off the binary templates in the [HaloWarsDocs](https://github.com/HaloMods/HaloWarsDocs) repository.

## Supported File Formats
* `.xtd` - Terrain data
  * Mesh
  * Ambient Occlusion map
  * Opacity map
* `.xtt` - Terrain data
  * Albedo map
* `.ugx` - Mesh data
  * Vertex positions/normals/texcoords
  * Materials (limited support)
  * Submeshes
  * Texture paths
* `.vis` - Visual represention
  * Meshes
* `.gls` - Lighting?
  * Sun inclination/rotation/color
  * Background color
* `.scn`/`.sc2`/`.sc3` - Scenario data
  * (No support yet)

## How To Use
1. Download [PhxTools](https://github.com/HaloMods/HaloWarsDocs/releases)
2. Run `PhxGui.exe`
3. Set `ERA Expand Path` to where you want to store extracted game files
4. Drag `.era` files from your HaloWarsDE folder into PhxGui to extract them
5. Clone this repository or download it as a `.zip`
6. Open the `.sln`, change the input/output paths, and run the example

## Notes
* Textures not mentioned above are stored in various DDS formats and will need to be manually converted for now.
* `.vis`, `.gls`, `.scn`, `.sc2`, and `.sc3` are all stored as binary XML files (`.xmb`) and will need to be manually converted by dragging them into PhxGui for now.
