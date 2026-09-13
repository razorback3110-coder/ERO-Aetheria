# ERO V538 — Loading, Transition & Shader Pass

Adds an ERO loading screen and reusable async scene transition API, plus five URP-compatible visual shaders: Holographic, Dissolve, Rift, Water and UI Glow.

Loading presentation keeps the approved ERO Character Creation reference as the visual backdrop. Scene transitions use SceneManager.LoadSceneAsync when loading a real scene; the current runtime zone travel also shows a brief transition while rebuilding its streamed/procedural zone.
