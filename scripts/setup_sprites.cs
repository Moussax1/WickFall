using UnityEditor;
using UnityEngine;

public static class SpriteImporterSetup
{
    public static void ConfigureAll()
    {
        ConfigureSprite("Assets/Sprites/UI/btn_glow_pill.png", new Vector4(16, 16, 16, 16));
        ConfigureSprite("Assets/Sprites/UI/menu_frame_reference.png", new Vector4(24, 24, 24, 24));
        ConfigureSprite("Assets/Sprites/UI/settings_frame_gothic.png", new Vector4(36, 36, 36, 90));
        ConfigureSprite("Assets/Sprites/UI/Buttons/btn_stone_slab.png", new Vector4(16, 16, 16, 16));
        ConfigureSprite("Assets/Sprites/UI/Buttons/btn_stone_slab_high.png", new Vector4(16, 16, 16, 16));
        ConfigureSprite("Assets/Sprites/UI/slider_groove.png", new Vector4(8, 4, 8, 4));
        ConfigureSprite("Assets/Sprites/UI/slider_flame_fill.png", new Vector4(8, 4, 8, 4));
        ConfigureSprite("Assets/Sprites/UI/slider_gem_thumb.png", Vector4.zero);
        ConfigureSprite("Assets/Sprites/UI/toggle_stone_box.png", new Vector4(6, 6, 6, 6));
        ConfigureSprite("Assets/Sprites/UI/toggle_flame_check.png", Vector4.zero);
        AssetDatabase.Refresh();
        Debug.Log("[SpriteImporterSetup] Configured all rich gothic UI sprites successfully!");
    }

    private static void ConfigureSprite(string path, Vector4 border)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        if (border != Vector4.zero)
        {
            ti.spriteBorder = border;
        }
        ti.SaveAndReimport();
    }
}
SpriteImporterSetup.ConfigureAll();
