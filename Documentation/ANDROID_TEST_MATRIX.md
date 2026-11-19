# Android Test Matrix

| Tier | Suggested device class | Target | Primary checks |
|---|---|---:|---|
| Low | 4 GB RAM / entry-level GPU | 30 FPS target | thermal load, memory pressure, input latency |
| Mid | 6–8 GB RAM / mid-range GPU | 30–60 FPS target | stable frame pacing, scene load, touch controls |
| High | 8+ GB RAM / recent GPU | 60 FPS target | rendering scale, shadows, effects, sustained load |

Record for every run:
- Device model, chipset, Android version and screen resolution.
- Unity Editor version and scripting backend.
- Graphics API (Vulkan/OpenGL ES).
- Average and 1% low FPS, CPU/GPU frame time.
- Peak RAM, app size, load time and crash/ANR count.
- Device temperature/battery drain after 15-30 minutes.
- Touch response and simultaneous multi-touch behavior.

Do not raise region population, traffic count or visual quality until the target device tier passes sustained testing.
