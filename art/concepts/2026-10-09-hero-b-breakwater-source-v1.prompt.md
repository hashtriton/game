# hero_b

Дата: 2026-10-09. Tool: built-in image_gen.imagegen. Use case: stylized-concept.
Новая генерация, без входных изображений.

## Exact prompt, attempt 1

```text
Use case: stylized-concept. Generate an original premium stylized 3D fantasy CHARACTER DECISION SHEET, landscape 1920x1080 native pixels requested. Warm daylight, cool soft shadows, sculpted believable anatomy, forged steel and woven fabric with restrained wear, clean large forms. No toy-like armor, no chibi, no plastic, no flat vector, no robotic human face, no franchise shapes, logos, emblems, characters or copied outfits. Pale blue-gray paper background. High quality art direction, calm neutral combat-ready stance. A consistent single character repeated accurately in all views.
Layout: upper left at x80..720 a LARGE full-body front three-quarter render with both feet and entire weapon visible. Upper right at x1000..1560 an enlarged oblique top-down 56 degrees camera reference. Along BOTTOM from y800 to1040, leave lots of empty space around six SMALL cutouts. First three are COLOR oblique TOP-DOWN 56 DEGREES game-camera cutouts, NOT front elevation on blue-gray paving at actual total character heights 110px, 135px, 160px (roughly 11.7%,14.3%,17% of a 941px high output) including weapon, not enlarged thumbnails; label each "110 px", "135 px", "160 px". Next three are pure BLACK silhouettes of those identical game-camera views, also total heights 110px,135px,160px; no gray shading, no internal details, no shadow. Large title upper left, section labels "3/4 FRONT", "CAMERA 56", "GAME SIZE / 1:1", "BLACK SILHOUETTE / 1:1". Clear editorial spacing. No extra characters, no props background, no stats, no invented measurement rulers. Silhouettes and game views must match the large design.
Title "B / BREAKWATER".
An imposing adult male hero in a completely CLOSED steel helmet with a low horizontal visor, no glowing robotic eye slit. 2.4m tall, broad shoulders, compact head sunk into collar, substantial barrel chest, heavy believable interlocking ivory and blue steel plates. Deliberately no cape, no horn, no spikes, no excessive ornamental trim. LEFT hand carries a large upright tapered trapezoid shield with rounded top corners, physically strapped, ivory with one thick cyan central seam. RIGHT hand holds a short heavy straight single-edged sword with broad clipped tip, length 0.85m, thick plain angular guard. Short thick dark fabric skirt not past upper thigh. A strong squat fortress silhouette: shield and raised shoulder form one tall left edge, sword short and clearly separated on the right. Cool cyan/white hero cues.
All 3 small color views AND all 3 black silhouettes MUST show the SAME 56 degree oblique TOP-DOWN view, see tops of shoulders and head, shortened legs in projection. Never use front standing elevation for the bottom row. Keep their top-to-bottom bounding boxes close to 110/135/160 native pixels, do not fill the bottom space.
```

Повторы и проверка: .local/codex-tasks/chars/progress.md.


## Exact prompt, attempt 2 (one retry)

Input image: 2026-10-09-hero-b-breakwater-v1.png attempt 1.

```text
Use case: precise-object-edit / stylized-concept. Edit this decision sheet. Fix the worst defect: excessive tiny metal chips and scratches make the armor photographic and busy. Replace gritty surface texture with clean large sculpted forged planes and very restrained edge wear, more premium stylized game art, without altering construction or character silhouette. Preserve all views, title and white/cyan cues. In the upper-right camera and all bottom views swing the left shield 10 degrees outward to give visible separation from left boot. The black silhouettes must match. Keep full-body 3/4 front, enlarged 56 degree camera, and bottom six cutouts: 3 game-camera color views and three pure black silhouettes labelled110/135/160px. Preserve original concept only, no reference to other characters. Do not introduce new panels or text.
```

