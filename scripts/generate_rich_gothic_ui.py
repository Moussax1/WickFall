import os
import uuid
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
UI_DIR = os.path.join(BASE_DIR, "Assets", "Sprites", "UI")
BTN_DIR = os.path.join(UI_DIR, "Buttons")
os.makedirs(UI_DIR, exist_ok=True)
os.makedirs(BTN_DIR, exist_ok=True)

def write_meta(filepath, ppu=100, border=(0,0,0,0)):
    meta_path = filepath + ".meta"
    guid = uuid.uuid4().hex
    bx, by, bz, bw = border
    meta = f"""fileFormatVersion: 2
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
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: {bx}, y: {by}, z: {bz}, w: {bw}}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta)

# Helper: Draw stone texture noise
def apply_stone_noise(draw, x0, y0, x1, y1, base_rgb, noise_amount=15):
    for y in range(y0, y1, 2):
        for x in range(x0, x1, 2):
            n = (hash((x, y)) % (noise_amount * 2 + 1)) - noise_amount
            r = max(0, min(255, base_rgb[0] + n))
            g = max(0, min(255, base_rgb[1] + n))
            b = max(0, min(255, base_rgb[2] + n))
            draw.rectangle([x, y, x+1, y+1], fill=(r, g, b, 255))

# -----------------------------------------------------------------
# 1. ORNATE GOTHIC MENU FRAME (500 x 600 px)
# -----------------------------------------------------------------
def make_rich_menu_frame():
    w, h = 500, 600
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    # Outer stone drop shadow
    draw.rounded_rectangle([6, 6, w-7, h-7], radius=16, fill=(0, 0, 0, 180))
    
    # Outer heavy chiseled ashlar stone border
    draw.rounded_rectangle([12, 12, w-13, h-13], radius=12, fill=(45, 41, 38, 255), outline=(22, 19, 18, 255), width=3)
    
    # Masonry stone texture pass on border
    apply_stone_noise(draw, 14, 14, w-15, h-15, (48, 44, 40), noise_amount=12)
    
    # Carved stone bevel highlights (Top & Left light, Bottom & Right dark shadow)
    draw.line([(14, 14), (w-15, 14)], fill=(85, 78, 72, 255), width=2)
    draw.line([(14, 14), (14, h-15)], fill=(85, 78, 72, 255), width=2)
    draw.line([(w-15, 14), (w-15, h-15)], fill=(18, 16, 14, 255), width=2)
    draw.line([(14, h-15), (w-15, h-15)], fill=(18, 16, 14, 255), width=2)
    
    # Inner gold / bronze embossed filigree border
    draw.rectangle([28, 28, w-29, h-29], outline=(150, 125, 75, 255), width=2)
    draw.rectangle([32, 32, w-33, h-33], outline=(65, 52, 32, 255), width=1)
    
    # Inner dark crypt slate background
    draw.rectangle([34, 34, w-35, h-35], fill=(16, 14, 13, 248))
    # Stone slab texture inside
    for y in range(40, h-40, 4):
        for x in range(40, w-40, 4):
            n = (hash((x*3, y*3)) % 9) - 4
            draw.rectangle([x, y, x+3, y+3], fill=(16+n, 14+n, 13+n, 248))
            
    # Wrought iron rivet corner braces
    corners = [(24, 24), (w-25, 24), (24, h-25), (w-25, h-25)]
    for cx, cy in corners:
        # Iron square plate
        draw.rectangle([cx-8, cy-8, cx+8, cy+8], fill=(28, 26, 24, 255), outline=(70, 65, 60, 255), width=1)
        # Golden corner clover emblem
        draw.polygon([(cx, cy-6), (cx+6, cy), (cx, cy+6), (cx-6, cy)], fill=(215, 175, 80, 255), outline=(30, 25, 20, 255))
        draw.rectangle([cx-1, cy-1, cx+1, cy+1], fill=(255, 240, 180, 255))
        
    # Stone shelf ledges for candles
    # Left shelf
    draw.rectangle([0, 150, 18, 162], fill=(45, 41, 38, 255), outline=(18, 16, 14, 255), width=1)
    draw.line([(0, 150), (18, 150)], fill=(90, 82, 75, 255), width=1)
    # Right shelf
    draw.rectangle([w-19, 100, w-1, 112], fill=(45, 41, 38, 255), outline=(18, 16, 14, 255), width=1)
    draw.line([(w-19, 100), (w-1, 100)], fill=(90, 82, 75, 255), width=1)
    
    path = os.path.join(UI_DIR, "menu_ornate_frame.png")
    im.save(path)
    write_meta(path, ppu=100, border=(40, 40, 40, 40))
    print("Saved rich menu_ornate_frame.png")

# -----------------------------------------------------------------
# 2. ORNATE GOTHIC SETTINGS MODAL FRAME (620 x 480 px)
# -----------------------------------------------------------------
def make_rich_settings_frame():
    w, h = 620, 480
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    # Outer drop shadow
    draw.rounded_rectangle([4, 4, w-5, h-5], radius=16, fill=(0, 0, 0, 220))
    
    # Heavy stone slab border
    draw.rounded_rectangle([10, 10, w-11, h-11], radius=12, fill=(42, 38, 35, 255), outline=(20, 18, 16, 255), width=3)
    apply_stone_noise(draw, 12, 12, w-13, h-13, (45, 41, 38), noise_amount=10)
    
    # Chiseled bevel highlights
    draw.line([(12, 12), (w-13, 12)], fill=(80, 75, 70, 255), width=2)
    draw.line([(12, 12), (12, h-13)], fill=(80, 75, 70, 255), width=2)
    draw.line([(w-13, 12), (w-13, h-13)], fill=(16, 14, 12, 255), width=2)
    draw.line([(12, h-13), (w-13, h-13)], fill=(16, 14, 12, 255), width=2)
    
    # Carved header banner plate (for "SETTINGS" title)
    draw.rectangle([80, 18, w-81, 75], fill=(26, 23, 21, 255), outline=(130, 105, 60, 255), width=2)
    draw.rectangle([84, 22, w-85, 71], outline=(50, 42, 32, 255), width=1)
    
    # Inner dark stone background
    draw.rectangle([24, 85, w-25, h-25], fill=(15, 13, 12, 250), outline=(55, 48, 42, 255), width=1)
    # Fine stone noise
    for y in range(88, h-28, 4):
        for x in range(28, w-28, 4):
            n = (hash((x*5, y*5)) % 7) - 3
            draw.rectangle([x, y, x+3, y+3], fill=(15+n, 13+n, 12+n, 250))
            
    # Ornate corner iron brackets
    corners = [(18, 18), (w-19, 18), (18, h-19), (w-19, h-19)]
    for cx, cy in corners:
        draw.rectangle([cx-6, cy-6, cx+6, cy+6], fill=(24, 22, 20, 255), outline=(60, 55, 50, 255), width=1)
        draw.ellipse([cx-2, cy-2, cx+2, cy+2], fill=(180, 150, 70, 255))
        
    path = os.path.join(UI_DIR, "settings_ornate_frame.png")
    im.save(path)
    write_meta(path, ppu=100, border=(32, 85, 32, 32))
    print("Saved rich settings_ornate_frame.png")

# -----------------------------------------------------------------
# 3. CARVED STONE "BACK" BUTTON (220 x 60 px)
# -----------------------------------------------------------------
def make_stone_buttons():
    # Normal stone button
    w, h = 220, 60
    im_norm = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw_n = ImageDraw.Draw(im_norm)
    draw_n.rounded_rectangle([3, 3, w-4, h-4], radius=8, fill=(38, 34, 32, 255), outline=(16, 14, 12, 255), width=2)
    apply_stone_noise(draw_n, 5, 5, w-6, h-6, (42, 38, 36), noise_amount=10)
    # Bevel lines
    draw_n.line([(5, 5), (w-6, 5)], fill=(75, 70, 65, 255), width=1)
    draw_n.line([(5, 5), (5, h-6)], fill=(75, 70, 65, 255), width=1)
    draw_n.line([(w-6, 5), (w-6, h-6)], fill=(14, 12, 10, 255), width=1)
    draw_n.line([(5, h-6), (w-6, h-6)], fill=(14, 12, 10, 255), width=1)
    # Inner border
    draw_n.rectangle([10, 10, w-11, h-11], outline=(60, 52, 45, 255), width=1)
    
    path_n = os.path.join(BTN_DIR, "btn_stone_normal.png")
    im_norm.save(path_n)
    write_meta(path_n, ppu=100, border=(12, 12, 12, 12))
    
    # Highlighted stone button with glowing orange border
    im_hl = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw_h = ImageDraw.Draw(im_hl)
    draw_h.rounded_rectangle([1, 1, w-2, h-2], radius=10, outline=(255, 120, 0, 160), width=2)
    draw_h.rounded_rectangle([3, 3, w-4, h-4], radius=8, fill=(35, 28, 22, 255), outline=(255, 195, 40, 255), width=2)
    apply_stone_noise(draw_h, 5, 5, w-6, h-6, (45, 34, 25), noise_amount=10)
    draw_h.rectangle([10, 10, w-11, h-11], outline=(200, 130, 30, 255), width=1)
    
    path_hl = os.path.join(BTN_DIR, "btn_stone_highlighted.png")
    im_hl.save(path_hl)
    write_meta(path_hl, ppu=100, border=(12, 12, 12, 12))
    print("Saved btn_stone_normal.png & btn_stone_highlighted.png")

# -----------------------------------------------------------------
# 4. GOTHIC SLIDER ASSETS (Track, Fill, Handle)
# -----------------------------------------------------------------
def make_slider_assets():
    # Slider Track (recessed dark stone groove)
    tw, th = 320, 24
    im_track = Image.new("RGBA", (tw, th), (0, 0, 0, 0))
    dt = ImageDraw.Draw(im_track)
    dt.rounded_rectangle([2, 2, tw-3, th-3], radius=6, fill=(18, 16, 14, 255), outline=(55, 48, 42, 255), width=2)
    dt.line([(4, 4), (tw-5, 4)], fill=(10, 8, 6, 255), width=1) # top inner shadow
    p_track = os.path.join(UI_DIR, "slider_track.png")
    im_track.save(p_track)
    write_meta(p_track, ppu=100, border=(8, 8, 8, 8))
    
    # Slider Fill (glowing liquid molten flame)
    im_fill = Image.new("RGBA", (tw, th), (0, 0, 0, 0))
    df = ImageDraw.Draw(im_fill)
    df.rounded_rectangle([3, 3, tw-4, th-4], radius=5, fill=(245, 120, 15, 255), outline=(255, 215, 60, 255), width=1)
    df.line([(4, 5), (tw-5, 5)], fill=(255, 245, 180, 200), width=1) # bright flame core
    p_fill = os.path.join(UI_DIR, "slider_fill.png")
    im_fill.save(p_fill)
    write_meta(p_fill, ppu=100, border=(8, 8, 8, 8))
    
    # Slider Handle (fiery amber diamond gem)
    hw, hh = 28, 36
    im_handle = Image.new("RGBA", (hw, hh), (0, 0, 0, 0))
    dh = ImageDraw.Draw(im_handle)
    dh.polygon([(hw//2, 2), (hw-3, hh//2), (hw//2, hh-3), (2, hh//2)], fill=(255, 165, 20, 255), outline=(80, 45, 10, 255))
    dh.polygon([(hw//2, 6), (hw-6, hh//2), (hw//2, hh-6), (5, hh//2)], fill=(255, 230, 100, 255))
    dh.rectangle([hw//2-2, hh//2-2, hw//2+2, hh//2+2], fill=(255, 255, 230, 255))
    p_handle = os.path.join(UI_DIR, "slider_handle.png")
    im_handle.save(p_handle)
    write_meta(p_handle, ppu=100)
    print("Saved slider assets: track, fill, handle")

# -----------------------------------------------------------------
# 5. ENHANCED PILL HIGHLIGHT WITH FLAME GRADIENT
# -----------------------------------------------------------------
def make_rich_pill_highlight():
    w, h = 340, 72
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    r = 14
    
    # Outer fiery ember glow
    draw.rounded_rectangle([1, 1, w-2, h-2], radius=r+1, outline=(255, 100, 0, 120), width=2)
    draw.rounded_rectangle([3, 3, w-4, h-4], radius=r, outline=(255, 165, 20, 220), width=2)
    # Bright inner gold rim
    draw.rounded_rectangle([5, 5, w-6, h-6], radius=r-1, outline=(255, 230, 80, 255), width=1, fill=(28, 20, 15, 240))
    # Bevel lines
    draw.line([(r, 7), (w-r, 7)], fill=(255, 240, 160, 140), width=1)
    
    path = os.path.join(UI_DIR, "btn_pill_highlight.png")
    im.save(path)
    write_meta(path, ppu=100, border=(18, 18, 18, 18))
    print("Saved rich btn_pill_highlight.png")

if __name__ == "__main__":
    make_rich_menu_frame()
    make_rich_settings_frame()
    make_stone_buttons()
    make_slider_assets()
    make_rich_pill_highlight()
    print("--- ALL RICH GOTHIC UI ASSETS CREATED SUCCESSFULLY! ---")
