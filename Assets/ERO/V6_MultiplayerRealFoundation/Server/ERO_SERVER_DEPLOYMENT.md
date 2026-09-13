# ERO V6 Server Foundation

Unity 6 supports a Dedicated Server build target and can build it from the Editor,
script or command line. Unity's Dedicated Server package is released for Unity 6000.0,
and Netcode for GameObjects 2.7.0 is released for Unity 6000.0. citeturn0search0turn0search1turn0search2

This patch adds an ERO-owned build entry point:
`ERO > V6 > Build Linux Dedicated Server`

Target topology:
Client -> Gateway -> Master -> World Instance

Important: this is a real build foundation, not a finished production backend.
Authentication, persistence, authoritative combat, database, anti-cheat, rate limiting,
monitoring, failover and load testing still need implementation and real deployment tests.
