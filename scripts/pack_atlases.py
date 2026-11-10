import os
import uuid
from PIL import Image

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS_DIR = os.path.join(BASE_DIR, "Assets", "Sprites")
CHAR_DIR = os.path.join(ASSETS_DIR, "Characters")
ENEMY_DIR = os.path.join(ASSETS_DIR, "Enemies")

def pack_player_atlas():
    # 7 columns, 3 rows, cell size 64x72
    cw, ch = 64, 72
    atlas = Image.new("RGBA", (cw * 7, ch * 3), (0, 0, 0, 0))
    
    rows = [
        ("down", [
            "candle_golem_walk_down_0.png", "candle_golem_walk_down_1.png",
            "candle_golem_walk_down_2.png", "candle_golem_walk_down_3.png",
            "candle_golem_idle_down.png", "candle_golem_attack_down_0.png",
            "candle_golem_attack_down_1.png"
        ]),
        ("side", [
            "candle_golem_walk_side_0.png", "candle_golem_walk_side_1.png",
            "candle_golem_walk_side_2.png", "candle_golem_walk_side_3.png",
            "candle_golem_idle_side.png", "candle_golem_attack_side_0.png",
            "candle_golem_attack_side_1.png"
        ]),
        ("up", [
            "candle_golem_walk_up_0.png", "candle_golem_walk_up_1.png",
            "candle_golem_walk_up_2.png", "candle_golem_walk_up_0.png",
            "candle_golem_idle_up.png", "candle_golem_attack_up.png",
            "candle_golem_attack_up.png"
        ]),
    ]
    
    sprites_meta = []
    
    for r_idx, (r_name, fnames) in enumerate(rows):
        for c_idx, fname in enumerate(fnames):
            fpath = os.path.join(CHAR_DIR, fname)
            if os.path.exists(fpath):
                img = Image.open(fpath)
                atlas.paste(img, (c_idx * cw, r_idx * ch), img)
                
            # In Unity, texture origin Y is at bottom
            unity_y = (2 - r_idx) * ch
            sprite_name = f"{r_name}_{c_idx}"
            s_meta = f"""    - serializedVersion: 2
      name: {sprite_name}
      rect:
        serializedVersion: 2
        x: {c_idx * cw}
        y: {unity_y}
        width: {cw}
        height: {ch}
      alignment: 7
      pivot: {{x: 0.5, y: 0.1}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: {uuid.uuid4().hex}
      internalID: {1000 + r_idx * 10 + c_idx}
      vertices: []
      indices: 
      edges: []
      weights: []"""
            sprites_meta.append(s_meta)
            
    out_path = os.path.join(CHAR_DIR, "candle_golem_atlas.png")
    atlas.save(out_path)
    
    # Write Multiple Sprite .meta
    guid = uuid.uuid4().hex
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  textureSettings:
    filterMode: 0
    aniso: 1
    wrapU: 1
    wrapV: 1
  spriteMode: 2
  spritePixelsToUnits: 64
  alphaIsTransparency: 1
  textureType: 8
  spriteSheet:
    serializedVersion: 2
    sprites:
{chr(10).join(sprites_meta)}
    outline: []
    physicsShape: []
    bones: []
    spriteID: {uuid.uuid4().hex}
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
"""
    with open(out_path + ".meta", "w", encoding="utf-8") as f:
        f.write(meta_content)
    print("Player atlas packed successfully:", atlas.size)

if __name__ == "__main__":
    pack_player_atlas()
