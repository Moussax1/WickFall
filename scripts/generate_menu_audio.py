import os
import math
import struct
import wave
import random

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
AUDIO_DIR = os.path.join(BASE_DIR, "Assets", "Audio")
os.makedirs(AUDIO_DIR, exist_ok=True)

def generate_wav(filepath, duration, sample_rate, generator_func):
    num_samples = int(duration * sample_rate)
    with wave.open(filepath, "w") as wav_file:
        wav_file.setnchannels(1) # mono
        wav_file.setsampwidth(2) # 16-bit
        wav_file.setframerate(sample_rate)
        
        frames = bytearray()
        for i in range(num_samples):
            t = float(i) / sample_rate
            sample = generator_func(t, duration)
            sample = max(-1.0, min(1.0, sample))
            sample_int = int(sample * 32767.0)
            frames.extend(struct.pack("<h", sample_int))
        wav_file.writeframes(frames)
    print(f"Generated audio: {os.path.basename(filepath)}")

# 1. Button Hover SFX: warm short frequency flare
def hover_sfx(t, dur):
    freq = 340.0 - 120.0 * (t / dur)
    env = math.sin(math.pi * (t / dur)) ** 1.5
    tone = math.sin(2.0 * math.pi * freq * t)
    noise = (random.random() * 2.0 - 1.0) * 0.15
    return (tone + noise) * env * 0.35

# 2. Button Click SFX: sharp ignite / stone tap
def click_sfx(t, dur):
    env = math.exp(-18.0 * t)
    freq = 520.0 * math.exp(-8.0 * t)
    tone = math.sin(2.0 * math.pi * freq * t) + 0.5 * math.sin(2.0 * math.pi * freq * 1.5 * t)
    click = (random.random() * 2.0 - 1.0) * math.exp(-40.0 * t) * 0.8
    return (tone * 0.6 + click) * env * 0.6

# 3. Ambient Crypt Drone Loop (5 seconds loopable)
def ambient_loop(t, dur):
    # Low fundamental drone at 55Hz (A1) and 82.5Hz (E2 fifth)
    drone1 = math.sin(2.0 * math.pi * 55.0 * t)
    drone2 = math.sin(2.0 * math.pi * 82.4 * t) * 0.6
    sub = math.sin(2.0 * math.pi * 27.5 * t) * 0.8
    
    # Subtle candle flicker noise
    crackle = 0.0
    if random.random() < 0.04:
        crackle = (random.random() * 2.0 - 1.0) * 0.2
        
    lfo = 0.7 + 0.3 * math.sin(2.0 * math.pi * 0.4 * t)
    return (drone1 + drone2 + sub + crackle) * lfo * 0.25

if __name__ == "__main__":
    sr = 44100
    generate_wav(os.path.join(AUDIO_DIR, "menu_hover.wav"), 0.12, sr, hover_sfx)
    generate_wav(os.path.join(AUDIO_DIR, "menu_click.wav"), 0.25, sr, click_sfx)
    generate_wav(os.path.join(AUDIO_DIR, "crypt_ambient.wav"), 6.0, sr, ambient_loop)
