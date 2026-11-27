### 2026-11-27 - Ending VO 28-clip provenance ledger (PR #95) - Instinct agent (for Ahmed)

Generator: Instinct main agent's text-to-speech capability (synthetic TTS, not
human recordings). This task agent received the 28 source WAVs from the
generator and performed format conversion only (mono 24 kHz Vorbis .ogg q4;
loudness and trimming untouched from generation; no audio content changes).

Voices as recorded: the generator's handoff message (main -> task agent,
2026-11-27 5:40:37 PM EET) states the casting used for this batch:
Memory=Schedar, Nour=Leda, Mother=Gacrux, Shadow=Enceladus. Speaker per line
follows Docs/ENDING_VO_SPEC.md section 3 (spec blob
7a3d13396e4bf4b4065a5e446d6d25dc1ed4d419 at base 3a3e81b). PROOF LEVEL: the
voice column rests on that handoff statement only. The per-call voice
parameters live in the generator's own logs (main agent), not in this repo;
no audition was possible in the agent environment, so the voice identity of
each file is NOT independently verified.

Continuity with prior generated batches: UNVERIFIED. The prior batch record
(AI_DEV_LOG.d/2026-11-27-generated-vo-clips-77-story-lines-in-en-ar-plan-.md)
lists Nour=Leda, Mother=Gacrux, Shadow=Enceladus (whisper), System=Schedar and
does not name a Memory voice.

Authority: Ahmed's verified 2026-11-27 5:35 PM EET acceptance of the 28 ending
clips (14 lines x EN/AR), relayed by main. No broader batch approval claimed.

Rights: synthetic in-house generation, NOT third-party stock; no third-party
license and no CC0 claim. Rights in the generated audio are governed by the
generator's terms; no clearance beyond that is claimed. Validator PASS is a
format check only, not a rights check.

| # | Clip | Path | Speaker (spec section 3) | Voice (handoff record) | Source WAV | Source WAV sha256 | Committed OGG sha256 | Dur (s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | VO_SN_Ending_01 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_01.ogg | Memory | Schedar | speech-20261127-153724-dad14498.wav | cf502ad816ef9f3127e711b40a1b7ebfac3f78311ee4a6f8a0ad00cd1f6ae2d4 | 19b1b567e308f4d8584fa692f3ee6b4c2abd51488376b2e6bab901f419376713 | 8.000000 |
| 1 | VO_SN_Ending_01 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_01.ogg | Memory | Schedar | speech-20261127-153723-b2d9fab0.wav | eb55fee50c4606d5d311425b16350bbbbbd27433e8fda626fc87e10dd1a05249 | e82ea204ed4fc28ab21f32972a42e6e7255ddd541ec5632b5464fa1dfa5e28fa | 7.200000 |
| 2 | VO_SN_Ending_02 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_02.ogg | Nour | Leda | speech-20261127-153724 (3)-cec0eb0c.wav | ac3deed14c856a011424649acd448b7957a03b28482c320cb8e9b00fa64d0218 | 7b0ae53f35562518ef1d13f4f1181a9ab873b09f1123d40c7d55b66603ef6ecf | 8.120000 |
| 2 | VO_SN_Ending_02 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_02.ogg | Nour | Leda | speech-20261127-153724 (2)-88951f47.wav | 27b650379b3d7496adc136f3e13dfeb767c47eebd52b2b1b5ed4d441df6c57ca | a15d2247e8413acb75354d6c1f17bca51ccee72db74894686ca1f6c1e9ab7cc8 | 8.760000 |
| 3 | VO_SN_Ending_03 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_03.ogg | Nour | Leda | speech-20261127-153754-935c4f42.wav | c3d9e0b84481389fc301c1429030366892d8545026cadc1434ead1ae0b2d7363 | f53d0d8066af7ca71c6b177873e18422f63c4497726bf1fbca90b07263bee827 | 5.360000 |
| 3 | VO_SN_Ending_03 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_03.ogg | Nour | Leda | speech-20261127-153724 (1)-01f3100b.wav | 5f0c33c0b50e7d6450953fc43f87d271245d3dd55516fd6258ed5af0526a9130 | 96a0afaa0c8a7f9eb79b643cc04ec34a4593716ec277d6dda9e03128983360d4 | 5.120000 |
| 4 | VO_SN_Ending_04 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_04.ogg | Mother | Gacrux | speech-20261127-153808-f493ad15.wav | 921619dca36dca427ef8d62e0373c0bd1e56bf13268a8a0b80803f48f86fe191 | a8ba0255f637fe411a2e4e231c3ec1f7b9dbc96241a30f4f1609f792a4e5d8e8 | 5.520000 |
| 4 | VO_SN_Ending_04 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_04.ogg | Mother | Gacrux | speech-20261127-153801-48e17d1b.wav | 1d9982d9fb5b0dc46d41f94a86b40753f1fa42a46425ff264a3d13ffb0152646 | ac4734c95496cd2b7dc5ffd6e71aea1147f0510787bf99c24938e579d48291bc | 5.360000 |
| 5 | VO_SN_Ending_05 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_05.ogg | Memory | Schedar | speech-20261127-153823-8b3e2a4f.wav | 0c6de25bcf53519ec8718f06e2660aad993b76f6d130d00bad7eb48a82a4f17f | ce946acb8ea8f0e1e365b75e4a1f5cb4ad1ec332cc1b38e76ef3c468c0c7ba58 | 8.320000 |
| 5 | VO_SN_Ending_05 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_05.ogg | Memory | Schedar | speech-20261127-153815-e5d43dca.wav | 382fc1b2e096ceccf3d4755c7c84dee9515a8ae4adee8dfafc8eefd6691439ed | 18e8d5e22c4496bd1c19306ee41e534144adc598ced6a117ad6dab0df15b2580 | 7.440000 |
| 6 | VO_SN_Ending_06 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_06.ogg | Shadow | Enceladus | speech-20261127-153836-cdde933d.wav | ded8d636edefa18b51054395d50f8ba357837678ff7c91ff1a65ab0db2e0ccbf | fe039fc28a75144bde9f782d4fc75d89c6a3f0015561bb6b9a733d93056c7fd1 | 4.040000 |
| 6 | VO_SN_Ending_06 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_06.ogg | Shadow | Enceladus | speech-20261127-153830-0947dcd4.wav | a1b3038d954549721bb5bfae765fe8b1ccc2795ded2513cb8cebeaa7b3d4f60b | 0f5e840da1d36b80e0798eb8c3164ec59724125c37b79c70bc18801918ba6870 | 5.160000 |
| 7 | VO_SN_Ending_07 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_07.ogg | Nour | Leda | speech-20261127-153851-2b28113a.wav | 50de43f65bacf21a79949cdfa1b867bc0c6e0f7345c52ecb6b2cf51aca572b52 | 2f6e7bf6e6e8e5aaee5697392a0c4b9ac41ad384f12a1aa78d6f89f833841697 | 5.240000 |
| 7 | VO_SN_Ending_07 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_07.ogg | Nour | Leda | speech-20261127-153843-2b0a2f09.wav | 7f1ffafa632f9f540476f0a0bfdbf318bccaf08cd071905c49c623786a2aa966 | 07bc8db606e846bbf48f109a9c36502e9a9596e0ed79bc6038b35337bfda1af1 | 5.920000 |
| 8 | VO_SN_Ending_08 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_08.ogg | Memory | Schedar | speech-20261127-153727-4252617f.wav | 803dfc8429b3b03e5750416ce2ad591dc9ff0efc1ce7c6a69c35d2a63b587826 | 334be57f0390780ff7190272ce316808df859181ce1867f5a0e159fd5a74acaa | 6.760000 |
| 8 | VO_SN_Ending_08 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_08.ogg | Memory | Schedar | speech-20261127-153858-033f4627.wav | e6fa616dfe4532eeec44c2a6e7259d4ccf0144948ed5a7ec4ed5d25b2295ff72 | 733a094f719efad1daa940490454ee32a0240ac1e99d4c98a796ca50f1eddef0 | 8.600000 |
| 9 | VO_SN_Ending_09 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_09.ogg | Mother | Gacrux | speech-20261127-153912-6fda4423.wav | acb69ab2b343e37fa5941743fd7efea5a9645cb2bc6a4dc8c8743a19ad3e58c5 | 999b2a4411bff33656b5e8cf04ea50274c7bfdaf67ecf10d710a09accb0635b0 | 5.320000 |
| 9 | VO_SN_Ending_09 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_09.ogg | Mother | Gacrux | speech-20261127-153905-8d783872.wav | 2adb6a2ee7f989e240856aa32d19946df3a4dc10e4d15ff7175838869adc1c76 | eee824c2168f68fd0e4a8e87f4e4df16187e4cf7172a501a56f5419d948e10ab | 5.760000 |
| 10 | VO_SN_Ending_10 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_10.ogg | Nour | Leda | speech-20261127-153933-fae9fa71.wav | 30a14c4d31d8d702626245d56e5144e015c6cb94eb85db7c2b0cbc7bccd401d3 | 676a71b8765226d005d5f9f0caf28e3a470bd5ebeb40620a7109d6cbba888315 | 6.920000 |
| 10 | VO_SN_Ending_10 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_10.ogg | Nour | Leda | speech-20261127-153919-6703d26e.wav | 6ce9af9dc4bd01a15d2ff5a1b5058a4bae052699de0695b88315abd501a0a266 | 65451f0240d0dad7c5da4896e7b52281fa75ba9d7a877583141a0a89160d7085 | 7.280000 |
| 11 | VO_SN_Ending_11 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_11.ogg | Nour | Leda | speech-20261127-153945-9f97d4cb.wav | 47e1fa65bd965daab0065c040cc8079b8c6cb8811ce53684bedff2828e8fd90d | 972a5d854f08c9f137bf4f5dba366b171bc66bd390465c863f278f07ac66f216 | 3.480000 |
| 11 | VO_SN_Ending_11 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_11.ogg | Nour | Leda | speech-20261127-153939-9f155e4b.wav | 1c320cd9531d195439d077033f9918a8fa1c8d220385e320f145b601a2b20b95 | 257c9f1faa955f00feab067727448890644c4e1398da29735eb26b6c719f2524 | 3.880000 |
| 12 | VO_SN_Ending_12 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_12.ogg | Shadow | Enceladus | speech-20261127-154000-1cc57c6b.wav | 3b8a750d3aba81a82718387069979701991048cf0d2d55ec4c76858a93900164 | e37bccc0a22cf3e2bb270e947e3dabd6a9b977336f754d4cb9eac517bedd0bf7 | 7.560000 |
| 12 | VO_SN_Ending_12 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_12.ogg | Shadow | Enceladus | speech-20261127-153952-5a4f9784.wav | 36ade137fe858a080a20246019735f9e6ad50cefae21022963ca2c956b34fe21 | dffec51149de4257463358cf395cf0c3feee1d8fe7b1a5cf6e58e1170d82b7f8 | 4.240000 |
| 13 | VO_SN_Ending_13 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_13.ogg | Memory | Schedar | speech-20261127-154007-6a676dae.wav | caaa8402aca32807b980fc7c359e85410d8b00d06f7ae7959022202fd7167519 | 48f4e28cc15d0eb626aeaa7b8490631c3a071841d01d98855dd569be998d956f | 9.240000 |
| 13 | VO_SN_Ending_13 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_13.ogg | Memory | Schedar | speech-20261127-153732-78aed797.wav | 45b7cc50d7265fa5c63ff87f5f7ade8330f40f5390e99c95e51d7a4bd8eee03e | 724b4cd68426dd6f41c39c2d03fc6ee50da138ac08a9d2c7d8daf5cab18875e1 | 8.600000 |
| 14 | VO_SN_Ending_14 AR | Assets/_Project/Resources/VO/AR/VO_SN_Ending_14.ogg | Memory | Schedar | speech-20261127-154021-4cb03663.wav | 8833cc07a29ed56c28da9f08cce4b601d3c94a3f97b03ad775e69adeba92ed35 | 254e68d2e626e248f4997b5145e76a9524ee8844f893a04891983d6b6cc70e58 | 5.400000 |
| 14 | VO_SN_Ending_14 EN | Assets/_Project/Resources/VO/EN/VO_SN_Ending_14.ogg | Memory | Schedar | speech-20261127-154015-10588097.wav | 27c9b40132a8d87e13380d2db7d339c089506fcfe927976892fe34a4326c6a6a | 369b4ae00693cd990da16cd2bfdb31f4b158863ca8e25e7388b4a5a5854d3d53 | 5.160000 |
