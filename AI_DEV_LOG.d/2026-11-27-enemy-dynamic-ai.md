# Dynamic enemy AI: free roam + ambush (2026-11-27)

From the user's answered questions (WhatsApp 13:01): one enemy; afraid of
light, chases, hears when close, hides to scare, and roams any room
dynamically - "real AI, not scripted paths."

- Patrol no longer walks the waypoint loop in order: each stop picks a
  random waypoint it has not visited lately (recent-history of 4), so the
  wraith wanders the whole school with readable intent.
- New Ambush state: when Nour is near (within 14m) but not yet engaged and
  in the dark, the wraith slips into cover and waits quiet for up to 18s.
  If she walks into it (4.5m) it bursts out - lunge, camera shake, whisper -
  and the normal stalk takes over. 45s cooldown between ambushes.
- Afraid-of-light, chase, close-range hearing and the banish rule already
  existed (LitZone clamp, Chase state, the noise bus, focused-beam repel);
  this PR changes nothing about them.
