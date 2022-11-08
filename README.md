# Glass system

The glass system is inspired by Receiver 2: On impact, a 2D pattern is clipped on the glass surface. this list of lines is then converted to a list of polygons (shards), finally the list of polygon is used to build the new meshes and gameobjects.

Contrary to other fracture packages that breaks arbitrary 3D volumes. this package is specialized in realistically shattering 2D windows.

There is a glass prefab ready to be shattered. The break function takes a world impact position and an origin vector. for physic, use the velocity of the incoming object, for raycast use the raycast direction, the vector magnitude is the impact strengh.


## Resources
All resources can be changed with these requirements:

### The patterns meshes
- The patterns should be 2D along the XY plane with all Z=0.
- They should use mesh topology and the lines should extend past your biggest glass diagonal length to avoid glitches.
- Each shard (mesh loop) doesn't need to be exactly concave but the Vectors angles from the loop center to each vertex should remain ordered.  

### The Glass mesh

- The Glass should be oriented on the XY plane and can be any thickness. 
- The mesh origin should be at the center along Z axis (vertex Z coordinate sign is used to guess the sides!)
- Again, not exactly concave but vectors angles to origin must remain ordered
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

## Contact 
Contact me on discord Titan#8190 if you have any question.
