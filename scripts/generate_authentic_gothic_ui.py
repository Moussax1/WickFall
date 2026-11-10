import math
from PIL import Image, ImageDraw, ImageFilter

def create_glow_pill(path="Assets/Sprites/UI/btn_glow_pill.png", w=340, h=64, r=12):
    # Base RGBA image
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    
    # 1. Soft glowing aura underneath
    aura = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    adraw = ImageDraw.Draw(aura)
    adraw.rounded_rectangle([2, 2, w - 3, h - 3], radius=r + 2, outline=(255, 120, 15, 180), width=4)
    aura_blur = aura.filter(ImageFilter.GaussianBlur(radius=3))
    img.alpha_composite(aura_blur)
    
    # 2. Main pill body
    draw = ImageDraw.Draw(img)
    # Dark charcoal fill
    draw.rounded_rectangle([4, 4, w - 5, h - 5], radius=r, fill=(30, 20, 16, 250), outline=(245, 115, 20, 255), width=2)
    # Inner hot golden ember highlight line
    draw.rounded_rectangle([6, 6, w - 7, h - 7], radius=r - 2, outline=(255, 210, 80, 220), width=1)
    
    img.save(path)
    print(f"Created {path}")

def create_menu_reference_frame(path="Assets/Sprites/UI/menu_frame_reference.png", w=420, h=520):
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Clean, dark translucent crypt interior (deep charcoal, NO harsh grid)
    draw.rectangle([10, 10, w - 11, h - 11], fill=(14, 12, 12, 245))
    
    # Double pale bone/ivory borders matching reference mockup exactly
    # Outer ivory border
    draw.rectangle([8, 8, w - 9, h - 9], outline=(235, 226, 210, 255), width=2)
    # Inner ivory border
    draw.rectangle([14, 14, w - 15, h - 15], outline=(210, 200, 182, 240), width=2)
    # Dark gap between lines
    draw.rectangle([10, 10, w - 11, h - 11], outline=(18, 15, 14, 255), width=2)
    
    # Four-point rosette diamonds at the 4 corners
    def draw_corner_rosette(cx, cy):
        size = 7
        draw.polygon([(cx, cy - size), (cx + size, cy), (cx, cy + size), (cx - size, cy)], fill=(245, 235, 215, 255))
        draw.polygon([(cx, cy - size + 2), (cx + size - 2, cy), (cx, cy + size - 2), (cx - size + 2, cy)], fill=(185, 90, 25, 255))
        draw.point((cx, cy), fill=(255, 250, 230, 255))

    draw_corner_rosette(11, 11)
    draw_corner_rosette(w - 12, 11)
    draw_corner_rosette(11, h - 12)
    draw_corner_rosette(w - 12, h - 12)
    
    # Iron sconce brackets on sides for candles
    # Left candle ledge at Y = 175
    draw.rectangle([0, 170, 9, 178], fill=(42, 38, 36, 255))
    draw.line([0, 170, 9, 170], fill=(85, 78, 72, 255))
    draw.line([0, 178, 9, 178], fill=(16, 14, 13, 255))
    
    # Right candle ledge at Y = 255
    draw.rectangle([w - 10, 250, w - 1, 258], fill=(42, 38, 36, 255))
    draw.line([w - 10, 250, w - 1, 250], fill=(85, 78, 72, 255))
    draw.line([w - 10, 258, w - 1, 258], fill=(16, 14, 13, 255))
    
    img.save(path)
    print(f"Created {path}")

def create_toggle_assets():
    # 1. Stone toggle box (32x32)
    w, h = 32, 32
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([1, 1, w - 2, h - 2], radius=4, fill=(20, 17, 16, 255), outline=(70, 62, 56, 255), width=2)
    # Beveled highlight
    draw.line([2, 2, w - 3, 2], fill=(110, 98, 88, 255))
    draw.line([2, 2, 2, h - 3], fill=(110, 98, 88, 255))
    draw.line([2, h - 3, w - 3, h - 3], fill=(12, 10, 9, 255))
    draw.line([w - 3, 2, w - 3, h - 3], fill=(12, 10, 9, 255))
    img.save("Assets/Sprites/UI/toggle_stone_box.png")
    print("Created Assets/Sprites/UI/toggle_stone_box.png")

    # 2. Flame checkmark (22x22)
    cw, ch = 22, 22
    img_chk = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    dchk = ImageDraw.Draw(img_chk)
    # Checkmark pts: (3, 11) -> (8, 17) -> (19, 4)
    pts = [(3, 11), (4, 10), (8, 15), (18, 4), (20, 6), (9, 18), (7, 18), (2, 13)]
    dchk.polygon(pts, fill=(255, 170, 30, 255))
    # Hot inner highlight
    inner_pts = [(4, 11), (8, 16), (18, 5)]
    dchk.line(inner_pts, fill=(255, 240, 130, 255), width=1)
    img_chk.save("Assets/Sprites/UI/toggle_flame_check.png")
    print("Created Assets/Sprites/UI/toggle_flame_check.png")

create_glow_pill()
create_menu_reference_frame()
create_toggle_assets()
