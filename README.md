# HaloWarsTools
WIP library to read Halo Wars Definitive Edition files and convert them to common formats for exporting to game engines, 3d modeling software, etc. Based off the binary templates in the [HaloWarsDocs](https://github.com/HaloMods/HaloWarsDocs) repository.

## Supported File Formats
* `.xtd` - Terrain visual data
  * Mesh
  * Ambient Occlusion map
  * Opacity map
* `.xtt` - Terrain texture data
  * Albedo map
* `.ugx` - Universal geometry data
  * Vertex positions/normals/texcoords
  * Materials (limited support)
  * Submeshes
  * Texture paths
* `.vis` - Visual data
  * Meshes
* `.gls` - Global light data
  * Sun inclination/rotation/color
  * Background color
* `.scn` - Main scenario data
  * (No support yet)
* `.sc2` - Scenario decoration objects
  * (No support yet)
* `.sc3` - Scenario sound objects
  * (No support yet)

## How To Use
1. Download [PhxTools](https://github.com/HaloMods/HaloWarsDocs/releases)
2. Run `PhxGui.exe`
3. Set `ERA Expand Path` to where you want to store extracted game files
4. Drag `.era` files from your HaloWarsDE folder into PhxGui to extract them
5. Clone this repository or download it as a `.zip`
6. Open the `.sln`, change the input/output paths, and run the example

## Notes
* You will need to clone the HaloWarsDocs repository into the same directory and build PhxTools.sln for automatic `.xmb` extraction to work. Here's what Visual Studio components you need to build that solution:
  * ![Requirements](https://raw.githubusercontent.com/srogee/HaloWarsTools/main/Requirements.png?token=ABIQA63R5UYXEVYD7K7SLWDBRBMPW)
* Textures not mentioned above are stored in various DDS formats and will need to be manually converted for now.
