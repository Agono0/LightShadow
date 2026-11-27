# World-space proximity text (Abdelraouf lane)

Status: delivered as a fork-branch input to the owner's combined PR (2026-11-27 agreement; below the 5k standalone-PR rule, no standalone PR opened). AI-prepared on behalf of Abdelraouf Zidan (GitHub Abdo1590). **Not compiled in Unity, no editor run, no scene build, no device run.**

World-space text (e.g. room quiz text) that fades in when the player is close and looking at it: distance gate with hysteresis, view-angle gate, smoothed alpha, optional billboard. Built in code on a child object (`TextMesh` + `MeshRenderer`); the component can sit on any GameObject.

## Files

- `Assets/_Project/Contrib/Abdelraouf/Scripts/ProximityText/ProximityTextMath.cs` - pure gating math (no UnityEngine): hysteresis, angle, alpha stepping.
- `.../ProximityText.cs` - the component: content (EN/AR), gating fields, fade, billboard, material resolution.
- `.../IProximityTextMaterialProvider.cs` - the material handoff interface.
- `.../ResourcesProximityTextMaterial.cs` - provider loading the owner's material from Resources by path.
- `.../ProximityTextManager.cs` - one Update + one camera fetch for all texts.
- `Assets/_Project/Contrib/Abdelraouf/Tests/Editor/ProximityTextMathTests.cs` - EditMode tests.

## Material contract (2026-11-27) - coordination needed

The owner's side adds a **build-included ZTest LEqual text shader and material in shared Resources**. This module never writes into shared Resources and never edits shared shaders.

- Resource path to coordinate: `ResourcesProximityTextMaterial.DefaultResourcePath = "ProximityText/DepthTestedTextMaterial"` (a `Resources.Load<Material>` path, so the asset would live at `Assets/_Project/Resources/ProximityText/DepthTestedTextMaterial.mat` on their side - confirm or pick another).
- Usage: put a `ResourcesProximityTextMaterial` component next to each `ProximityText` (or one shared provider in the scene) and assign it to `ProximityText.materialProviderSource`. Any custom `IProximityTextMaterialProvider` works too.
- **Fallback is honest:** with no provider (or a missing material) the text renders with the font's default material, which is NOT depth-tested (draws through walls). A warning is logged once. Nothing in this module claims depth testing without the owner's material.

## Usage

Add `ProximityText` to any GameObject, set `textEn`/`textAr` (or call `SetText(en, ar)`), tune `showRadius`/`hideRadius`/`maxAngleDegrees`/`fadePerSecond`. Placement in rooms is the owner's wiring (no scene YAML from this lane). The manager self-creates when the first text registers.

## Known limits

- `TextMesh` does no right-to-left layout: keep Arabic strings short or break lines manually; needs an Arabic eyeball pass in editor/device.
- `Camera.main` is cached by the manager and re-fetched when null; a camera that never tags `MainCamera` leaves every text hidden.
- Font swaps follow `Localization.LanguageChanged`.
