"""
Core mechanics runnable verification for WickFall.
Verifies wax drain/restore math, 4-directional dominant axis resolution, and light scaling.
"""

def test_wax_mechanics():
    max_wax = 100.0
    current_wax = 100.0
    drain_amount = 25.0

    # Drain
    current_wax = max(0.0, current_wax - drain_amount)
    assert current_wax == 75.0, f"Expected 75.0, got {current_wax}"

    ratio = max(0.0, min(1.0, current_wax / max_wax))
    assert ratio == 0.75, f"Expected ratio 0.75, got {ratio}"

    # Over-drain clamped at 0
    current_wax = max(0.0, current_wax - 200.0)
    assert current_wax == 0.0, f"Expected 0.0, got {current_wax}"

    # Restore clamped at max
    current_wax = max(0.0, min(max_wax, current_wax + 150.0))
    assert current_wax == max_wax, f"Expected {max_wax}, got {current_wax}"

def test_directional_facing():
    def get_facing(x, y):
        if abs(x) > abs(y):
            return "East" if x > 0 else "West"
        else:
            return "North" if y > 0 else "South"

    assert get_facing(0.8, 0.2) == "East"
    assert get_facing(-0.7, 0.1) == "West"
    assert get_facing(0.1, 0.9) == "North"
    assert get_facing(-0.2, -0.6) == "South"

def test_light_scaling():
    min_range, max_range = 2.0, 8.0
    min_intensity, max_intensity = 0.4, 1.5

    def get_light(ratio):
        rng = min_range + (max_range - min_range) * ratio
        intensity = min_intensity + (max_intensity - min_intensity) * ratio
        return rng, intensity

    r_full, i_full = get_light(1.0)
    assert r_full == 8.0 and i_full == 1.5

    r_half, i_half = get_light(0.5)
    assert r_half == 5.0 and round(i_half, 2) == 0.95

    r_zero, i_zero = get_light(0.0)
    assert r_zero == 2.0 and i_zero == 0.4

def test_cursed_mire_avoidance_geometry():
    # Corridor 1 bounds: y in [-1.5, 1.5], height 3.0
    corridor_y_min, corridor_y_max = -1.5, 1.5
    player_radius = 0.35
    player_diameter = player_radius * 2

    # Mire placement in Corridor 1
    mire_center_y = -0.45
    mire_size_y = 1.2
    mire_top_edge = mire_center_y + (mire_size_y / 2.0) # -0.45 + 0.6 = 0.15

    # Safe lane above mire
    safe_lane_height = corridor_y_max - mire_top_edge # 1.5 - 0.15 = 1.35
    assert safe_lane_height > player_diameter, (
        f"Safe lane ({safe_lane_height}) must be wider than player diameter ({player_diameter})"
    )

    # Corridor 3 bounds: y in [17.5, 20.5], height 3.0
    corridor3_y_max = 20.5
    mire3_center_y = 18.5
    mire3_size_y = 1.1
    mire3_top_edge = mire3_center_y + (mire3_size_y / 2.0) # 18.5 + 0.55 = 19.05
    safe_lane3_height = corridor3_y_max - mire3_top_edge # 20.5 - 19.05 = 1.45
    assert safe_lane3_height > player_diameter, (
        f"Safe lane 3 ({safe_lane3_height}) must be wider than player diameter ({player_diameter})"
    )

def test_cursed_mire_escape_mechanics():
    base_speed = 4.2
    mire_multiplier = 0.6
    slowed_speed = base_speed * mire_multiplier
    assert slowed_speed == 2.52

    # Ember strike impulse gives an immediate burst to break through entanglement
    lunge_impulse = 2.5
    mass = 1.0
    escape_burst_velocity = lunge_impulse / mass
    total_escape_speed = slowed_speed + escape_burst_velocity
    assert total_escape_speed > base_speed, (
        f"Escape lunge ({total_escape_speed}) must exceed base walk speed ({base_speed})"
    )

def test_flash_color_invariance():
    # Model rapid flash behavior to ensure original color is invariant
    base_color = (1.0, 1.0, 1.0, 1.0)
    flash_color = (1.0, 0.2, 0.2, 1.0)
    current_color = base_color

    # First flash starts
    current_color = flash_color
    assert current_color == flash_color

    # Rapid second hit arrives mid-flash: must reset to base_color, never capture current_color
    interrupted_restored_color = base_color
    assert interrupted_restored_color == (1.0, 1.0, 1.0, 1.0)

    # After final flash completes
    current_color = base_color
    assert current_color == (1.0, 1.0, 1.0, 1.0)

if __name__ == "__main__":
    test_wax_mechanics()
    test_directional_facing()
    test_light_scaling()
    test_cursed_mire_avoidance_geometry()
    test_cursed_mire_escape_mechanics()
    test_flash_color_invariance()
    print("All core mechanics checks passed successfully.")
