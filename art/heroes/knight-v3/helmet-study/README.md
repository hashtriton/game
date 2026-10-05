# Knight v3 helmet study

Original local model, 2026-10-05. Blender 5.2.2 LTS, meters, +Z up, front -Y.

`v1/` preserves the first six-volume blockout and its four clay renders. The current `blockout.blend` and unprefixed PNG files show the refined larger surfaces: V-shaped brow, wedge nose, folded cheek volume and narrowed chin. `detailed.blend` and `detailed-*.png` add fitted bronze brow/chin edging and six actual cheek breathing openings with recessed dark backing. No downloaded assets are used.

Only `detailed.blend`, the four `detailed-*.png` views and this README are included in Git. The earlier blockouts, unprefixed previews, local check script and logs are local history; the complete character has a repository validator in `tools/knight_validate.py`.

Builder: `tools/knight_helmet.py`, `build_helmet(collection, materials, detailed=False)`. The materials dictionary supplies steel, bronze, dark and edge. It returns only new objects in the supplied collection. Import does not mutate the scene. The CLI creates the standalone study only in background mode. The owning coordinator places the local helmet onto the body.

Verified locally through `Blender --background --factory-startup --disable-autoexec --offline-mode --python .local/knight-v3/helmet/check.py` with and without `-- --detailed`:

- Blockout: 6 objects, 13,632 evaluated triangles.
- Detailed: 14 objects, 14,366 evaluated triangles.
- Evaluated size: 0.2704 x 0.3219 x 0.3550 m including low crown rib.
- Detailed Z range: -0.1540 to +0.2010 m.
- Both modes: zero evaluated triangles with area below 1e-12 m2.
- Real front, profile, three-quarter and back renders generated and visually reviewed.

The unnecessary bevels on the dark eye recess and crown rib were removed. The mask's original smooth surface normals are transferred onto its exterior after the local Boolean breathing cuts; newly exposed interior walls retain their own normals. This prevents triangular shading pinches while preserving physical cuts.

This is a static visual component. Full-body integration is checked by the coordinator; game UV, retopology, rigging, animation and Unity export are outside this study.
