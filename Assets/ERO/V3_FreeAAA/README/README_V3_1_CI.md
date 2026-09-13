# ERO V3.1 — Automatic Unity CI

This patch makes the project GitHub Actions-ready.

Pipeline:
1. structural preflight
2. secret scan
3. Unity 6000.0.67f1 test run
4. Linux validation build
5. artifact/log upload

This is intentionally separate from gameplay and rendering code.
