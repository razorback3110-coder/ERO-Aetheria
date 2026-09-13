# ERO V2.1 — DIRECT RENDER PUSH

This patch is an actual runtime rendering upgrade layered over ERO V2/V543.

## What changes in-game
- HDR camera output
- URP post-processing enabled
- ACES filmic tonemapping
- controlled bloom for Rift/magic highlights
- cinematic vignette
- contrast/exposure balancing
- stronger soft shadows and longer shadow distance
- exponential-squared atmospheric fog
- higher-quality camera antialiasing
- automatic installation after scene load
- optional ERO/V21/CharacterRim shader for hero-character rim lighting

## Installation
Extract this ZIP over the existing ERO V2 project and keep:
Assets/
Packages/
ProjectSettings/

The bootstrap installs itself at runtime, so no Boss Room UI stack is required.

## Important
This improves the actual rendered output, but it does not magically turn placeholder/procedural meshes into authored AAA assets. Final commercial AAA quality still requires production character/environment/animation/VFX/audio assets and real multiplayer QA.
