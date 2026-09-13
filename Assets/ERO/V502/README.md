# Eternal Realms Online — V502

V502 is the first ERO gameplay systems layer built on the working Boss Room foundation.

Implemented as isolated ERO code so the Boss Room networking sample remains intact:
- 8 ERO base classes and the canonical level 18 / 40 / 75 progression gates.
- 3 branches per class and 3 specialization + 6 branch skills per branch.
- Ragnarok-inspired equipment-slot model and 100-slot inventory model.
- Founder/Owner entitlement model with 10,000 Cristaux ERO and 1,000,000 Gold defaults.
- Runtime player profile and persistence-ready data model.
- Automatic bootstrap/status overlay.

The Boss Room networking layer remains the multiplayer foundation. Boss Room is server-authoritative and uses NetworkTransform for replicated movement; ERO systems are intentionally separated so they can be wired into server-authoritative gameplay without replacing the proven network core.
