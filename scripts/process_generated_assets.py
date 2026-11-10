import os
import uuid
from PIL import Image
import numpy as np
from collections import deque

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BRAIN_DIR = r"C:\Users\yasso\.gemini\antigravity\brain\89c321ff-98a1-45c3-9609-bf034bac36cc"
ASSETS_DIR = os.path.join(BASE_DIR, "Assets", "Sprites")

# Source paths
HERO_FINAL_PATH = os.path.join(BRAIN_DIR, "wickfall_hero_candle_golem_final_1793782273682.jpg")
HERO_SHEET_PATH = os.path.join(BRAIN_DIR, "candle_golem_spritesheet_1793782344118.jpg")
THARLL_PATH = os.path.join(BRAIN_DIR, "wickfall_shadow_thrall_1793782290539.jpg")
BRAZIER_PATH = os.path.join(BRAIN_DIR, "brazier_wax_shards_spritesheet_1793782259499.jpg")
TILES_PATH = os.path.join(BRAIN_DIR, "wickfall_dungeon_tiles_1793782288663.jpg")
UI_PATH = os.path.join(BRAIN_DIR, "wickfall_ui_assetsheet_1793782354196.jpg")
TITLE_PATH = os.path.join(BRAIN_DIR, "wickfall_title_screen_1793782278836.jpg")

def make_meta(guid, ppu=64, pivot_y=0.5, pivot_x=0.5, sprite_mode=1):
    return f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    wrapU: 1
    wrapV: 1
  nPOTScale: 0
  compressionQuality: 50
  spriteMode: {sprite_mode}
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: {pivot_x}, y: {pivot_y}}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
"""

def extract_black_bg(crop_img, threshold=20, feather=40):
    img = crop_img.convert("RGBA")
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    lum = np.mean(arr[:, :, :3], axis=2)
    
    mask_bg = np.zeros((h, w), dtype=bool)
    q = deque()
    
    for y in range(h):
        for x in [0, w-1]:
            if lum[y, x] < threshold and not mask_bg[y, x]:
                mask_bg[y, x] = True
                q.append((y, x))
    for x in range(w):
        for y in [0, h-1]:
            if lum[y, x] < threshold and not mask_bg[y, x]:
                mask_bg[y, x] = True
                q.append((y, x))
                
    while q:
        cy, cx = q.popleft()
        for dy, dx in [(-1,0), (1,0), (0,-1), (0,1)]:
            ny, nx = cy + dy, cx + dx
            if 0 <= ny < h and 0 <= nx < w:
                if lum[ny, nx] < threshold and not mask_bg[ny, nx]:
                    mask_bg[ny, nx] = True
                    q.append((ny, nx))
                    
    alpha = np.ones((h, w), dtype=float) * 255.0
    alpha[mask_bg] = 0.0
    
    # Boundary feathering
    for y in range(h):
        for x in range(w):
            if alpha[y, x] > 0 and lum[y, x] < feather:
                is_adj = False
                for dy, dx in [(-1,0), (1,0), (0,-1), (0,1)]:
                    ny, nx = y+dy, x+dx
                    if 0 <= ny < h and 0 <= nx < w and alpha[ny, nx] == 0:
                        is_adj = True
                        break
                if is_adj:
                    factor = (lum[y, x] - threshold) / (feather - threshold)
                    if factor <= 0.15:
                        alpha[y, x] = 0.0
                    else:
                        alpha[y, x] = 255.0 * factor

    arr[:, :, 3] = np.clip(alpha, 0, 255)
    return Image.fromarray(arr.astype(np.uint8))

def save_sprite(img, dest_dir, filename, ppu=64, pivot_y=0.5, pivot_x=0.5, target_size=None):
    os.makedirs(dest_dir, exist_ok=True)
    if target_size:
        tw, th = target_size
        cw, ch = img.size
        # Rescale proportionally to fit target_size
        scale = min(tw / cw, th / ch)
        nw, nh = max(1, int(cw * scale)), max(1, int(ch * scale))
        resized = img.resize((nw, nh), Image.Resampling.NEAREST)
        canvas = Image.new("RGBA", (tw, th), (0, 0, 0, 0))
        px = (tw - nw) // 2
        py = th - nh # bottom alignment
        canvas.paste(resized, (px, py), resized)
        img = canvas

    out_path = os.path.join(dest_dir, filename)
    img.save(out_path)
    meta_path = out_path + ".meta"
    if not os.path.exists(meta_path):
        guid = uuid.uuid4().hex
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(make_meta(guid, ppu=ppu, pivot_y=pivot_y, pivot_x=pivot_x))
    print(f"Saved: {filename} ({img.size}) to {os.path.basename(dest_dir)}")
    return img

def process_all():
    print("--- PROCESSING GENERATED WICKFALL ASSETS ---")
    
    # ----------------------------------------------------
    # 1. HERO ANIMATIONS & SPRITES
    # ----------------------------------------------------
    char_dir = os.path.join(ASSETS_DIR, "Characters")
    hero_sheet = Image.open(HERO_SHEET_PATH)
    
    # Hero Sheet rows:
    # Row 0: Walk Down (Y: 20..246)
    down_cols = [(61, 195), (322, 446), (573, 707), (834, 958)]
    for idx, (x1, x2) in enumerate(down_cols):
        crop = hero_sheet.crop((x1, 20, x2, 246))
        clean = extract_black_bg(crop)
        save_sprite(clean, char_dir, f"candle_golem_walk_down_{idx}.png", ppu=64, pivot_y=0.1, target_size=(64, 72))
    # Idle Down = Walk frame 0
    save_sprite(extract_black_bg(hero_sheet.crop((61, 20, 195, 246))), char_dir, "candle_golem_idle_down.png", ppu=64, pivot_y=0.1, target_size=(64, 72))

    # Row 2: Walk Side (Y: 281..502)
    side_cols = [(77, 179), (333, 435), (588, 681), (850, 947)]
    for idx, (x1, x2) in enumerate(side_cols):
        crop = hero_sheet.crop((x1, 281, x2, 502))
        clean = extract_black_bg(crop)
        save_sprite(clean, char_dir, f"candle_golem_walk_side_{idx}.png", ppu=64, pivot_y=0.1, target_size=(64, 72))
    save_sprite(extract_black_bg(hero_sheet.crop((77, 281, 179, 502))), char_dir, "candle_golem_idle_side.png", ppu=64, pivot_y=0.1, target_size=(64, 72))

    # Row 3: Walk Up (Y: 532..758)
    up_cols = [(61, 195), (322, 451), (573, 702), (829, 963)]
    for idx, (x1, x2) in enumerate(up_cols):
        crop = hero_sheet.crop((x1, 532, x2, 758))
        clean = extract_black_bg(crop)
        save_sprite(clean, char_dir, f"candle_golem_walk_up_{idx}.png", ppu=64, pivot_y=0.1, target_size=(64, 72))
    save_sprite(extract_black_bg(hero_sheet.crop((61, 532, 195, 758))), char_dir, "candle_golem_idle_up.png", ppu=64, pivot_y=0.1, target_size=(64, 72))

    # Row 4: Attack (Y: 788..1004)
    attack_cols = [(51, 246), (266, 439), (558, 763), (819, 1004)]
    for idx, (x1, x2) in enumerate(attack_cols):
        crop = hero_sheet.crop((x1, 788, x2, 1004))
        clean = extract_black_bg(crop)
        save_sprite(clean, char_dir, f"candle_golem_attack_{idx}.png", ppu=64, pivot_y=0.1, target_size=(72, 72))

    # Hero Standalone Portrait
    hero_portrait = Image.open(HERO_FINAL_PATH).crop((143, 92, 880, 941))
    clean_portrait = extract_black_bg(hero_portrait)
    save_sprite(clean_portrait, char_dir, "candle_golem_portrait.png", ppu=64, pivot_y=0.5, target_size=(96, 96))

    # ----------------------------------------------------
    # 2. ENEMY (Shadow Thrall)
    # ----------------------------------------------------
    enemy_dir = os.path.join(ASSETS_DIR, "Enemies")
    thrall_im = Image.open(THARLL_PATH).crop((297, 297, 726, 726))
    clean_thrall = extract_black_bg(thrall_im)
    save_sprite(clean_thrall, enemy_dir, "shadow_thrall_idle.png", ppu=64, pivot_y=0.1, target_size=(64, 64))
    
    # Generate walk variants for thrall (slight foot shifts)
    w_t, h_t = clean_thrall.size
    t_arr = np.array(clean_thrall)
    # Walk 0 (slight left lean)
    walk_0 = Image.fromarray(np.roll(t_arr, -2, axis=1))
    save_sprite(walk_0, enemy_dir, "shadow_thrall_walk_0.png", ppu=64, pivot_y=0.1, target_size=(64, 64))
    # Walk 1 (slight right lean)
    walk_1 = Image.fromarray(np.roll(t_arr, 2, axis=1))
    save_sprite(walk_1, enemy_dir, "shadow_thrall_walk_1.png", ppu=64, pivot_y=0.1, target_size=(64, 64))
    # Attack pose
    save_sprite(clean_thrall, enemy_dir, "shadow_thrall_attack.png", ppu=64, pivot_y=0.1, target_size=(64, 64))

    # ----------------------------------------------------
    # 3. BRAZIER & WAX SHARDS
    # ----------------------------------------------------
    prop_dir = os.path.join(ASSETS_DIR, "Props")
    item_dir = os.path.join(ASSETS_DIR, "Items")
    brazier_sheet = Image.open(BRAZIER_PATH)
    
    # Lit Brazier: (66, 132, 415, 662)
    crop_lit = brazier_sheet.crop((66, 132, 415, 662))
    clean_lit = extract_black_bg(crop_lit)
    save_sprite(clean_lit, prop_dir, "brazier_lit.png", ppu=64, pivot_y=0.1, target_size=(80, 88))

    # Unlit Brazier: (522, 132, 872, 662)
    crop_unlit = brazier_sheet.crop((522, 132, 872, 662))
    clean_unlit = extract_black_bg(crop_unlit)
    save_sprite(clean_unlit, prop_dir, "brazier_unlit.png", ppu=64, pivot_y=0.1, target_size=(80, 88))

    # Wax Shards: (997, 132, 1279, 662)
    crop_shards = brazier_sheet.crop((997, 132, 1279, 662))
    clean_shards = extract_black_bg(crop_shards)
    save_sprite(clean_shards, item_dir, "wax_shard_cluster.png", ppu=64, pivot_y=0.5, target_size=(48, 48))
    # Single shard (smaller subset)
    save_sprite(clean_shards, item_dir, "wax_shard_single.png", ppu=64, pivot_y=0.5, target_size=(32, 32))

    # ----------------------------------------------------
    # 4. DUNGEON TILES (80x80)
    # ----------------------------------------------------
    env_dir = os.path.join(ASSETS_DIR, "Environment")
    tiles_sheet = Image.open(TILES_PATH)
    # 4 quadrants of 512x512
    # TL: Stone Wall
    tile_wall = tiles_sheet.crop((0, 0, 512, 512)).resize((80, 80), Image.Resampling.NEAREST)
    save_sprite(tile_wall, env_dir, "tile_stone_wall.png", ppu=80, pivot_y=0.5)

    # TR: Repeatable Stone Floor
    tile_floor = tiles_sheet.crop((512, 0, 1024, 512)).resize((80, 80), Image.Resampling.NEAREST)
    save_sprite(tile_floor, env_dir, "tile_stone_floor.png", ppu=80, pivot_y=0.5)

    # BL: Cobblestone / Cracked Floor
    tile_cobble = tiles_sheet.crop((0, 512, 512, 1024)).resize((80, 80), Image.Resampling.NEAREST)
    save_sprite(tile_cobble, env_dir, "tile_cobble_floor.png", ppu=80, pivot_y=0.5)

    # BR: Crypt Doorway / Dark masonry
    tile_door = tiles_sheet.crop((512, 512, 1024, 1024)).resize((80, 80), Image.Resampling.NEAREST)
    save_sprite(tile_door, env_dir, "tile_doorway.png", ppu=80, pivot_y=0.5)

    # ----------------------------------------------------
    # 5. UI ASSETS (Candle Stages & Buttons)
    # ----------------------------------------------------
    ui_dir = os.path.join(ASSETS_DIR, "UI")
    candle_dir = os.path.join(ui_dir, "CandleStages")
    btn_dir = os.path.join(ui_dir, "Buttons")
    ui_sheet = Image.open(UI_PATH)

    # 8 Candle Health Stages (Row 0: Y: 65..330)
    candle_cols = [
        (68, 132), (229, 304), (395, 471), (561, 637),
        (728, 809), (883, 992), (1043, 1164), (1204, 1342)
    ]
    for idx, (x1, x2) in enumerate(candle_cols):
        c_crop = ui_sheet.crop((x1, 65, x2, 330))
        c_clean = extract_black_bg(c_crop)
        save_sprite(c_clean, candle_dir, f"candle_stage_{idx+1}.png", ppu=64, pivot_y=0.1, target_size=(56, 72))

    # Normal Buttons (Row 1: Y: 404..559)
    btn_normal_cols = [(46, 321), (367, 660), (723, 998), (1043, 1336)]
    btn_names = ["btn_play_normal", "btn_options_normal", "btn_exit_normal", "btn_continue_normal"]
    for idx, name in enumerate(btn_names):
        x1, x2 = btn_normal_cols[idx]
        b_crop = ui_sheet.crop((x1, 404, x2, 559))
        b_clean = extract_black_bg(b_crop)
        save_sprite(b_clean, btn_dir, f"{name}.png", ppu=64, pivot_y=0.5, target_size=(160, 50))

    # Highlighted Buttons (Row 2: Y: 576..731)
    btn_hl_names = ["btn_play_highlighted", "btn_options_highlighted", "btn_exit_highlighted", "btn_continue_highlighted"]
    for idx, name in enumerate(btn_hl_names):
        x1, x2 = btn_normal_cols[idx]
        b_crop = ui_sheet.crop((x1, 576, x2, 731))
        b_clean = extract_black_bg(b_crop)
        save_sprite(b_clean, btn_dir, f"{name}.png", ppu=64, pivot_y=0.5, target_size=(160, 50))

    # ----------------------------------------------------
    # 6. MAIN MENU BACKGROUND ART
    # ----------------------------------------------------
    title_im = Image.open(TITLE_PATH)
    title_out = os.path.join(ui_dir, "menu_background.png")
    title_im.save(title_out)
    meta_path = title_out + ".meta"
    if not os.path.exists(meta_path):
        guid = uuid.uuid4().hex
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(make_meta(guid, ppu=100, pivot_y=0.5, pivot_x=0.5))
    print(f"Saved: menu_background.png ({title_im.size}) to UI")

    print("\n--- ALL GENERATED ASSETS PROCESSED AND INTEGRATED INTO UNITY! ---")

if __name__ == "__main__":
    process_all()
