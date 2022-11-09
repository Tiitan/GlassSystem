# Glass system

The glass system is inspired by Receiver 2: On impact, a 2D pattern is clipped on the glass surface. this list of lines is then converted to a list of polygons (shards), finally the list of polygon is used to build the new meshes and gameobjects.

Contrary to other fracture packages that breaks arbitrary 3D volumes. this package is specialized in realistically shattering 2D windows. Moreover it is focused on simulating Annealed Glass which is standard non-security glass.

There is a glass prefab ready to be shattered. The break function takes a world impact position and an origin vector. for physic, use the velocity of the incoming object, for raycast use the raycast direction, the vector magnitude is the impact strengh.


## Resources
All resources can be changed with these requirements:

### The patterns meshes
- The patterns should be 2D along the XY plane with all Z=0.
- They should use mesh topology and the lines should extend past your biggest glass diagonal length to avoid glitches.
- Each shard (mesh loop) doesn't need to be exactly convex but the Vectors angles from the loop center to each vertex should remain ordered.
- all vertices must be connected to a loop, except for the ones extending outward and expected to be clipped.

Note: Line meshes import is surprisingly not supported by unity by default, a plugin is required for that, I found several custom importers for that online, but 
I  made a blender exporter/ unity importer of my own format (https://github.com/Tiitan/BlenderTools) if you want to use it.

### The Glass mesh

- The Glass should be oriented on the XY plane and can be any thickness. 
- The mesh origin should be at the center along Z axis (vertex Z coordinate sign is used to guess the sides!)
- Again, not exactly convex but vectors angles to origin must remain ordered
- Scalling (on XY plane) is supported
- It must contain 2 submeshes (material). The first submesh must be the main surface and the second submesh should be the edges

### Materials
They can be anything you want, but for the glass edge shader, if you want to disable backface culling, your vertex shader should push the position a little inward along normal to avoid Z fighting.

## dependencies

### Math.net Spatial
https://spatial.mathdotnet.com/

It is included in this package and is on (MIT/X11) license. You can also get the latest version on NuGet (https://www.nuget.org/packages/MathNet.Spatial).

## Troubleshooting

TODO

## Limitation

This package focus on annealed glass. It would be very inneficient to make a Tempered glass pattern (The thousand of small pebbles kind), because each shard geometry is built individually and their meshes are unique. It is better to create a particle effect for this kind of glass. 

It is also a bad choice for laminated glass (The kind that breaks without falling, like tesla's cybertruck), here a shader would be more suited, with a dynamic additive impact texture. because it is more efficient and because break lines usually overlap on laminated glass wichi is impossible with this package.

## Contact 
Contact me on discord Titan#8190 if you have any question.
