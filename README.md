# To Go Anywhere

Personal unity project with infinite procedural generation and instanced rendering

![Procedural Terrain](https://i.ibb.co/C6Sjqfy/Terrain.png)

## Procedural Generation
- Terrain can be generated infinitely in all directions
- It generates on the fly in camera view on separate threads, allowing for smooth generation with no hitching
- Generation is efficient enough to allow for fast camera panning without ever seeing ungenerated chunks, even on low-end systems
- Procedural generation parameters are fully customizable in the editor
- Biomes are supported as sub-generators, which can be nested, allowing for local biomes
- Additionally, A* pathfinding is implemented, which supports finding road paths which are optimized for minimal height differences, as well as generating flowing rivers in the future.
- Pathfinding also works asynchronously, paths are calculated in the background without blocking the main thread

## Instanced Rendering
- All tiles and resources are rendered as instanced meshes, passed to the GPU as chunks
- Both tiles and resources can be animated in shader

## Gameplay
In the current state, gameplay only has a basic implementation, only allowing player to build roads and place basic buildings.
UI currently exists as a prototype for debugging, utilizing UGUI. When gameplay features and general direction are more established, I plan to move the UI to UIToolkit to utilize data binding and better scalability.

Core game loop is planned to focus on gathering resources and building a road to the other side of a locked area of the map, while building and managing small colonies as well as transporting the resources between them.

![Procedural Terrain](https://i.ibb.co/MnVG7xD/Mountains.png) 
![Animated Water](https://raw.githubusercontent.com/petepi3/ToGoAnywhere/main/Water.gif) 
![Procedural Terrain](https://github.com/petepi3/ToGoAnywhere/blob/main/Trees.gif?raw=true) 
![Procedural Terrain](https://i.ibb.co/nsWh2G4/Village.png) 
