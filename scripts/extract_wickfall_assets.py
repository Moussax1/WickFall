import os
import uuid
from PIL import Image
import numpy as np
from collections import deque

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC_PATH = os.path.join(BASE_DIR, "resssources.png")
MENU_PATH = os.path.join(BASE_DIR, "Main menu.png")
ASSETS_DIR = os.path.join(BASE_DIR, "Assets", "Sprites")

def make_meta(guid, ppu=64, pivot_y=0.5, pivot_x=0.5, sprite_mode=1):
    return f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
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
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbook: 0
  ignorePngGamma: 0
  cookieLightType: 0
"""

def extract_alpha(crop_img, bg_color=(201, 195, 180), tol=34):
    img = crop_img.convert("RGBA")
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    
    bg_np = np.array(bg_color, dtype=float)
    diff = np.sqrt(np.sum((arr[:, :, :3] - bg_np)**2, axis=2))
    
    # Outer flood fill
    mask_to_remove = np.zeros((h, w), dtype=bool)
    q = deque()
    
    for y in range(h):
        for x in [0, w-1]:
            if diff[y, x] < tol and not mask_to_remove[y, x]:
                mask_to_remove[y, x] = True
                q.append((y, x))
    for x in range(w):
        for y in [0, h-1]:
            if diff[y, x] < tol and not mask_to_remove[y, x]:
                mask_to_remove[y, x] = True
                q.append((y, x))
                
    while q:
        cy, cx = q.popleft()
        for dy, dx in [(-1,0), (1,0), (0,-1), (0,1)]:
            ny, nx = cy + dy, cx + dx
            if 0 <= ny < h and 0 <= nx < w:
                if diff[ny, nx] < tol and not mask_to_remove[ny, nx]:
                    mask_to_remove[ny, nx] = True
                    q.append((ny, nx))
                    
    alpha = np.ones((h, w), dtype=float) * 255.0
    alpha[mask_to_remove] = 0.0
    
    # Defringe border pixels
    for y in range(h):
        for x in range(w):
            if alpha[y, x] > 0:
                is_adj = False
                for dy, dx in [(-1,0), (1,0), (0,-1), (0,1)]:
                    ny, nx = y+dy, cx+dx # note: cy, cx
                # proper check:
                for dy, dx in [(-1,0), (1,0), (0,-1), (0,1)]:
                    ny, nx = y+dy, x+dx
                    if 0 <= ny < h and 0 <= nx < w and alpha[ny, nx] == 0:
                        is_adj = True
                        break
                if is_adj and diff[y, x] < 50:
                    factor = (diff[y, x] - tol) / (50.0 - tol)
                    if factor <= 0.25:
                        alpha[y, x] = 0.0
                    else:
                        alpha[y, x] = 255.0 * factor
                        
    arr[:, :, 3] = np.clip(alpha, 0, 255)
    return Image.fromarray(arr.astype(np.uint8))

def crop_and_save(src_img, box, dest_folder, filename, ppu=64, pivot_y=0.5, pivot_x=0.5, remove_bg=True, target_size=None):
    os.makedirs(dest_folder, exist_ok=True)
    crop = src_img.crop(box)
    
    if remove_bg:
        processed = extract_alpha(crop)
    else:
        processed = crop.convert("RGBA")
        
    if target_size:
        # Place centered with bottom alignment
        tw, th = target_size
        canvas = Image.new("RGBA", (tw, th), (0, 0, 0, 0))
        cw, ch = processed.size
        # Bottom-center placement
        px = max(0, (tw - cw) // 2)
        py = max(0, th - ch)
        canvas.paste(processed, (px, py), processed)
        processed = canvas

    out_path = os.path.join(dest_folder, filename)
    processed.save(out_path)
    
    # Save meta
    meta_path = out_path + ".meta"
    if not os.path.exists(meta_path):
        g = uuid.uuid4().hex
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(make_meta(g, ppu=ppu, pivot_y=pivot_y, pivot_x=pivot_x))
            
    print(f"Saved: {filename} ({processed.size}) to {os.path.basename(dest_folder)}")
    return processed

def run_extraction():
    src = Image.open(SRC_PATH)
    print(f"Loaded master sheet: {src.size}")
    
    # ----------------------------------------------------
    # 1. CHARACTERS (Candle Golem)
    # ----------------------------------------------------
    char_dir = os.path.join(ASSETS_DIR, "Characters")
    
    # Row 0: Facing Down (Walk 0..3, Idle, Attack 1, Attack 2)
    # y: 375..445
    down_frames = [
        ((25, 375, 70, 445), "candle_golem_walk_down_0.png"),
        ((78, 375, 123, 445), "candle_golem_walk_down_1.png"),
        ((129, 375, 174, 445), "candle_golem_walk_down_2.png"),
        ((181, 375, 226, 445), "candle_golem_walk_down_3.png"),
        ((233, 375, 279, 445), "candle_golem_idle_down.png"),
        ((288, 375, 330, 445), "candle_golem_attack_down_0.png"),
        ((331, 375, 391, 445), "candle_golem_attack_down_1.png"),
    ]
    for box, name in down_frames:
        crop_and_save(src, box, char_dir, name, ppu=64, pivot_y=0.1, target_size=(64, 72))
        
    # Row 1: Facing Side (Walk 0..3, Idle, Attack 1, Attack 2)
    # y: 445..515
    side_frames = [
        ((25, 445, 70, 515), "candle_golem_walk_side_0.png"),
        ((79, 445, 122, 515), "candle_golem_walk_side_1.png"),
        ((130, 445, 174, 515), "candle_golem_walk_side_2.png"),
        ((181, 445, 224, 515), "candle_golem_walk_side_3.png"),
        ((243, 445, 271, 515), "candle_golem_idle_side.png"),
        ((296, 445, 335, 515), "candle_golem_attack_side_0.png"),
        ((337, 445, 387, 515), "candle_golem_attack_side_1.png"),
    ]
    for box, name in side_frames:
        crop_and_save(src, box, char_dir, name, ppu=64, pivot_y=0.1, target_size=(64, 72))
        
    # Row 2: Facing Up (Walk 0..3, Idle, Attack)
    # y: 515..585
    up_frames = [
        ((25, 515, 70, 585), "candle_golem_walk_up_0.png"),
        ((75, 515, 123, 585), "candle_golem_walk_up_1.png"),
        ((127, 515, 176, 585), "candle_golem_walk_up_2.png"),
        ((185, 515, 258, 585), "candle_golem_idle_up.png"),
        ((290, 515, 361, 585), "candle_golem_attack_up.png"),
    ]
    for box, name in up_frames:
        crop_and_save(src, box, char_dir, name, ppu=64, pivot_y=0.1, target_size=(64, 72))
        
    # Standing Candle Golem Portrait: (1180, 445, 1250, 520)
    crop_and_save(src, (1180, 445, 1250, 520), char_dir, "candle_golem_portrait.png", ppu=64, pivot_y=0.5)

    # ----------------------------------------------------
    # 2. ENEMIES (Shadow Thralls, Melting Guardian, Corrupted Wisp)
    # ----------------------------------------------------
    enemy_dir = os.path.join(ASSETS_DIR, "Enemies")
    
    # Shadow Thralls
    thrall_frames = [
        ((420, 385, 467, 445), "shadow_thrall_walk_0.png"),
        ((470, 385, 525, 445), "shadow_thrall_walk_1.png"),
        ((522, 385, 565, 445), "shadow_thrall_walk_2.png"),
        ((565, 385, 608, 445), "shadow_thrall_idle.png"),
        ((420, 480, 470, 555), "shadow_thrall_attack.png"),
        ((558, 490, 615, 555), "shadow_thrall_death.png"),
    ]
    for box, name in thrall_frames:
        crop_and_save(src, box, enemy_dir, name, ppu=64, pivot_y=0.1, target_size=(56, 64))
        
    # Melting Guardian
    crop_and_save(src, (625, 365, 700, 445), enemy_dir, "melting_guardian_molten.png", ppu=64, pivot_y=0.1, target_size=(80, 88))
    crop_and_save(src, (480, 470, 550, 555), enemy_dir, "melting_guardian_shadow.png", ppu=64, pivot_y=0.1, target_size=(80, 88))
    
    # Corrupted Wisp
    crop_and_save(src, (635, 485, 690, 560), enemy_dir, "corrupted_wisp.png", ppu=64, pivot_y=0.5, target_size=(56, 72))

    # ----------------------------------------------------
    # 3. ENVIRONMENT TILES (80x80)
    # ----------------------------------------------------
    env_dir = os.path.join(ASSETS_DIR, "Environment")
    
    tiles = [
        ((21, 590, 101, 670), "tile_stone_floor.png", 80, 0.5, False),
        ((107, 590, 187, 670), "tile_dirt_floor.png", 80, 0.5, False),
        ((194, 590, 274, 670), "tile_stone_wall.png", 80, 0.5, False),
        ((21, 671, 101, 751), "tile_cobble_floor.png", 80, 0.5, False),
        ((107, 671, 187, 751), "tile_cracked_floor.png", 80, 0.5, False),
        ((194, 671, 274, 751), "tile_doorway.png", 80, 0.5, False),
        ((284, 590, 368, 750), "arch_left.png", 80, 0.0, True),
        ((379, 590, 463, 750), "arch_right.png", 80, 0.0, True),
        ((474, 590, 514, 670), "wall_window.png", 80, 0.5, True),
    ]
    for box, name, ppu, pivot_y, rem_bg in tiles:
        crop_and_save(src, box, env_dir, name, ppu=ppu, pivot_y=pivot_y, remove_bg=rem_bg)

    # ----------------------------------------------------
    # 4. PROPS (Braziers, Gates, Pillars, Crates, Altar)
    # ----------------------------------------------------
    prop_dir = os.path.join(ASSETS_DIR, "Props")
    
    props = [
        ((542, 595, 625, 675), "brazier_lit.png", 64, 0.1),
        ((627, 600, 710, 675), "brazier_unlit.png", 64, 0.1),
        ((715, 600, 795, 675), "iron_gate_left.png", 64, 0.1),
        ((800, 600, 880, 675), "iron_gate_right.png", 64, 0.1),
        ((890, 600, 945, 675), "stone_pillar.png", 64, 0.1),
        ((540, 685, 615, 758), "stone_altar.png", 64, 0.1),
        ((625, 685, 705, 758), "cobweb_large.png", 64, 0.5),
        ((710, 685, 755, 758), "crate_single.png", 64, 0.1),
        ((758, 685, 818, 758), "crate_sarcophagus.png", 64, 0.1),
        ((820, 685, 880, 758), "crate_stacked.png", 64, 0.1),
        ((885, 685, 945, 758), "crate_big.png", 64, 0.1),
    ]
    for box, name, ppu, pivot_y in props:
        crop_and_save(src, box, prop_dir, name, ppu=ppu, pivot_y=pivot_y)

    # ----------------------------------------------------
    # 5. LIGHTING & TORCHES
    # ----------------------------------------------------
    light_dir = os.path.join(ASSETS_DIR, "Lighting")
    
    lights = [
        ((990, 600, 1020, 655), "candle_tall.png", 64, 0.1),
        ((1045, 600, 1072, 655), "candle_medium.png", 64, 0.1),
        ((1095, 600, 1125, 655), "candle_short.png", 64, 0.1),
        ((1140, 600, 1175, 655), "candle_melted.png", 64, 0.1),
        ((990, 665, 1020, 750), "candle_lit_tall.png", 64, 0.1),
        ((1045, 665, 1075, 750), "torch_wall_lit.png", 64, 0.1),
        ((1095, 665, 1125, 750), "torch_hand.png", 64, 0.1),
        ((1200, 685, 1270, 755), "light_cone_wide.png", 64, 0.5),
        ((1280, 685, 1350, 755), "light_cone_medium.png", 64, 0.5),
        ((1360, 685, 1405, 755), "light_cone_narrow.png", 64, 0.5),
    ]
    for box, name, ppu, pivot_y in lights:
        crop_and_save(src, box, light_dir, name, ppu=ppu, pivot_y=pivot_y)

    # ----------------------------------------------------
    # 6. ITEMS & PICKUPS
    # ----------------------------------------------------
    item_dir = os.path.join(ASSETS_DIR, "Items")
    
    items = [
        ((721, 385, 771, 435), "relic_flame_heart.png", 64, 0.5),
        ((778, 385, 827, 435), "relic_solar_amulet.png", 64, 0.5),
        ((835, 385, 884, 435), "relic_ruby_brooch.png", 64, 0.5),
        ((902, 385, 952, 435), "relic_skull_mask.png", 64, 0.5),
        ((959, 385, 1009, 435), "relic_fiery_stone.png", 64, 0.5),
        ((734, 525, 760, 565), "wax_shard_single.png", 64, 0.5),
        ((798, 525, 815, 565), "wax_shard_tiny.png", 64, 0.5),
        ((836, 525, 882, 575), "wax_shard_cluster.png", 64, 0.5), # Main wax pickup
        ((904, 525, 950, 575), "wick_bundle.png", 64, 0.5),       # Main wick pickup
        ((968, 525, 1000, 575), "wick_bundle_tied.png", 64, 0.5),
        ((1073, 465, 1175, 575), "ember_blade.png", 64, 0.5),      # Fire blade
        ((1330, 465, 1385, 575), "pyre_staff.png", 64, 0.5),       # Pyre staff
    ]
    for box, name, ppu, pivot_y in items:
        crop_and_save(src, box, item_dir, name, ppu=ppu, pivot_y=pivot_y)

    # ----------------------------------------------------
    # 7. UI & HUD (Buttons, Panels, Candle Health, Flames)
    # ----------------------------------------------------
    ui_dir = os.path.join(ASSETS_DIR, "UI")
    btn_dir = os.path.join(ui_dir, "Buttons")
    candle_dir = os.path.join(ui_dir, "CandleStages")
    flame_dir = os.path.join(ui_dir, "Flames")
    
    # Title Banner: "WICKFALL" logo + candles
    crop_and_save(src, (460, 25, 950, 125), ui_dir, "title_banner.png", ppu=64, pivot_y=0.5)
    
    # Main menu background from "Main menu.png"
    if os.path.exists(MENU_PATH):
        menu_src = Image.open(MENU_PATH)
        menu_out = os.path.join(ui_dir, "menu_background.png")
        menu_src.save(menu_out)
        meta_path = menu_out + ".meta"
        if not os.path.exists(meta_path):
            with open(meta_path, "w", encoding="utf-8") as f:
                f.write(make_meta(uuid.uuid4().hex, ppu=100, pivot_y=0.5, pivot_x=0.5))
        print(f"Saved: menu_background.png ({menu_src.size})")

    # UI Panels: Pause Menu & Inventory
    crop_and_save(src, (970, 145, 1170, 335), ui_dir, "panel_pause_menu.png", ppu=64, pivot_y=0.5, remove_bg=True)
    crop_and_save(src, (1180, 145, 1390, 335), ui_dir, "panel_inventory.png", ppu=64, pivot_y=0.5, remove_bg=True)

    # Buttons (160x50):
    buttons = [
        ((20, 185, 180, 235), "btn_new_game_normal.png"),
        ((190, 185, 350, 235), "btn_continue_normal.png"),
        ((360, 185, 520, 235), "btn_settings_normal.png"),
        ((360, 245, 520, 295), "btn_quit_normal.png"),
        ((190, 245, 350, 295), "btn_frame_highlighted.png"),
        ((360, 305, 520, 355), "btn_frame_pressed.png"),
    ]
    for box, name in buttons:
        crop_and_save(src, box, btn_dir, name, ppu=64, pivot_y=0.5, remove_bg=True)

    # Candle Health Stages (1 to 8):
    # From fresh tall candle (stage 1) to melted wax pool (stage 8)
    candle_stages = [
        ((585, 180, 635, 270), "candle_stage_1.png"),
        ((636, 185, 680, 270), "candle_stage_2.png"),
        ((681, 190, 722, 270), "candle_stage_3.png"),
        ((723, 195, 765, 270), "candle_stage_4.png"),
        ((766, 205, 805, 270), "candle_stage_5.png"),
        ((806, 215, 845, 270), "candle_stage_6.png"),
        ((846, 225, 885, 270), "candle_stage_7.png"),
        ((886, 235, 950, 270), "candle_stage_8.png"),
    ]
    for box, name in candle_stages:
        crop_and_save(src, box, candle_dir, name, ppu=64, pivot_y=0.1, target_size=(56, 72))

    # Elemental Flames:
    flames = [
        ((595, 305, 660, 385), "flame_orange.png"),
        ((665, 305, 730, 385), "flame_blue.png"),
        ((735, 305, 800, 385), "flame_green.png"),
        ((805, 305, 870, 385), "flame_white.png"),
        ((875, 305, 940, 385), "flame_magenta.png"),
    ]
    for box, name in flames:
        crop_and_save(src, box, flame_dir, name, ppu=64, pivot_y=0.5, remove_bg=True)

    print("\n--- ALL WICKFALL ASSETS EXTRACTED AND SAVED SUCCESSFULLY! ---")

if __name__ == "__main__":
    run_extraction()
