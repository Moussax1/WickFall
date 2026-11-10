using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WickFall.CameraControl;
using WickFall.Enemies;
using WickFall.Environment;
using WickFall.Items;
using WickFall.Player;
using WickFall.UI;

namespace WickFall.EditorTools
{
    /// <summary>
    /// Automated Level and Scene Builder for WickFall's GameScene.
    /// Handcrafts a complete 2D dungeon layout with colliders, ambient lighting,
    /// Player with in-world melting visual states, Dynamic Darkness Shroud (Fog of War),
    /// Shadow Thralls, Corrupted Wisps, Spike Traps, Cursed Mires, Sarcophagi,
    /// Wax Pickups, the Ancient Brazier, and the full ornate Gothic HUD.
    /// </summary>
    public static class GameSceneBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/GameScene.unity";

        // Fonts
        private const string FONT_GOTHIC = "Assets/Fonts/MedievalGothic.ttf";

        // Environment Sprites
        private const string TILE_FLOOR_STONE = "Assets/Sprites/Environment/tile_stone_floor.png";
        private const string TILE_FLOOR_COBBLE = "Assets/Sprites/Environment/tile_cobble_floor.png";
        private const string TILE_WALL = "Assets/Sprites/Environment/tile_stone_wall.png";
        private const string TILE_DOORWAY = "Assets/Sprites/Environment/tile_doorway.png";

        // Prop Sprites
        private const string BRAZIER_UNLIT = "Assets/Sprites/Props/brazier_unlit.png";
        private const string BRAZIER_LIT = "Assets/Sprites/Props/brazier_lit.png";
        private const string PROP_PILLAR = "Assets/Sprites/Props/stone_pillar.png";
        private const string PROP_SARCOPHAGUS = "Assets/Sprites/Props/crate_sarcophagus.png";
        private const string PROP_COBWEB = "Assets/Sprites/Props/cobweb_large.png";
        private const string PROP_ALTAR = "Assets/Sprites/Props/stone_altar.png";

        // Enemies
        private const string ENEMY_THARLL_IDLE = "Assets/Sprites/Enemies/shadow_thrall_idle.png";
        private const string ENEMY_THARLL_ATTACK = "Assets/Sprites/Enemies/shadow_thrall_attack.png";
        private const string ENEMY_WISP = "Assets/Sprites/Enemies/corrupted_wisp.png";

        // Item Sprites
        private const string WAX_SHARD_SINGLE = "Assets/Sprites/Items/wax_shard_single.png";
        private const string WAX_SHARD_CLUSTER = "Assets/Sprites/Items/wax_shard_cluster.png";

        // Lighting & Shroud
        private const string DARKNESS_MASK = "Assets/Sprites/Lighting/darkness_vignette_mask.png";

        // UI Sprites
        private const string HUD_BEZEL = "Assets/Sprites/UI/hud_ornate_bezel.png";
        private const string HUD_BANNER = "Assets/Sprites/UI/hud_parchment_banner.png";
        private const string BTN_STONE_NORM = "Assets/Sprites/UI/Buttons/btn_stone_slab.png";
        private const string BTN_STONE_HIGH = "Assets/Sprites/UI/Buttons/btn_stone_slab_high.png";
        private const string SLIDER_TRACK = "Assets/Sprites/UI/slider_groove.png";
        private const string SLIDER_FILL = "Assets/Sprites/UI/slider_flame_fill.png";

        // Audio Clips
        private const string BGM_PATH = "Assets/Audio/crypt_ambient.wav";
        private const string SWING_SFX = "Assets/Audio/attack_swing.wav";
        private const string HIT_SFX = "Assets/Audio/hit_impact.wav";
        private const string PICKUP_SFX = "Assets/Audio/wax_pickup.wav";
        private const string SCREECH_SFX = "Assets/Audio/enemy_screech.wav";
        private const string KINDLE_SFX = "Assets/Audio/brazier_kindle.wav";
        private const string HURT_SFX = "Assets/Audio/player_hurt.wav";
        private const string EXTINGUISH_SFX = "Assets/Audio/player_extinguish.wav";

        [MenuItem("WickFall/Validate Game Scene")]
        public static int ValidateScene()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH);
            var player = Object.FindFirstObjectByType<PlayerController>();
            var wax = Object.FindFirstObjectByType<PlayerWax>();
            var attack = Object.FindFirstObjectByType<PlayerAttack>();
            var light = Object.FindFirstObjectByType<CandleLight>();
            var melter = Object.FindFirstObjectByType<PlayerVisualMelter>();
            var shroud = Object.FindFirstObjectByType<DarknessShroud>();
            var hud = Object.FindFirstObjectByType<GameHUDManager>();
            var brazier = Object.FindFirstObjectByType<Brazier>();
            var camFollow = Object.FindFirstObjectByType<CameraFollow2D>();

            var thralls = Object.FindObjectsByType<ShadowThrallController>(FindObjectsSortMode.None);
            var wisps = Object.FindObjectsByType<CorruptedWispController>(FindObjectsSortMode.None);
            var traps = Object.FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None);
            var mires = Object.FindObjectsByType<CursedMire>(FindObjectsSortMode.None);
            var pickups = Object.FindObjectsByType<WaxPickup>(FindObjectsSortMode.None);

            if (player == null) { Debug.LogError("Player missing"); return -1; }
            if (wax == null) { Debug.LogError("Wax missing"); return -2; }
            if (attack == null) { Debug.LogError("Attack missing"); return -3; }
            if (light == null) { Debug.LogError("Light missing"); return -4; }
            if (melter == null) { Debug.LogError("PlayerVisualMelter missing"); return -5; }
            if (shroud == null) { Debug.LogError("DarknessShroud missing"); return -6; }
            if (hud == null) { Debug.LogError("HUD missing"); return -7; }
            if (brazier == null) { Debug.LogError("Brazier missing"); return -8; }
            if (camFollow == null) { Debug.LogError("CameraFollow2D missing"); return -9; }
            if (thralls.Length == 0) { Debug.LogError("No thralls found"); return -10; }
            if (wisps.Length == 0) { Debug.LogError("No wisps found"); return -11; }
            if (traps.Length == 0) { Debug.LogError("No traps found"); return -12; }
            if (mires.Length == 0) { Debug.LogError("No mires found"); return -13; }
            if (pickups.Length == 0) { Debug.LogError("No pickups found"); return -14; }

            Debug.Log($"[VALIDATION_SUCCESS] Player, Melter, Shroud, Light, Attack, HUD, Brazier, CamFollow, {thralls.Length} Thralls, {wisps.Length} Wisps, {traps.Length} Traps, {mires.Length} Mires, {pickups.Length} Pickups verified!");
            return 200 + thralls.Length + wisps.Length;
        }

        [MenuItem("WickFall/Build Game Scene")]
        public static void BuildScene()
        {
            Debug.Log("[GameSceneBuilder] Rebuilding enhanced WickFall GameScene with Gothic UI, Melting Character, Darkness Shroud, Hazards and Wisps...");

            // 1. Create fresh scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Lighting & Dark Gothic Ambience
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.03f, 0.05f, 1f); // Deep pitch black shadows

            // 3. Setup Camera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.015f, 0.02f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cameraObj.AddComponent<AudioListener>();
            cameraObj.transform.position = new Vector3(0, 0, -10);

            var bgmSource = cameraObj.AddComponent<AudioSource>();
            var bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>(BGM_PATH);
            if (bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.loop = true;
                bgmSource.volume = 0.35f;
                bgmSource.playOnAwake = true;
            }

            var camFollow = cameraObj.AddComponent<CameraFollow2D>();

            // 4. Build Dungeon Tile Map
            GameObject dungeonParent = new GameObject("Dungeon_Environment");
            BuildDungeonLayout(dungeonParent);

            // 5. Build Player
            GameObject playerObj = BuildPlayer();
            camFollow.SetTarget(playerObj.transform);
            var playerWax = playerObj.GetComponent<PlayerWax>();

            // 6. Build Dynamic Darkness Shroud (Fog of War)
            BuildDarknessShroud(playerObj.transform);

            // 7. Spawn Enemy Encounters (Shadow Thralls + Corrupted Wisps)
            GameObject enemiesParent = new GameObject("Enemies");
            CreateShadowThrall(enemiesParent.transform, new Vector2(15f, 1f), "ShadowThrall_Hall");
            CreateShadowThrall(enemiesParent.transform, new Vector2(14f, 19f), "ShadowThrall_Ossuary1");
            CreateShadowThrall(enemiesParent.transform, new Vector2(20f, 17f), "ShadowThrall_Ossuary2");
            CreateShadowThrall(enemiesParent.transform, new Vector2(1f, 18f), "ShadowThrall_Guardian");

            // Elite Corrupted Wisps (Phase through walls)
            CreateCorruptedWisp(enemiesParent.transform, new Vector2(17f, 18f), "CorruptedWisp_Ossuary");
            CreateCorruptedWisp(enemiesParent.transform, new Vector2(-3f, 22f), "CorruptedWisp_Sanctum");

            // 8. Spawn Environmental Hazards (Cursed Mires & Spike Traps)
            GameObject hazardsParent = new GameObject("Hazards");
            CreateCursedMire(hazardsParent.transform, new Vector2(6.5f, -0.45f), new Vector2(2.4f, 1.2f));
            CreateCursedMire(hazardsParent.transform, new Vector2(16.5f, 21.5f), new Vector2(2.4f, 2.0f));
            CreateCursedMire(hazardsParent.transform, new Vector2(8.0f, 18.5f), new Vector2(2.0f, 1.1f));

            CreateSpikeTrap(hazardsParent.transform, new Vector2(13.5f, 2.5f));
            CreateSpikeTrap(hazardsParent.transform, new Vector2(17f, -1.5f));
            CreateSpikeTrap(hazardsParent.transform, new Vector2(19.5f, 20f));
            CreateSpikeTrap(hazardsParent.transform, new Vector2(0f, 16.5f));
            CreateSpikeTrap(hazardsParent.transform, new Vector2(-2.5f, 16.5f));

            // 9. Spawn Obstacles & Props (Pillars, Sarcophagi, Cobwebs)
            GameObject propsParent = new GameObject("Props");
            CreateProp(propsParent.transform, new Vector2(12f, 0f), PROP_PILLAR, true, new Vector2(0.8f, 0.9f));
            CreateProp(propsParent.transform, new Vector2(18f, 2f), PROP_PILLAR, true, new Vector2(0.8f, 0.9f));
            CreateProp(propsParent.transform, new Vector2(-4f, 17f), PROP_PILLAR, true, new Vector2(0.8f, 0.9f));
            CreateProp(propsParent.transform, new Vector2(2f, 21f), PROP_PILLAR, true, new Vector2(0.8f, 0.9f));

            CreateProp(propsParent.transform, new Vector2(12f, 17f), PROP_SARCOPHAGUS, true, new Vector2(0.9f, 0.9f));
            CreateProp(propsParent.transform, new Vector2(12f, 21f), PROP_SARCOPHAGUS, true, new Vector2(0.9f, 0.9f));
            CreateProp(propsParent.transform, new Vector2(22f, 18f), PROP_SARCOPHAGUS, true, new Vector2(0.9f, 0.9f));

            CreateProp(propsParent.transform, new Vector2(-2.5f, 2.5f), PROP_COBWEB, false, Vector2.zero);
            CreateProp(propsParent.transform, new Vector2(19.5f, 5.5f), PROP_COBWEB, false, Vector2.zero);
            CreateProp(propsParent.transform, new Vector2(-6.5f, 24.5f), PROP_COBWEB, false, Vector2.zero);

            // 10. Spawn Wax Shards
            GameObject itemsParent = new GameObject("Pickups");
            CreateWaxShard(itemsParent.transform, new Vector2(0f, 2.5f), false); // Tutorial shard
            CreateWaxShard(itemsParent.transform, new Vector2(12f, 4f), false);
            CreateWaxShard(itemsParent.transform, new Vector2(18f, -2f), true); // Cluster
            CreateWaxShard(itemsParent.transform, new Vector2(13f, 22f), false);
            CreateWaxShard(itemsParent.transform, new Vector2(21f, 21f), false);
            CreateWaxShard(itemsParent.transform, new Vector2(-5f, 23f), true); // Cluster

            // 11. Spawn The Ancient Brazier
            BuildBrazier(propsParent.transform, new Vector2(-1f, 20f));

            // 12. Build Ornate Gothic In-Game HUD Canvas
            BuildGameHUD(playerWax);

            // 13. Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[GameSceneBuilder] Successfully built and saved {SCENE_PATH}!");
        }

        private static void BuildDungeonLayout(GameObject parent)
        {
            Sprite stoneFloor = AssetDatabase.LoadAssetAtPath<Sprite>(TILE_FLOOR_STONE);
            Sprite cobbleFloor = AssetDatabase.LoadAssetAtPath<Sprite>(TILE_FLOOR_COBBLE);
            Sprite stoneWall = AssetDatabase.LoadAssetAtPath<Sprite>(TILE_WALL);

            HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

            void AddRoom(int xMin, int xMax, int yMin, int yMax)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int y = yMin; y <= yMax; y++)
                    {
                        floorPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Room 1: Sanctuary of Wicks (Spawn)
            AddRoom(-3, 3, -3, 3);
            // Corridor 1: Crypt Hallway (East to Room 2)
            AddRoom(4, 9, -1, 1);
            // Room 2: The Hall of Shadows
            AddRoom(10, 20, -4, 6);
            // Corridor 2: The Ascent (North to Room 3)
            AddRoom(14, 16, 7, 13);
            // Room 3: The Forgotten Ossuary
            AddRoom(11, 23, 14, 24);
            // Corridor 3: The Sanctum Anteroom (West to Room 4)
            AddRoom(5, 10, 18, 20);
            // Room 4: The Brazier Sanctum
            AddRoom(-7, 4, 14, 25);

            // Instantiate Floors
            GameObject floorsParent = new GameObject("Floors");
            floorsParent.transform.SetParent(parent.transform);

            foreach (var pos in floorPositions)
            {
                GameObject tile = new GameObject($"Floor_{pos.x}_{pos.y}");
                tile.transform.SetParent(floorsParent.transform);
                tile.transform.position = new Vector3(pos.x, pos.y, 0);

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = ((pos.x + pos.y) % 3 == 0) ? cobbleFloor : stoneFloor;
                sr.sortingOrder = -10;
            }

            // Calculate Perimeter Walls
            GameObject wallsParent = new GameObject("Walls");
            wallsParent.transform.SetParent(parent.transform);

            HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };

            foreach (var fpos in floorPositions)
            {
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = fpos + dir;
                    if (!floorPositions.Contains(neighbor))
                    {
                        wallPositions.Add(neighbor);
                    }
                }
            }

            foreach (var wpos in wallPositions)
            {
                GameObject wall = new GameObject($"Wall_{wpos.x}_{wpos.y}");
                wall.transform.SetParent(wallsParent.transform);
                wall.transform.position = new Vector3(wpos.x, wpos.y, 0);

                var sr = wall.AddComponent<SpriteRenderer>();
                sr.sprite = stoneWall;
                sr.sortingOrder = 0;

                var col = wall.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
            }
        }

        private static GameObject BuildPlayer()
        {
            GameObject player = new GameObject("Player_CandleGolem");
            player.transform.position = new Vector3(0, 0, 0);

            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/candle_golem_idle_down.png");
            sr.sortingOrder = 5;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.offset = new Vector2(0f, 0.15f);

            var controller = player.AddComponent<PlayerController>();

            // Setup serialized sprites
            SerializedObject soController = new SerializedObject(controller);
            soController.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soController.FindProperty("_idleDown").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/candle_golem_idle_down.png");
            soController.FindProperty("_idleUp").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/candle_golem_idle_up.png");
            soController.FindProperty("_idleSide").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/candle_golem_idle_side.png");

            PopulateSpriteArray(soController.FindProperty("_walkDownFrames"), "Assets/Sprites/Characters/candle_golem_walk_down_", 4);
            PopulateSpriteArray(soController.FindProperty("_walkUpFrames"), "Assets/Sprites/Characters/candle_golem_walk_up_", 4);
            PopulateSpriteArray(soController.FindProperty("_walkSideFrames"), "Assets/Sprites/Characters/candle_golem_walk_side_", 4);
            soController.ApplyModifiedProperties();

            // PlayerWax
            var wax = player.AddComponent<PlayerWax>();
            var sfxSource = player.AddComponent<AudioSource>();
            SerializedObject soWax = new SerializedObject(wax);
            soWax.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soWax.FindProperty("_sfxSource").objectReferenceValue = sfxSource;
            soWax.FindProperty("_hurtSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(HURT_SFX);
            soWax.FindProperty("_extinguishSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(EXTINGUISH_SFX);
            soWax.ApplyModifiedProperties();

            // PlayerAttack
            var attack = player.AddComponent<PlayerAttack>();
            SerializedObject soAttack = new SerializedObject(attack);
            soAttack.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soAttack.FindProperty("_sfxSource").objectReferenceValue = sfxSource;
            soAttack.FindProperty("_swingClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SWING_SFX);
            soAttack.FindProperty("_hitClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(HIT_SFX);
            soAttack.FindProperty("_enemyLayer").intValue = ~0; // All layers
            PopulateSpriteArray(soAttack.FindProperty("_attackFrames"), "Assets/Sprites/Characters/candle_golem_attack_", 4);
            soAttack.ApplyModifiedProperties();

            // Dynamic Candle Light
            GameObject lightObj = new GameObject("CandleWickLight");
            lightObj.transform.SetParent(player.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.35f, -0.5f);

            var pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(1.0f, 0.65f, 0.28f, 1f);
            pointLight.range = 8.0f;
            pointLight.intensity = 1.4f;

            var candleLight = lightObj.AddComponent<CandleLight>();
            SerializedObject soLight = new SerializedObject(candleLight);
            soLight.FindProperty("_playerWax").objectReferenceValue = wax;
            soLight.FindProperty("_pointLight").objectReferenceValue = pointLight;
            soLight.ApplyModifiedProperties();

            // In-World Visual Melter
            var melter = player.AddComponent<PlayerVisualMelter>();
            SerializedObject soMelter = new SerializedObject(melter);
            soMelter.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soMelter.FindProperty("_playerWax").objectReferenceValue = wax;
            soMelter.FindProperty("_collider").objectReferenceValue = col;
            soMelter.FindProperty("_wickLightTransform").objectReferenceValue = lightObj.transform;
            soMelter.FindProperty("_hurtColor").colorValue = new Color(1.0f, 0.15f, 0.15f, 1.0f);
            soMelter.ApplyModifiedProperties();

            // Dynamic Ground Wax Trail (Footsteps & Molten Lake)
            var trail = player.AddComponent<PlayerWaxTrail>();
            SerializedObject soTrail = new SerializedObject(trail);
            soTrail.FindProperty("_footstepSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/wax_droplet_step.png");
            soTrail.FindProperty("_puddleSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/wax_puddle_molten.png");
            soTrail.ApplyModifiedProperties();

            // Candle Golem Death Animation Sequence
            var death = player.AddComponent<PlayerDeathSequence>();
            SerializedObject soDeath = new SerializedObject(death);
            PopulateSpriteArray(soDeath.FindProperty("_meltFrames"), "Assets/Sprites/Characters/candle_golem_melt_", 4);
            soDeath.FindProperty("_extinguishPuffSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(EXTINGUISH_SFX);
            soDeath.ApplyModifiedProperties();

            return player;
        }

        private static void BuildDarknessShroud(Transform playerTransform)
        {
            GameObject shroudObj = new GameObject("DarknessShroud_FogOfWar");
            shroudObj.transform.position = playerTransform.position;

            var sr = shroudObj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DARKNESS_MASK);
            sr.sortingOrder = 50;

            var shroud = shroudObj.AddComponent<DarknessShroud>();
            SerializedObject so = new SerializedObject(shroud);
            so.FindProperty("_playerTransform").objectReferenceValue = playerTransform;
            so.ApplyModifiedProperties();
        }

        private static void CreateShadowThrall(Transform parent, Vector2 pos, string name)
        {
            GameObject thrall = new GameObject(name);
            thrall.transform.SetParent(parent);
            thrall.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = thrall.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ENEMY_THARLL_IDLE);
            sr.sortingOrder = 5;

            var rb = thrall.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = thrall.AddComponent<CircleCollider2D>();
            col.radius = 0.38f;
            col.offset = new Vector2(0f, 0.2f);

            var sfx = thrall.AddComponent<AudioSource>();

            var health = thrall.AddComponent<EnemyHealth>();
            SerializedObject soHealth = new SerializedObject(health);
            soHealth.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soHealth.FindProperty("_deathSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SCREECH_SFX);
            soHealth.ApplyModifiedProperties();

            var controller = thrall.AddComponent<ShadowThrallController>();
            SerializedObject soCtrl = new SerializedObject(controller);
            soCtrl.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soCtrl.FindProperty("_idleSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ENEMY_THARLL_IDLE);
            soCtrl.FindProperty("_attackSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ENEMY_THARLL_ATTACK);
            soCtrl.FindProperty("_attackRange").floatValue = 0.75f;
            soCtrl.FindProperty("_sfxSource").objectReferenceValue = sfx;
            soCtrl.FindProperty("_screechSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SCREECH_SFX);
            soCtrl.FindProperty("_attackSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(HIT_SFX);

            PopulateSpriteArray(soCtrl.FindProperty("_walkFrames"), "Assets/Sprites/Enemies/shadow_thrall_walk_", 2);
            soCtrl.ApplyModifiedProperties();
        }

        private static void CreateCorruptedWisp(Transform parent, Vector2 pos, string name)
        {
            GameObject wisp = new GameObject(name);
            wisp.transform.SetParent(parent);
            wisp.transform.position = new Vector3(pos.x, pos.y, 0);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(wisp.transform, false);
            visual.transform.localPosition = Vector3.zero;

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ENEMY_WISP);
            sr.sortingOrder = 6;

            var rb = wisp.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = wisp.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.42f;

            var sfx = wisp.AddComponent<AudioSource>();

            var health = wisp.AddComponent<EnemyHealth>();
            SerializedObject soHealth = new SerializedObject(health);
            soHealth.FindProperty("_maxHealth").floatValue = 25f;
            soHealth.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soHealth.FindProperty("_deathSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SCREECH_SFX);
            soHealth.FindProperty("_dropChance").floatValue = 1.0f;
            soHealth.ApplyModifiedProperties();

            var controller = wisp.AddComponent<CorruptedWispController>();
            controller.SetOrigin(pos);
            SerializedObject soCtrl = new SerializedObject(controller);
            soCtrl.FindProperty("_origin").vector2Value = pos;
            soCtrl.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            soCtrl.FindProperty("_sfxSource").objectReferenceValue = sfx;
            soCtrl.FindProperty("_screechSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SCREECH_SFX);
            soCtrl.ApplyModifiedProperties();
        }

        private static void CreateCursedMire(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject mire = new GameObject("CursedMire_Hazard");
            mire.transform.SetParent(parent);
            mire.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = mire.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/wax_puddle_molten.png");
            sr.color = new Color(0.22f, 0.08f, 0.35f, 0.92f);
            sr.sortingOrder = -5;
            mire.transform.localScale = new Vector3(size.x, size.y, 1f);

            var col = mire.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var mireComp = mire.AddComponent<CursedMire>();
            var sfx = mire.AddComponent<AudioSource>();
            sfx.loop = true;
            sfx.volume = 0.4f;
            sfx.playOnAwake = false;
        }

        private static void CreateSpikeTrap(Transform parent, Vector2 pos)
        {
            GameObject trap = new GameObject("SpikeTrap_Hazard");
            trap.transform.SetParent(parent);
            trap.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = trap.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TILE_FLOOR_COBBLE);
            sr.sortingOrder = -4;

            var col = trap.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.85f, 0.85f);

            var sfx = trap.AddComponent<AudioSource>();
            var spikeComp = trap.AddComponent<SpikeTrap>();
            SerializedObject so = new SerializedObject(spikeComp);
            so.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("_sfxSource").objectReferenceValue = sfx;
            so.FindProperty("_springSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(HIT_SFX);
            so.ApplyModifiedProperties();
        }

        private static void CreateProp(Transform parent, Vector2 pos, string spritePath, bool isObstacle, Vector2 colliderSize)
        {
            GameObject prop = new GameObject("Prop_" + System.IO.Path.GetFileNameWithoutExtension(spritePath));
            prop.transform.SetParent(parent);
            prop.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = prop.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingOrder = 2;

            if (isObstacle)
            {
                var col = prop.AddComponent<BoxCollider2D>();
                col.size = colliderSize;
                col.offset = new Vector2(0f, 0.1f);
            }
        }

        private static void CreateWaxShard(Transform parent, Vector2 pos, bool isCluster)
        {
            GameObject shard = new GameObject(isCluster ? "WaxShard_Cluster" : "WaxShard_Single");
            shard.transform.SetParent(parent);
            shard.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = shard.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(isCluster ? WAX_SHARD_CLUSTER : WAX_SHARD_SINGLE);
            sr.sortingOrder = 2;

            var col = shard.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var pickup = shard.AddComponent<WaxPickup>();
            SerializedObject so = new SerializedObject(pickup);
            so.FindProperty("_waxAmount").floatValue = isCluster ? 45f : 18f;
            so.FindProperty("_isCluster").boolValue = isCluster;
            so.FindProperty("_pickupSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(PICKUP_SFX);
            so.ApplyModifiedProperties();
        }

        private static void BuildBrazier(Transform parent, Vector2 pos)
        {
            GameObject brazierObj = new GameObject("Sacred_Brazier");
            brazierObj.transform.SetParent(parent);
            brazierObj.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = brazierObj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BRAZIER_UNLIT);
            sr.sortingOrder = 3;

            var col = brazierObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.75f;

            var audio = brazierObj.AddComponent<AudioSource>();

            GameObject lightObj = new GameObject("BrazierRadiantLight");
            lightObj.transform.SetParent(brazierObj.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.4f, -0.5f);

            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.72f, 0.35f, 1f);
            light.intensity = 0f;
            light.range = 0f;

            var brazier = brazierObj.AddComponent<Brazier>();
            SerializedObject so = new SerializedObject(brazier);
            so.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("_unlitSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(BRAZIER_UNLIT);
            so.FindProperty("_litSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(BRAZIER_LIT);
            so.FindProperty("_brazierLight").objectReferenceValue = light;
            so.FindProperty("_audioSource").objectReferenceValue = audio;
            so.FindProperty("_kindleSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(KINDLE_SFX);
            so.ApplyModifiedProperties();
        }

        private static void BuildGameHUD(PlayerWax playerWax)
        {
            // Event System
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            // Canvas
            GameObject canvasObj = new GameObject("HUD_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Load Custom Gothic Font
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FONT_GOTHIC);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            // Danger Vignette (Pulsing critical wax alert)
            GameObject dangerObj = new GameObject("DangerVignette");
            dangerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rtDanger = dangerObj.AddComponent<RectTransform>();
            rtDanger.anchorMin = Vector2.zero;
            rtDanger.anchorMax = Vector2.one;
            rtDanger.sizeDelta = Vector2.zero;
            Image imgDanger = dangerObj.AddComponent<Image>();
            imgDanger.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DARKNESS_MASK);
            imgDanger.color = Color.clear;
            imgDanger.raycastTarget = false;

            // Top-Left Health & Candle Indicator with Ornate Bezel
            GameObject healthContainer = new GameObject("HealthContainer");
            healthContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform rtHealth = healthContainer.AddComponent<RectTransform>();
            rtHealth.anchorMin = new Vector2(0, 1);
            rtHealth.anchorMax = new Vector2(0, 1);
            rtHealth.pivot = new Vector2(0, 1);
            rtHealth.anchoredPosition = new Vector2(30, -30);
            rtHealth.sizeDelta = new Vector2(340, 95);

            // Bezel Background
            Image imgBezel = healthContainer.AddComponent<Image>();
            imgBezel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HUD_BEZEL);
            imgBezel.type = Image.Type.Sliced;

            // Candle Stage Icon
            GameObject candleIconObj = new GameObject("CandleIcon");
            candleIconObj.transform.SetParent(healthContainer.transform, false);
            RectTransform rtCandle = candleIconObj.AddComponent<RectTransform>();
            rtCandle.anchorMin = new Vector2(0, 0.5f);
            rtCandle.anchorMax = new Vector2(0, 0.5f);
            rtCandle.pivot = new Vector2(0, 0.5f);
            rtCandle.anchoredPosition = new Vector2(24, 0);
            rtCandle.sizeDelta = new Vector2(56, 76);
            Image imgCandle = candleIconObj.AddComponent<Image>();
            imgCandle.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/CandleStages/candle_stage_1.png");

            // Wax Bar Slider
            GameObject sliderObj = new GameObject("WaxSlider");
            sliderObj.transform.SetParent(healthContainer.transform, false);
            RectTransform rtSlider = sliderObj.AddComponent<RectTransform>();
            rtSlider.anchorMin = new Vector2(0, 0.5f);
            rtSlider.anchorMax = new Vector2(0, 0.5f);
            rtSlider.pivot = new Vector2(0, 0.5f);
            rtSlider.anchoredPosition = new Vector2(96, 12);
            rtSlider.sizeDelta = new Vector2(215, 24);

            Slider waxSlider = sliderObj.AddComponent<Slider>();
            waxSlider.minValue = 0f;
            waxSlider.maxValue = 1f;
            waxSlider.value = 1f;
            waxSlider.interactable = false;

            // Slider Background
            GameObject sliderBg = new GameObject("Background");
            sliderBg.transform.SetParent(sliderObj.transform, false);
            RectTransform rtBg = sliderBg.AddComponent<RectTransform>();
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            rtBg.sizeDelta = Vector2.zero;
            Image imgBg = sliderBg.AddComponent<Image>();
            imgBg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_TRACK);
            imgBg.type = Image.Type.Sliced;
            imgBg.color = new Color(0.28f, 0.25f, 0.24f, 1f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform rtFillArea = fillArea.AddComponent<RectTransform>();
            rtFillArea.anchorMin = Vector2.zero;
            rtFillArea.anchorMax = Vector2.one;
            rtFillArea.sizeDelta = new Vector2(-4, -4);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform rtFill = fill.AddComponent<RectTransform>();
            rtFill.sizeDelta = Vector2.zero;
            Image imgFill = fill.AddComponent<Image>();
            imgFill.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FILL);
            imgFill.type = Image.Type.Sliced;
            imgFill.color = new Color(1.0f, 0.65f, 0.2f, 1f);
            waxSlider.fillRect = rtFill;

            // Wax Text
            GameObject waxTextObj = new GameObject("WaxText");
            waxTextObj.transform.SetParent(healthContainer.transform, false);
            RectTransform rtWaxTxt = waxTextObj.AddComponent<RectTransform>();
            rtWaxTxt.anchorMin = new Vector2(0, 0);
            rtWaxTxt.anchorMax = new Vector2(0, 0);
            rtWaxTxt.pivot = new Vector2(0, 0);
            rtWaxTxt.anchoredPosition = new Vector2(96, 10);
            rtWaxTxt.sizeDelta = new Vector2(215, 24);
            Text txtWax = waxTextObj.AddComponent<Text>();
            txtWax.font = font;
            txtWax.fontSize = 20;
            txtWax.fontStyle = FontStyle.Bold;
            txtWax.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtWax.verticalOverflow = VerticalWrapMode.Overflow;
            txtWax.alignment = TextAnchor.MiddleCenter;
            txtWax.color = new Color(1.0f, 0.95f, 0.82f, 1f);
            txtWax.text = "100 / 100";
            var waxOutline = waxTextObj.AddComponent<Outline>();
            waxOutline.effectColor = new Color(0.1f, 0.05f, 0.02f, 0.9f);
            waxOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Objective Parchment Banner (Top Center)
            GameObject objContainer = new GameObject("ObjectiveContainer");
            objContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform rtObjCont = objContainer.AddComponent<RectTransform>();
            rtObjCont.anchorMin = new Vector2(0.5f, 1);
            rtObjCont.anchorMax = new Vector2(0.5f, 1);
            rtObjCont.pivot = new Vector2(0.5f, 1);
            rtObjCont.anchoredPosition = new Vector2(0, -45);
            rtObjCont.sizeDelta = new Vector2(940, 62);

            Image imgBanner = objContainer.AddComponent<Image>();
            imgBanner.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HUD_BANNER);
            imgBanner.type = Image.Type.Sliced;

            GameObject objTextObj = new GameObject("ObjectiveText");
            objTextObj.transform.SetParent(objContainer.transform, false);
            RectTransform rtObj = objTextObj.AddComponent<RectTransform>();
            rtObj.anchorMin = Vector2.zero;
            rtObj.anchorMax = Vector2.one;
            rtObj.sizeDelta = new Vector2(-110, -14);
            Text txtObj = objTextObj.AddComponent<Text>();
            txtObj.font = font;
            txtObj.fontSize = 18;
            txtObj.fontStyle = FontStyle.Bold;
            txtObj.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtObj.verticalOverflow = VerticalWrapMode.Overflow;
            txtObj.alignment = TextAnchor.MiddleCenter;
            txtObj.color = new Color(1.0f, 0.88f, 0.45f, 1f);
            txtObj.text = "Traverse the dark catacombs and kindle the sacred Brazier.";
            var objShadow = objTextObj.AddComponent<Shadow>();
            objShadow.effectColor = new Color(0.05f, 0.05f, 0.05f, 0.85f);
            objShadow.effectDistance = new Vector2(1.5f, -1.5f);

            // Stats Text (Top Right)
            GameObject statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rtStats = statsObj.AddComponent<RectTransform>();
            rtStats.anchorMin = new Vector2(1, 1);
            rtStats.anchorMax = new Vector2(1, 1);
            rtStats.pivot = new Vector2(1, 1);
            rtStats.anchoredPosition = new Vector2(-35, -45);
            rtStats.sizeDelta = new Vector2(360, 44);
            Text txtStats = statsObj.AddComponent<Text>();
            txtStats.font = font;
            txtStats.fontSize = 22;
            txtStats.fontStyle = FontStyle.Bold;
            txtStats.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtStats.verticalOverflow = VerticalWrapMode.Overflow;
            txtStats.alignment = TextAnchor.MiddleRight;
            txtStats.color = new Color(0.95f, 0.90f, 0.82f, 1f);
            txtStats.text = "Shadow Thralls Cleansed: 0";
            var statsOutline = statsObj.AddComponent<Outline>();
            statsOutline.effectColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            statsOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Victory Modal
            GameObject victoryModal = CreateModal(canvasObj.transform, "VictoryModal", font, "SANCTUM CLEANSED",
                new Color(1f, 0.78f, 0.3f, 1f), out Button vReplay, out Button vMenu, out Text vSummary);

            // Game Over Modal
            GameObject gameOverModal = CreateModal(canvasObj.transform, "GameOverModal", font, "THE FLAME HAS DIED",
                new Color(0.95f, 0.3f, 0.25f, 1f), out Button goRetry, out Button goMenu, out Text goSummary);
            goSummary.text = "Your wax has melted into the cold stone of the crypt.\nThe ancient catacombs remain plunged in darkness.";

            victoryModal.SetActive(false);
            gameOverModal.SetActive(false);

            // Wire GameHUDManager
            var hudManager = canvasObj.AddComponent<GameHUDManager>();
            SerializedObject soHud = new SerializedObject(hudManager);
            soHud.FindProperty("_playerWax").objectReferenceValue = playerWax;
            soHud.FindProperty("_candleIcon").objectReferenceValue = imgCandle;
            soHud.FindProperty("_waxPercentText").objectReferenceValue = txtWax;
            soHud.FindProperty("_waxSlider").objectReferenceValue = waxSlider;
            soHud.FindProperty("_dangerVignette").objectReferenceValue = imgDanger;
            soHud.FindProperty("_objectiveText").objectReferenceValue = txtObj;
            soHud.FindProperty("_statsTrackerText").objectReferenceValue = txtStats;
            soHud.FindProperty("_victoryModal").objectReferenceValue = victoryModal;
            soHud.FindProperty("_gameOverModal").objectReferenceValue = gameOverModal;
            soHud.FindProperty("_victorySummaryText").objectReferenceValue = vSummary;
            soHud.FindProperty("_btnVictoryReplay").objectReferenceValue = vReplay;
            soHud.FindProperty("_btnVictoryMenu").objectReferenceValue = vMenu;
            soHud.FindProperty("_btnGameOverRetry").objectReferenceValue = goRetry;
            soHud.FindProperty("_btnGameOverMenu").objectReferenceValue = goMenu;

            // Load 8 candle stage sprites
            SerializedProperty stagesProp = soHud.FindProperty("_candleStages");
            stagesProp.arraySize = 8;
            for (int i = 0; i < 8; i++)
            {
                stagesProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/CandleStages/candle_stage_{i + 1}.png");
            }

            soHud.ApplyModifiedProperties();
        }

        private static GameObject CreateModal(Transform parent, string name, Font font, string title, Color titleColor,
            out Button btnAction, out Button btnMenu, out Text summaryText)
        {
            GameObject modal = new GameObject(name);
            modal.transform.SetParent(parent, false);
            RectTransform rtModal = modal.AddComponent<RectTransform>();
            rtModal.anchorMin = Vector2.zero;
            rtModal.anchorMax = Vector2.one;
            rtModal.sizeDelta = Vector2.zero;

            Image darkOverlay = modal.AddComponent<Image>();
            darkOverlay.color = new Color(0.02f, 0.02f, 0.03f, 0.90f);

            // Dialog Panel
            GameObject dialog = new GameObject("DialogPanel");
            dialog.transform.SetParent(modal.transform, false);
            RectTransform rtDialog = dialog.AddComponent<RectTransform>();
            rtDialog.anchorMin = new Vector2(0.5f, 0.5f);
            rtDialog.anchorMax = new Vector2(0.5f, 0.5f);
            rtDialog.pivot = new Vector2(0.5f, 0.5f);
            rtDialog.anchoredPosition = Vector2.zero;
            rtDialog.sizeDelta = new Vector2(580, 380);

            Image imgDialog = dialog.AddComponent<Image>();
            imgDialog.color = new Color(0.12f, 0.11f, 0.13f, 0.96f);

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(dialog.transform, false);
            RectTransform rtTitle = titleObj.AddComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0.5f, 1);
            rtTitle.anchorMax = new Vector2(0.5f, 1);
            rtTitle.pivot = new Vector2(0.5f, 1);
            rtTitle.anchoredPosition = new Vector2(0, -32);
            rtTitle.sizeDelta = new Vector2(520, 52);
            Text txtTitle = titleObj.AddComponent<Text>();
            txtTitle.font = font;
            txtTitle.fontSize = 32;
            txtTitle.fontStyle = FontStyle.Bold;
            txtTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtTitle.verticalOverflow = VerticalWrapMode.Overflow;
            txtTitle.alignment = TextAnchor.MiddleCenter;
            txtTitle.color = titleColor;
            txtTitle.text = title;

            // Summary
            GameObject sumObj = new GameObject("SummaryText");
            sumObj.transform.SetParent(dialog.transform, false);
            RectTransform rtSum = sumObj.AddComponent<RectTransform>();
            rtSum.anchorMin = new Vector2(0.5f, 0.5f);
            rtSum.anchorMax = new Vector2(0.5f, 0.5f);
            rtSum.pivot = new Vector2(0.5f, 0.5f);
            rtSum.anchoredPosition = new Vector2(0, 15);
            rtSum.sizeDelta = new Vector2(500, 130);
            summaryText = sumObj.AddComponent<Text>();
            summaryText.font = font;
            summaryText.fontSize = 20;
            summaryText.horizontalOverflow = HorizontalWrapMode.Wrap;
            summaryText.verticalOverflow = VerticalWrapMode.Overflow;
            summaryText.alignment = TextAnchor.MiddleCenter;
            summaryText.color = new Color(0.92f, 0.88f, 0.82f, 1f);
            summaryText.text = "Summary statistics placeholder...";

            // Button 1 (Action: Replay / Retry)
            GameObject btnActionObj = new GameObject("BtnAction");
            btnActionObj.transform.SetParent(dialog.transform, false);
            RectTransform rtBtn1 = btnActionObj.AddComponent<RectTransform>();
            rtBtn1.anchorMin = new Vector2(0.5f, 0);
            rtBtn1.anchorMax = new Vector2(0.5f, 0);
            rtBtn1.pivot = new Vector2(0.5f, 0);
            rtBtn1.anchoredPosition = new Vector2(-115, 40);
            rtBtn1.sizeDelta = new Vector2(200, 52);

            Image imgBtn1 = btnActionObj.AddComponent<Image>();
            imgBtn1.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_STONE_NORM);
            imgBtn1.type = Image.Type.Sliced;
            btnAction = btnActionObj.AddComponent<Button>();

            GameObject btn1TxtObj = new GameObject("Text");
            btn1TxtObj.transform.SetParent(btnActionObj.transform, false);
            RectTransform rtBtn1Txt = btn1TxtObj.AddComponent<RectTransform>();
            rtBtn1Txt.anchorMin = Vector2.zero;
            rtBtn1Txt.anchorMax = Vector2.one;
            rtBtn1Txt.sizeDelta = Vector2.zero;
            Text txtBtn1 = btn1TxtObj.AddComponent<Text>();
            txtBtn1.font = font;
            txtBtn1.fontSize = 22;
            txtBtn1.fontStyle = FontStyle.Bold;
            txtBtn1.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtBtn1.verticalOverflow = VerticalWrapMode.Overflow;
            txtBtn1.alignment = TextAnchor.MiddleCenter;
            txtBtn1.color = new Color(0.95f, 0.9f, 0.8f, 1f);
            txtBtn1.text = name.Contains("Victory") ? "Play Again" : "Try Again";

            // Button 2 (Main Menu)
            GameObject btnMenuObj = new GameObject("BtnMenu");
            btnMenuObj.transform.SetParent(dialog.transform, false);
            RectTransform rtBtn2 = btnMenuObj.AddComponent<RectTransform>();
            rtBtn2.anchorMin = new Vector2(0.5f, 0);
            rtBtn2.anchorMax = new Vector2(0.5f, 0);
            rtBtn2.pivot = new Vector2(0.5f, 0);
            rtBtn2.anchoredPosition = new Vector2(115, 40);
            rtBtn2.sizeDelta = new Vector2(200, 52);

            Image imgBtn2 = btnMenuObj.AddComponent<Image>();
            imgBtn2.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_STONE_NORM);
            imgBtn2.type = Image.Type.Sliced;
            btnMenu = btnMenuObj.AddComponent<Button>();

            GameObject btn2TxtObj = new GameObject("Text");
            btn2TxtObj.transform.SetParent(btnMenuObj.transform, false);
            RectTransform rtBtn2Txt = btn2TxtObj.AddComponent<RectTransform>();
            rtBtn2Txt.anchorMin = Vector2.zero;
            rtBtn2Txt.anchorMax = Vector2.one;
            rtBtn2Txt.sizeDelta = Vector2.zero;
            Text txtBtn2 = btn2TxtObj.AddComponent<Text>();
            txtBtn2.font = font;
            txtBtn2.fontSize = 22;
            txtBtn2.fontStyle = FontStyle.Bold;
            txtBtn2.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtBtn2.verticalOverflow = VerticalWrapMode.Overflow;
            txtBtn2.alignment = TextAnchor.MiddleCenter;
            txtBtn2.color = new Color(0.95f, 0.9f, 0.8f, 1f);
            txtBtn2.text = "Main Menu";

            return modal;
        }

        private static void PopulateSpriteArray(SerializedProperty arrayProp, string pathPrefix, int count)
        {
            arrayProp.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                string fullPath = $"{pathPrefix}{i}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
            }
        }
    }
}
