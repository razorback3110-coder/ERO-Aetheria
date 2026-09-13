# ERO Steam Release Architecture

Target flow:
Steam client -> ERO.exe -> ERO Startup -> Main Menu -> Character Creation -> World.

Steam is the primary PC distribution path. A separate launcher is optional for
standalone/internal builds and must not be required by the Steam build.

Release gates:
- Windows x64 build
- Steamworks integration
- Steam overlay/invites
- Steam Input/controller support
- achievements/stats
- cloud strategy reviewed
- crash reporting
- patching via SteamPipe
- redistributables
- clean install test
- offline/error-state UX
- Steam Deck compatibility review

This file is an implementation plan; Steam partner configuration and AppID are
performed in the Steamworks partner portal.
