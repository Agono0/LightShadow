# Arabic Table Proposals (Input for LightShadow Localization Audit)

**Status: AI-drafted, preliminary. Native Arabic review needed. NOT teacher-verified. This is not a release approval.**

These are proposed Arabic strings for HUD/prompt text that currently exists in code only as hardcoded English (audit v3 section 3.4 follow-up). Nothing here is wired into the game; this file is a proposal for the maintainer's localization table. String keys below are suggested names only.

## Scope and method

- Source sweep of the current `feat/audit-fixes` tip for user-facing English literals that bypass the localization table.
- The three prompts named in audit v3 section 3.4 ("Pick it up", "Put it down", "Take the medkit") are already fixed on the current tip, so they are not listed here.
- The `"Unlocked with the " + key + "."` concatenation in `DoorController.cs` was replaced upstream on the current tip with `string.Format(Localization.Tr("Unlocked with the {0}."), ...)`, so it is no longer a proposal item; its Arabic table entry belongs to the maintainer's localization table work.

## A. Interaction prompts (2 strings, strictly English-only today)

Found in `Assets/_Project/Scripts/Gameplay/DoorController.cs` (Prompt property): returned directly, no `Localization.Tr` call.

| Suggested key | English (exact code string) | Proposed Arabic | Note |
|---|---|---|---|
| `prompt.door.open` | Open the door | افتح الباب | Player-facing action prompt on a closed door. |
| `prompt.door.close` | Close the door | أغلق الباب | Player-facing action prompt on an open door. |

## B. Adjacent HUD messages (5 strings)

These do go through the HUD message path but have no localization-table entries on the current tip.

| Suggested key | English (exact code string) | Proposed Arabic | Source location |
|---|---|---|---|
| `hud.wraith.hit` | It struck her - she is barely standing. Find a medkit. | أصابها - إنها بالكاد تستطيع الوقوف. ابحث عن حقيبة إسعافات. | `GameManager.cs` (wraith hit, survived) |
| `hud.door.locked` | Locked. The key must be somewhere in this school. | مقفل. لا بد أن المفتاح في مكان ما داخل هذه المدرسة. | `DoorController.cs` (locked door, no key) |
| `hud.medkit.packed` | Medkit packed. Press M when she needs it. | تم حزم حقيبة الإسعافات. اضغط M عندما تحتاجها. | `MedkitPickup.cs` |
| `hud.medkit.used` | Medkit used. Back on her feet. | تم استخدام حقيبة الإسعافات. عادت إلى قدميها. | `InventoryUseHandlers.cs`, `PlayerHealth.cs`, `TouchButton.cs` |
| `hud.medkit.none` | No medkit. They are rare - some puzzles hide one. | لا توجد حقيبة إسعافات. إنها نادرة - بعض الألغاز يخفي واحدة. | `PlayerHealth.cs` |

## Notes for the native reviewer

- "Medkit" is rendered as حقيبة إسعافات (first-aid kit) throughout for consistency; بديل شائع في الألعاب هو "علبة إسعافات" - please pick one house term and apply it to all four medkit strings.
- "Press M" keeps the Latin key name M, matching how the quiz assets handle control hints; confirm this matches the project's convention for Arabic players.
- `hud.wraith.hit`: the pronoun "It" refers to the wraith enemy; the Arabic keeps it neutral (أصابها, "it struck her"). If the house style prefers naming the attacker, an alternative is أصابها الشبح.
- Tone: the game addresses the player about a girl protagonist ("she"); the proposals keep that third-person framing.
