# Art Direction Brief: WickFall

## Game frame

- **Player fantasy**: A fragile, brave candle golem traversing an oppressive, shadowy gothic dungeon. Its molten wax is both its health pool and its only source of light.
- **Core verbs**: Move (4 directions), Attack (ember blade swing), Collect (wax shards, wick bundles), Kindle/Rest (brazier checkpoint/recharge), Survive (manage wax consumption & avoid shadow stalkers).
- **Engine and renderer**: Unity 6 (6000.3.8f1), 2D Sprite Renderer with Pixel-perfect/Point filtering, 2D Physics.
- **Target platforms**: Windows PC Standalone.
- **Camera/view/facing**: Top-Down 2D Action RPG with 4-directional character orientation (South, North, East, West).
- **Native viewport and common display scale**: 1920x1080 display target, orthographic camera, 1:1 pixel scaling.
- **Typical asset size on screen**:
  - Player Candle Golem: ~32x56 to 48x64 px
  - Enemies (Thralls, Guardian, Wisp): 32x32 to 64x64 px
  - Dungeon Tiles (Floors, Walls): 80x80 px modular grid
  - Props (Braziers, Gates, Altars, Crates): 48x48 to 96x96 px
  - UI Icons & Bars: 32x32 relics, 160x48 buttons, 8-stage candle health indicator

## Visual system

- **Shape language**: Gothic archways, melting candle wax stalactites and drips, flickering jagged flames, jagged wrought-iron portcullis bars, heavy ashlar masonry.
- **Silhouette priorities**: Strong high-contrast readability. Glowing candle heads and flaming blades stand out against pitch-black dungeons and dark shadowy creatures.
- **Value structure**:
  - Background (Floor/Walls): Low-key values (15% - 35% luminance), slate stone, dark mortar.
  - Enemies: Silhouette values (5% - 20% luminance) with piercing amber eye points.
  - Interactive / Pickups: High-visibility warm golds and embers (70% - 95% luminance).
  - Player: Mid-to-high value wax body topped with a radiant flame core.
- **Palette roles and exact swatches**:
  - Wax / Golem Body: Ivory/cream `#F2EAD8`, warm tan `#D3C3A3`, shaded umber `#7D6B50`, outline `#2C2219`.
  - Flame / Ember: Core white `#FFFDF0`, bright gold `#FFD13B`, fiery orange `#F37318`, deep ember `#A82200`.
  - Dungeon Masonry: Dark granite `#423E3B`, slate `#5C5652`, highlight mortar `#7D7570`, deep shadow `#1B1918`.
  - Elemental Flames:
    - Default Ember: `#FF7F11`
    - Azure Spirit: `#00B4D8`
    - Jade Flame: `#2EC4B6`
    - Pure Soul: `#F7F7F7`
    - Crimson Fury: `#D90429`
- **Materials and surface cues**:
  - Molten wax (smooth drips, rounded volume, glossy highlights).
  - Rough-hewn stone (chiseled edges, mortar lines, cracks).
  - Ironwork (riveted straps, wrought spikes, oxidized dark metal).
- **Edge/line treatment**: Deliberate 1px dark contour outlines for all characters and dynamic props. Clean pixel clustering with no bilinear fuzziness.
- **Lighting direction and contrast**: Dynamic point lights centered at the player's candle wick and static light sources (braziers, torches). Radial falloff into dark fog-of-war.
- **Detail density and focal hierarchy**: Highest detail and contrast on the player and interactive braziers. Low noise on dungeon floor tiles to maintain high gameplay clarity.
- **Motion character**: Rhythmic, slightly heavy waddling walk cycle for the candle golem; erratic twitchy scurrying for Shadow Thralls; smooth hovering bob for the Wisp.
- **Explicit exclusions**: Modern UI elements, sci-fi shapes, flat vector gradients, anti-aliased soft blur edges.

## Technical contract

- **Asset dimensions/aspect**:
  - Characters: 64x64 px canvas standard
  - Enemies: 64x64 px (Thrall), 96x96 px (Melting Guardian), 48x48 px (Wisp)
  - Tiles: 80x80 px modular tileset
  - UI Buttons: 160x48 px
  - Health Candle: 32x64 px
- **Alpha/background**: Fully transparent RGBA PNGs (no baked background, no halos).
- **Anchor/pivot/baseline**:
  - Characters & Enemies: Bottom-Center `(0.5, 0.0)` or `(0.5, 0.1)` for ground contact.
  - Tiles: Center `(0.5, 0.5)` or Bottom-Left `(0.0, 0.0)`.
  - Props: Bottom-Center `(0.5, 0.0)`.
  - UI: Center `(0.5, 0.5)`.
- **Filtering/mipmaps/compression**:
  - Filter Mode: `Point (no filter)`
  - Texture Compression: `None` / `RGBA 32 bit`
  - Mipmaps: `Disabled`
  - Pixels Per Unit (PPU): `64` or `80` (uniform world scale)
- **Naming and folders**:
  - `Assets/Sprites/Characters/`
  - `Assets/Sprites/Enemies/`
  - `Assets/Sprites/Environment/`
  - `Assets/Sprites/Props/`
  - `Assets/Sprites/Items/`
  - `Assets/Sprites/UI/`
  - `Assets/Sprites/Lighting/`
