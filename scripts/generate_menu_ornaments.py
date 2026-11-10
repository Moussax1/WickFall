import os
import uuid
from PIL import Image, ImageDraw, ImageFont
import numpy as np

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
UI_DIR = os.path.join(BASE_DIR, "Assets", "Sprites", "UI")
os.makedirs(UI_DIR, exist_ok=True)

def write_meta(filepath, ppu=100, border=(0,0,0,0)):
    meta_path = filepath + ".meta"
    if not os.path.exists(meta_path):
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

# -----------------------------------------------------------------
# 1. PILL HIGHLIGHT (Selected button frame: 320 x 64 px)
# -----------------------------------------------------------------
def make_pill_highlight():
    w, h = 320, 64
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    r = 12
    # Outer glow
    draw.rounded_rectangle([2, 2, w-3, h-3], radius=r, outline=(255, 140, 0, 180), width=3)
    # Bright inner flame border
    draw.rounded_rectangle([4, 4, w-5, h-5], radius=r-2, outline=(255, 215, 60, 255), width=2, fill=(24, 18, 14, 230))
    # Subtle inner dark bevel
    draw.rounded_rectangle([7, 7, w-8, h-8], radius=r-4, outline=(40, 28, 20, 200), width=1)
    
    path = os.path.join(UI_DIR, "btn_pill_highlight.png")
    im.save(path)
    write_meta(path, ppu=100, border=(16, 16, 16, 16))
    print("Saved btn_pill_highlight.png")

# -----------------------------------------------------------------
# 2. ORNATE GOTHIC MENU FRAME (420 x 500 px)
# -----------------------------------------------------------------
def make_ornate_frame():
    w, h = 420, 500
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    # Dark gothic translucent stone background
    draw.rectangle([12, 12, w-13, h-13], fill=(14, 12, 10, 235))
    
    # Outer stone border (2px)
    draw.rectangle([10, 10, w-11, h-11], outline=(90, 82, 75, 255), width=2)
    # Gap space
    # Inner gold/stone accent border (1px)
    draw.rectangle([16, 16, w-17, h-17], outline=(160, 145, 125, 255), width=1)
    # Innermost dark line
    draw.rectangle([20, 20, w-21, h-21], outline=(45, 38, 34, 255), width=1)
    
    # Corner clover / card-suit filigree ornaments
    corners = [(16, 16), (w-17, 16), (16, h-17), (w-17, h-17)]
    for cx, cy in corners:
        # Diamond / clover flourish
        draw.polygon([(cx, cy-8), (cx+8, cy), (cx, cy+8), (cx-8, cy)], fill=(210, 190, 150, 255), outline=(40, 32, 28, 255))
        draw.rectangle([cx-2, cy-2, cx+2, cy+2], fill=(255, 240, 200, 255))
        
    path = os.path.join(UI_DIR, "menu_ornate_frame.png")
    im.save(path)
    write_meta(path, ppu=100, border=(24, 24, 24, 24))
    print("Saved menu_ornate_frame.png")

# -----------------------------------------------------------------
# 3. GOTHIC TITLE LOGO ("WICKFALL")
# -----------------------------------------------------------------
def make_title_logo():
    w, h = 500, 130
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    # Try finding an installed gothic/serif font or render high-detail bitmap typography
    font = None
    for fpath in ["C:\\Windows\\Fonts\\gothic.ttf", "C:\\Windows\\Fonts\\cloister.ttf", "C:\\Windows\\Fonts\\oldenglish.ttf", "C:\\Windows\\Fonts\\georgiab.ttf", "C:\\Windows\\Fonts\\timesbd.ttf"]:
        if os.path.exists(fpath):
            try:
                font = ImageFont.truetype(fpath, 72)
                break
            except:
                pass
    if font is None:
        font = ImageFont.load_default()
        
    text = "WICKFALL"
    bbox = draw.textbbox((0, 0), text, font=font)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    tx = (w - tw) // 2
    ty = (h - th) // 2 - 10
    
    # Drop shadow (deep black/umber)
    for ox, oy in [(-3,3), (3,3), (-2,2), (2,2), (0,4), (0,5)]:
        draw.text((tx+ox, ty+oy), text, font=font, fill=(10, 8, 6, 255))
        
    # Dark outline
    for ox, oy in [(-2,0), (2,0), (0,-2), (0,2), (-1,-1), (1,-1), (-1,1), (1,1)]:
        draw.text((tx+ox, ty+oy), text, font=font, fill=(35, 28, 22, 255))
        
    # Base fill (warm antique bone ivory)
    draw.text((tx, ty), text, font=font, fill=(238, 230, 215, 255))
    
    # Highlight top sheen
    draw.text((tx, ty-1), text, font=font, fill=(255, 252, 245, 180))
    
    path = os.path.join(UI_DIR, "title_logo.png")
    im.save(path)
    write_meta(path, ppu=100)
    print("Saved title_logo.png")

# -----------------------------------------------------------------
# 4. FRAME CANDLE ACCENTS
# -----------------------------------------------------------------
def make_frame_candle():
    w, h = 48, 80
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    # Candle base / wax body (ivory with drips)
    bx, by = 16, 35
    bw, bh = 16, 40
    draw.rectangle([bx, by, bx+bw, by+bh], fill=(225, 215, 195, 255), outline=(50, 40, 30, 255))
    # Wax drips on sides
    draw.polygon([(bx-3, by+5), (bx, by+15), (bx, by)], fill=(210, 200, 180, 255))
    draw.polygon([(bx+bw, by), (bx+bw, by+20), (bx+bw+3, by+8)], fill=(210, 200, 180, 255))
    
    # Wick
    draw.line([(bx+bw//2, by), (bx+bw//2, by-8)], fill=(20, 15, 10, 255), width=2)
    
    # Flame (outer orange, mid gold, inner white)
    fx, fy = bx+bw//2, by-10
    draw.polygon([(fx, fy-22), (fx+7, fy-8), (fx+5, fy+2), (fx-5, fy+2), (fx-7, fy-8)], fill=(255, 95, 0, 255))
    draw.polygon([(fx, fy-16), (fx+5, fy-6), (fx+3, fy), (fx-3, fy), (fx-5, fy-6)], fill=(255, 195, 20, 255))
    draw.polygon([(fx, fy-10), (fx+2, fy-4), (fx, fy), (fx-2, fy-4)], fill=(255, 255, 230, 255))
    
    path = os.path.join(UI_DIR, "frame_candle.png")
    im.save(path)
    write_meta(path, ppu=100)
    print("Saved frame_candle.png")

if __name__ == "__main__":
    make_pill_highlight()
    make_ornate_frame()
    make_title_logo()
    make_frame_candle()
