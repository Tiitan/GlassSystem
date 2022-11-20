# Glass system

The glass system is inspired by Receiver 2: On impact, a 2D pattern is clipped on the glass surface. this list of lines is then converted to a list of polygons (shards), finally the list of polygon is used to build the new meshes and game objects.

Contrary to other fracture packages that breaks arbitrary 3D volumes. this package is specialized in realistically shattering 2D windows. Moreover it is focused on simulating Annealed Glass which is standard non-security glass.

There is a glass prefab ready to be shattered. The break function takes a world impact position and an origin vector. for physic, use the velocity of the incoming object, for raycast use the raycast direction, the vector magnitude is the impact strength.

This package focus on annealed glass. It would be very inefficient to make a Tempered glass pattern (The thousand of small pebbles kind), because each shard geometry is built individually and their meshes are unique. It is better to create a particle effect that will use instancing for this kind of glass.
It is also a bad choice for laminated glass (The kind that breaks without falling, like tesla's cybertruck), here a simple shader would be more suited, with a dynamic additive impact texture. because it is more efficient and because break lines overlap on laminated glass which is impossible with this package.

## Features
- The glass shards can recursively break.
- The glass transform can be scaled and have any thickness for convenient level design.
- randomized array of hand-crafted fracture pattern, randomly rotated.
- UV0 is propagated to the shards to allow consistent textures.
- The shard velocity is inherited from it's parent when it detach (TODO).
- Glass corners stay attached to their frame, while the interconnected graph of shard may trigger a fall cascade (TODO).

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
- Again, not exactly convex but vectors angles to origin must remain ordered, all vertices must be on the side, no vertices can be on the surface
- Scaling (on XY plane) is supported
- It must contain 2 sub-meshes (material). The first sub-mesh must be the main surface and the second sub-mesh should be the edges
- UV coordinate must be mirrored on backside to match front side, only UV0 is propagated, and uvs must be regular (arbitrary triangle selected for the barycentric interpolation).

### Materials
They can be anything you want, but for the glass edge shader, if you want to disable backface culling, your vertex shader should push the position a little inward along normal to avoid Z fighting.
The edge material shouldn't use UV.

## dependencies

### Math.net Spatial
https://spatial.mathdotnet.com/

It is included in the released package and is on (MIT/X11) license. You can also get the latest version on NuGet (https://www.nuget.org/packages/MathNet.Spatial).

## Troubleshooting

TODO

## Known issues
- Sometimes the shard algorithm fails to build the meshes from the pattern, for a reason still unknown. The demo "gun" contains a retry for that reason.
- If the glass panel is inclined, the normals on some shard may be inverted. normals are currently automatically calculated by unity, passing them manually during mesh creation will solve this (todo)

## Licence
This package is for sale on the Unity asset store, if you got it from github and if you plan on using it, you will need to buy the amount and type of licence required by the asset store's EULA. Thank you for your support.

## Contact 
Contact me on discord Titan#8190 if you have any question.

You can also create a github issue https://github.com/Tiitan/GlassSystem/issues
