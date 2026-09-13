# ERO V507 — Real Character Assets

The character preview uses the original Boss Room character-select prefabs already included in the source project. ERO does not duplicate or override the Boss Room `Avatar` selection flow at runtime.

V509 adds the reference repair/fallback layer so these real prefabs remain available even if a serialized `GraphicsCharacterSelect` reference is lost during Unity migration/import.
