# Glass system
This glass shatter system simulate window fracture with hand drawn impact patterns.
it is inspired by Receiver 2's algorithms: On impact, a 2D pattern is clipped on the glass surface. this list of lines is then converted to a list of polygons (shards), finally the list of polygon is used to build the new meshes and game objects.
Contrary to other fracture packages that allows to breaks arbitrary 3D volumes, this package is specialized in realistically simulating two dimensional windows. Moreover it focuses on simulating Annealed Glass which is standard non-security glass.
The hand drawn nature of the fracture pattern provide 2 major benefit over a procedural approach with no real drawback:
- You keep your full creative freedom over the style of fracture you wish to see, to match the style of your project contrary to a simple procedural approach that lacks details and personality.
- The shard generation stays very lightweight and mobile friendly compared to a physics based fracture method.
- By rotating the patterns, only a handful of fracture meshes are enough to keep the illusion of randomness.

In this released package, there is a glass prefab ready to be shattered. The "break" function just takes a world impact position and an origin direction vector. For physic impact, you should just use the velocity and colision point of the incoming object, for raycast use the raycast direction, the vector magnitude is the impact strength.

This package focuses on annealed glass. It would be very inefficient to make a Tempered glass pattern (The thousand of small pebbles kind), because each shard geometry is built individually and their meshes are unique. It is better to create a particle effect that will use instancing for this kind of glass.
It is also a bad choice for laminated glass (The kind that breaks without falling, like tesla's cybertruck), here a simple shader would be more suited, with a dynamic additive impact texture. because it is more efficient and because break lines overlap on laminated glass which is impossible with this package.

## Features
- The glass shards can recursively break.
- The glass transform can be non-uniformly scaled and have any thickness for convenient level design.
- randomized array of hand-crafted fracture pattern, randomly rotated.
- Glass mesh of Any convex shape can be used
- UV0 is propagated to the shards to allow consistent textures.
- Networking ready: randomness optional (rotation value and pattern index can be passed in)
- The shard velocity is inherited from it's parent when it detach.
- Glass corners stay attached to their frame, while the interconnected graph of shard may trigger a fall cascade (Premium version, WIP).

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
- While importing in unity, "Read/Write" option must be enabled

### Materials
They can be anything you want, but for the glass edge shader, if you want to disable backface culling, your vertex shader should push the position a little inward along normal to avoid Z fighting.
The edge material shouldn't use UV.

## dependencies

### Math.net Spatial
https://spatial.mathdotnet.com/

It is included in the released package and is on (MIT/X11) license. You can also get the latest version on NuGet (https://www.nuget.org/packages/MathNet.Spatial).

## Troubleshooting

TODO

## Future
Future features will be developped for a paid package version, but remain open source here in the "premium" branch, see licence.
### Glass shard graph logic
Each shard will be linked to its neighbors with a bidirectional graph. This will enable more realistic physic (no more flying shards) and advanced gameplay logic (shards starts falling after X impact). Do not expect it sooner than end of 2023, I have other priorities to finish first.
### Long term possibilites (no promises)
- Temperated glass particle system
- Laminated glass shader
- In-engine glass pattern editor
### User request
I can implement small features to remove some limitations if they appear to be a deal breaker for a lot of users,
such as supporting additional UVs, concave glass panels, etc

## Known issues
none, all fixed :)

## Licence
The packaged version of this tool is also available on the unity asset store for free under the store's [EULA](https://unity.com/legal/as-terms?utm_source=google&utm_medium=cpc&utm_campaign=cc_dd_upr_emea_emea_en_aw_dsp-gg_acq_w-rt_2023-03_pmax-mofu_cc3022_mofu-dd&utm_content=&utm_term=&gclid=Cj0KCQjw8qmhBhClARIsANAtbocYuqb7OnPFyv-M6r0zf9QDIOIwoJQN0s3nu9VISXDfI_9XZJarkJIaAjNGEALw_wcB&gclsrc=aw.ds). Premium features will be released in a paid version and will requires you to buy the amount and type of licence in accordance to the store's EULA requirements.
The whole source code is open for consultation and documentation purposes but the "premium" branch shouldn't be used in a production environement without the adequate unity store licence.

## Contact 
Contact me on discord Titan#8190 if you have any question.

You can also create a github issue https://github.com/Tiitan/GlassSystem/issues
