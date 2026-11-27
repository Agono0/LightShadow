# Wire hurt and shadow hiss SFX (2026-11-27)

- PlayerHealth.ApplyHit plays SFX_HurtHit on a non-lethal strike and SFX_HurtHeavy on the lethal one.
- ShadowAudio plays SFX_ShadowHiss at the wraith when it enters the Repelled state.
- Both go through the Sfx helper, so a missing clip is a silent no-op.
- Not verified in the editor or on a device.
