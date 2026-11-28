# SFX loudness report (bounded set) - feat/audio-assets-input

Base: live PR #129 tip 45b226172bd5ad9215235f3e2adc66e38d16b01e (feat/audit-fixes). Audio tree byte-identical to 45aad6b where the sweep ran (empty git diff between the two).

Method: ffmpeg ebur128=peak=true (BS.1770). Units: I = integrated loudness (LUFS), LRA = loudness range (LU), TP = true peak (dBTP). sha256 computed from the repo file bytes. Rights columns read verbatim from Docs/ASSET_MANIFEST.md.

SHORT-CLIP LIMITATION: BS.1770 integrated loudness is unreliable below ~0.4 s. Clips shorter than 0.4 s are marked SHORT and report sample peak (dBFS) and RMS (dBFS, ffmpeg astats) instead of I/LRA/TP.

Scope: the requested families only - footsteps, breathing, monster/hit, doors, UI clicks, ambience stingers. Other SFX families in the folder (glass, paper, creaks, crank, puzzle, quiz, bell, hums, etc.) were measured in the same sweep but are EXCLUDED from this bounded report. All 54 listed clips were measured; none unavailable. All numbers are MEASURED; in-game perceived loudness, mixer gain and device playback are UNVERIFIED.

## Footsteps (27 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_FootstepCarpet_0.ogg | 0.145 | - | - | - | SHORT: -1.1 / -23.7 | CC0-1.0 | Kenney (kenney.nl) | b9524658b52a99a20590f8feefc103088637fd13ea8ec35811aee785e1f2f330 |
| Assets/_Project/Audio/SFX/SFX_FootstepCarpet_1.ogg | 0.145 | - | - | - | SHORT: -1.1 / -20.3 | CC0-1.0 | Kenney (kenney.nl) | 9af3c3f76705107339e72036e4053bdca3eaa50ad31c8c929f42ce8d40e5d781 |
| Assets/_Project/Audio/SFX/SFX_FootstepCarpet_2.ogg | 0.145 | - | - | - | SHORT: -1.1 / -20.3 | CC0-1.0 | Kenney (kenney.nl) | 9af3c3f76705107339e72036e4053bdca3eaa50ad31c8c929f42ce8d40e5d781 |
| Assets/_Project/Audio/SFX/SFX_FootstepCarpet_3.ogg | 0.145 | - | - | - | SHORT: -1.1 / -23.3 | CC0-1.0 | Kenney (kenney.nl) | 4ae7454338d57c7d102145919ae653c7d010eb542797c4f25a1c7470be3a7e4c |
| Assets/_Project/Audio/SFX/SFX_FootstepCarpet_4.ogg | 0.145 | - | - | - | SHORT: -1.1 / -23.3 | CC0-1.0 | Kenney (kenney.nl) | 4ae7454338d57c7d102145919ae653c7d010eb542797c4f25a1c7470be3a7e4c |
| Assets/_Project/Audio/SFX/SFX_FootstepConcrete_0.ogg | 0.106 | - | - | - | SHORT: -0.9 / -21.2 | CC0-1.0 | Kenney (kenney.nl) | d7267e183067757c92c169de2a379abea592cfca6b39bb2e8feea15221ad79fe |
| Assets/_Project/Audio/SFX/SFX_FootstepConcrete_1.ogg | 0.108 | - | - | - | SHORT: -1.2 / -22.9 | CC0-1.0 | Kenney (kenney.nl) | 507a75eb19b897087855a3be3cf11a12d7fa37d143ad535eeebe0b8031c7325d |
| Assets/_Project/Audio/SFX/SFX_FootstepConcrete_2.ogg | 0.113 | - | - | - | SHORT: -1.0 / -23.5 | CC0-1.0 | Kenney (kenney.nl) | b21fa546d74941196b25da8a20b7010abd8c05c4fcf93545d1f344e66a664e13 |
| Assets/_Project/Audio/SFX/SFX_FootstepConcrete_3.ogg | 0.110 | - | - | - | SHORT: -1.1 / -21.8 | CC0-1.0 | Kenney (kenney.nl) | 8b513c2f0316324060b9174b8f28df6c2147418c506e6a53b77f18e149a5547b |
| Assets/_Project/Audio/SFX/SFX_FootstepConcrete_4.ogg | 0.114 | - | - | - | SHORT: -1.0 / -21.9 | CC0-1.0 | Kenney (kenney.nl) | 24dd8db2413e5b81ad181e1205c12a1c7801da5f85715d471b92a4b2f632e45e |
| Assets/_Project/Audio/SFX/SFX_FootstepGrass_0.ogg | 0.778 | -25.7 | 0.0 | -1.1 | - | CC0-1.0 | Kenney (kenney.nl) | 9d49497777405d78d7cf7f2888e28277f3a23192300cfd1d79d54876b20f479f |
| Assets/_Project/Audio/SFX/SFX_FootstepGrass_1.ogg | 0.674 | -24.9 | 0.0 | -1.1 | - | CC0-1.0 | Kenney (kenney.nl) | fb28781bd22b2eefdc14077f1adf5c94d7fe9c3a130194633b55f3e0d39d9f04 |
| Assets/_Project/Audio/SFX/SFX_FootstepGrass_2.ogg | 0.692 | -25.4 | 0.0 | -1.1 | - | CC0-1.0 | Kenney (kenney.nl) | 73a520139f5be716a403bfe9c80e0e94d1e61d8e9de04a354b336392164b53dc |
| Assets/_Project/Audio/SFX/SFX_FootstepGrass_3.ogg | 0.669 | -26.0 | 0.0 | -0.9 | - | CC0-1.0 | Kenney (kenney.nl) | 0546e1d8ab2def714425701a18bd34c5ee2794b12e858e5b595aabcf38a34137 |
| Assets/_Project/Audio/SFX/SFX_FootstepGrass_4.ogg | 0.590 | -25.5 | 0.0 | -1.1 | - | CC0-1.0 | Kenney (kenney.nl) | 086fd4ab3546a507ea5f5c759a9a10e535c95ade13705c238bc0eeffd7d4e8ba |
| Assets/_Project/Audio/SFX/SFX_FootstepGravel_0.ogg | 0.497 | -27.6 | 0.0 | -3.3 | - | CC0-1.0 | TinyWorlds | 2ae23b376ca303e84d69f717af41787cc7c1d750ba2c944272510ec945fbd58c |
| Assets/_Project/Audio/SFX/SFX_FootstepLeaves_0.ogg | 0.254 | - | - | - | SHORT: -3.4 / -24.6 | CC0-1.0 | TinyWorlds | a84d74450148db29b2883a3cdd5438685e9652eecbe993cbeee2db881cc5cfcb |
| Assets/_Project/Audio/SFX/SFX_FootstepLeaves_1.ogg | 0.182 | - | - | - | SHORT: -2.9 / -21.9 | CC0-1.0 | TinyWorlds | 5872e76c16bb25c9f5184b40c8ce70bf644a0dbaa07d34de9b6102f7620aa764 |
| Assets/_Project/Audio/SFX/SFX_FootstepMud_0.ogg | 0.069 | - | - | - | SHORT: -3.6 / -21.9 | CC0-1.0 | TinyWorlds | 3ba5e9436c9f39319376cbf86fab5be2a513b7d0ec3e20f1aaf0a4156725832a |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_0.ogg | 0.250 | - | - | - | SHORT: -0.9 / -21.2 | CC0-1.0 | Kenney (kenney.nl) | 5f1c252942a220d658121bd9417448cd19649ded9fff2ff15f6e50899373ce8e |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_1.ogg | 0.252 | - | - | - | SHORT: -1.0 / -20.4 | CC0-1.0 | Kenney (kenney.nl) | e4f68e1c9fd55b4dc8ca89ed2bbab6f200171bc54bdc18aa96b80d8e7336f462 |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_2.ogg | 0.251 | - | - | - | SHORT: -0.9 / -19.8 | CC0-1.0 | Kenney (kenney.nl) | 9ff095b05cee5eec40b081defe3f0eef7232bb16d4865ce03b9f6dbab875cfdd |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_3.ogg | 0.252 | - | - | - | SHORT: -1.1 / -21.9 | CC0-1.0 | Kenney (kenney.nl) | 2a68a083daa9eda08fed0ea5da5280dfbe0de226196c576be02f95259dd139d9 |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_4.ogg | 0.248 | - | - | - | SHORT: -1.0 / -19.6 | CC0-1.0 | Kenney (kenney.nl) | b33e0854e2b3f6b6492e25c25ef8f90447a332f7c6bb25dd02edced9581d3441 |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_5.ogg | 0.254 | - | - | - | SHORT: -3.6 / -19.7 | CC0-1.0 | TinyWorlds | 33d948e4b3a00377f5f4655b7cffda1f9c837727852d63a54e64d735ea135280 |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_6.ogg | 0.201 | - | - | - | SHORT: -3.1 / -16.7 | CC0-1.0 | TinyWorlds | 4b79aba53f45a9f66da03916e5d6dd66cba97a4d9041b2208b5e394e262d1cc8 |
| Assets/_Project/Audio/SFX/SFX_FootstepWood_7.ogg | 0.318 | - | - | - | SHORT: -3.0 / -22.5 | CC0-1.0 | TinyWorlds | a8f819eba82f25f32bb103cc4453155fe7a51784bb65996399e559c798b01fb1 |

## Breathing (player) (2 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_Breath_Scared.ogg | 6.254 | -20.0 | 2.4 | 4.2 | - | CC0-1.0 | primbal | d533585cfcf7635a1cd6143d4aa7a970bc3036ab2b005c8f64d6eb457c9557ac |
| Assets/_Project/Audio/SFX/SFX_Breath_Tired.ogg | 3.172 | -20.7 | 20.0 | -2.9 | - | CC0-1.0 | mikeask | 5aa337c900d36c00c52488cb5cc7d15f3590f62e9a33c88e4500dff7cee1236f |

## Monster / creature (7 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_ShadowHiss_0.ogg | 0.267 | - | - | - | SHORT: -3.5 / -19.7 | CC0-1.0 | EZduzziteh | 1d123828b95614991194576dfd8767a3159b647a5028f5ff6a1ad3475af612a4 |
| Assets/_Project/Audio/SFX/SFX_ShadowMoan_0.wav | 3.024 | -14.5 | 20.0 | -0.0 | - | CC0-1.0 | qubodup | 6ecb024d4cdc5cc85743faa79d41cd68f3d3e75d8faf3f23191311eac708efd4 |
| Assets/_Project/Audio/SFX/SFX_ShadowMoan_1.wav | 5.166 | -15.3 | 6.1 | -0.0 | - | CC0-1.0 | qubodup | 9fa1a4a133bd1d0c45e7a5998c9ea97df455b5ca6f790c0b80e3f43568475bff |
| Assets/_Project/Audio/SFX/SFX_ShadowMoan_2.wav | 5.737 | -17.9 | 5.1 | -0.0 | - | CC0-1.0 | qubodup | 380b486d55a5e18109d570b5c8fa5c31c74d9d83764c6a093f61d8355e36ff5e |
| Assets/_Project/Audio/SFX/SFX_Whisper_Burst.wav | 192.000 | -23.5 | 2.2 | -8.3 | - | CC0-1.0 | iamoneabe | 2b19c5f98659018e9a18c10896fc85efe478acc5367b1c99765b53b6c0b98a2b |
| Assets/_Project/Audio/SFX/SFX_WraithBreathNear_0.ogg | 2.988 | -15.8 | 0.0 | -2.7 | - | CC0-1.0 | qubodup | 6df1940f7e0291ac90d6eb708c65a222a55e4b9606b2868b1e064cd0b2817f1e |
| Assets/_Project/Audio/SFX/SFX_WraithRoam_0.ogg | 7.000 | -17.7 | 0.6 | -3.3 | - | CC0-1.0 | Ruhinre | 95ab2cb13d086a14652ea8b0d70a16b4ea063ce90340db36f029d1ab91870097 |

## Hits / impacts (4 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_HurtHeavy_0.ogg | 1.129 | -15.1 | 0.0 | -3.4 | - | CC0-1.0 | Nocturnal_Vanguard | edfb3ac9ffbac1237cf69470bd270b2e888f97c54809e753e04c496a3f4a7401 |
| Assets/_Project/Audio/SFX/SFX_HurtHit_0.ogg | 0.358 | - | - | - | SHORT: -2.6 / -15.3 | CC0-1.0 | Nocturnal_Vanguard | ae2351c857b7fcfd34af088fe4fcc831342917d22e7f4ff04eea749c60c8824b |
| Assets/_Project/Audio/SFX/SFX_ThrowImpactHard_0.ogg | 0.213 | - | - | - | SHORT: -3.8 / -27.8 | CC0-1.0 | rubberduck | 0bd7bafcbf7753163c35cef34dde7f2a56de33b55e78b8035e6990d1690fd3a0 |
| Assets/_Project/Audio/SFX/SFX_ThrowImpactSoft_0.ogg | 0.337 | - | - | - | SHORT: -4.1 / -23.3 | CC0-1.0 | rubberduck | 4a3af2ac3e758b582ff2538fcadefed659a0efe6965b9dff6011c3e728554c04 |

## Doors (10 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_DoorClose_0.ogg | 0.675 | -10.3 | 0.0 | 1.5 | - | CC0-1.0 | Kenney (kenney.nl) | 834d29c60a8a8bfb50b158cdb6b7dfa8f02812a408a1ee9703d038dfab0b1aeb |
| Assets/_Project/Audio/SFX/SFX_DoorClose_1.ogg | 0.612 | -15.6 | 0.0 | -2.4 | - | CC0-1.0 | Kenney (kenney.nl) | 4aa4fe79836c53cd6d0cb0b22ddc33b9901429463ae1bb75131f072683f63f21 |
| Assets/_Project/Audio/SFX/SFX_DoorClose_2.ogg | 0.714 | -8.2 | 0.0 | 1.5 | - | CC0-1.0 | Kenney (kenney.nl) | d0a2540e5e1639bc2fbeef34e76f28e9f9cb8a151736a0ac4c13859994328d0e |
| Assets/_Project/Audio/SFX/SFX_DoorClose_3.ogg | 0.603 | -15.5 | 0.0 | 0.3 | - | CC0-1.0 | Kenney (kenney.nl) | fd21c0e7a9d0317375d2561590f0770dd3380ee35507d064862cb44d6f71595b |
| Assets/_Project/Audio/SFX/SFX_DoorCreak.ogg | 0.661 | -20.0 | 0.0 | -6.6 | - | CC0-1.0 | Kenney (kenney.nl) | 8a346186fd297254248cab8e8117060a52a5cf2a84f603153a762108550ea95e |
| Assets/_Project/Audio/SFX/SFX_DoorLockedRattle_0.ogg | 0.613 | -33.4 | 0.0 | -3.2 | - | CC0-1.0 | pauliuw | c95f7d44bd096cee98971cb425f849489fc1a89465252ba55dec22f1968f0c85 |
| Assets/_Project/Audio/SFX/SFX_DoorOpen_0.ogg | 0.919 | -14.4 | 0.0 | 0.9 | - | CC0-1.0 | Kenney (kenney.nl) | 4ab93bab96522d8eb109ff96dc57cb6765deb02448fe14c10472084be5bb2a0b |
| Assets/_Project/Audio/SFX/SFX_DoorOpen_1.ogg | 1.413 | -16.7 | 0.0 | 1.1 | - | CC0-1.0 | Kenney (kenney.nl) | 68962fb0458c9bac59dec3adefa9849703c7c89d34712de03dd0075d095e79e9 |
| Assets/_Project/Audio/SFX/SFX_DoorSteelBang.ogg | 0.168 | - | - | - | SHORT: -0.9 / -20.9 | CC0-1.0 | Kenney (kenney.nl) | e07045693e4a2b3d165c424e3dab4c781d9ff8880a386880ac89a51315d7f831 |
| Assets/_Project/Audio/SFX/SFX_DoorUnlock_0.ogg | 0.595 | -24.4 | 0.0 | -3.2 | - | CC0-1.0 | rubberduck | ac518209b8a80609ad7f9f20a2de10718d455490bfbb9f1f231c93897623cbf1 |

## UI clicks (3 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_Click.ogg | 0.010 | - | - | - | SHORT: -0.9 / -16.2 | CC0-1.0 | Kenney (kenney.nl) | adcd1f4adc35f1b41bc1b5bbefeff7aa44f2f3f0d96d3199b544140c7c1e761c |
| Assets/_Project/Audio/SFX/SFX_UIPaperTap_0.ogg | 0.121 | - | - | - | SHORT: -3.3 / -23.7 | CC0-1.0 | Kenney (kenney.nl) | 5f696b3f963e1bcaa05ec5555906809b99bf422f3a72962b8dbb43981ce0b2c8 |
| Assets/_Project/Audio/SFX/SFX_UIPaperTap_1.ogg | 0.194 | - | - | - | SHORT: -3.0 / -21.9 | CC0-1.0 | Kenney (kenney.nl) | 15231e2f06bc98d7b0c97f23d8727245b7c2a759ca6b7a4c4c6891df6e8a16f7 |

## Ambience stingers (1 clips)

| path | dur s | I LUFS | LRA LU | TP dBTP | short: peak/RMS dBFS | license | author | sha256 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Assets/_Project/Audio/SFX/SFX_WraithAmbushStinger_0.ogg | 7.522 | -18.9 | 3.1 | -3.0 | - | CC0-1.0 | congusbongus | 88835d754e59875b3ebe61a440424d8d08c0528b8654455568e2e2d8721b1fe2 |

Total clips in bounded set: 54 (all measured).

## New asset: AMB_NightWindDrips (NOT wired)

Assets/_Project/Audio/Ambience/AMB_NightWindDrips.ogg - unmodified source bytes of "Loopable Dungeon Ambience" by JaggedStone, CC0 (page https://opengameart.org/content/loopable-dungeon-ambience, CC0 badge and text re-verified 2026-11-28; file https://opengameart.org/sites/default/files/dungeon_ambient_1.ogg, re-downloaded and byte-compared 2026-11-28, sha256 df491823e4877371c34dbda4e9321cd83a4a14fa7573cee0ebca1ae423b70e6e). Ogg Vorbis, 48 kHz stereo, 94.310 s.

The filename is a label only: it is NOT proof of wind, drips or non-musical character. All character judgments below are unverified.

Measured (ffmpeg ebur128=peak=true): I = -27.7 LUFS, LRA = 3.8 LU, TP = -3.1 dBTP. Overall sample peak -3.13 dBFS, RMS -29.71 dBFS (astats).

Loop-boundary numeric analysis (NOT an audition, NOT proof of seamlessness, school fit or non-musical character): the file was concatenated with itself and levels measured across the join at 94.310 s.

| window | sample peak dBFS | RMS dBFS |
| --- | --- | --- |
| first 100 ms of file | -22.66 | -30.56 |
| last 100 ms of file | -21.06 | -30.29 |
| 100 ms before join | -21.06 | -30.16 |
| 100 ms after join | -22.66 | -30.65 |
| +/-25 ms around join (click check) | -23.69 | -33.38 |

Reading: boundary levels match within ~0.5 dB RMS and no click spike appears (the join-region peak is ~20 dB below the file's overall peak). Limitations: numeric window analysis only; no listening check performed; codec edge behavior after Unity re-import untested; a slow boundary-phase drift at sub-window scale would not be caught.
