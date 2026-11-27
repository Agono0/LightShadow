using LightShadow.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace LightShadow.Tools
{
    public static partial class SceneBuilder
    {
        // Interactive wing doors (user brief 2026-11-27): open/close/locked
        // with keys. The two old grille-decor boxes (no colliders) are
        // replaced by real doors, and the other wing gaps gain unlocked
        // closable ones. Key chain, soft-lock safe: the library stays open
        // and hides the classroom key, the classroom hides the staff key,
        // and the staff room keeps the master key it always had. Roots sit
        // on the Default layer so the LevelLayer-only NavMesh bake ignores
        // them; runtime passage is the DoorController's carving obstacle.
        static void BuildSchoolDoors(Transform parent, Palette palette)
        {
            // West wing (wall x = -2.65; rooms on the -x side, swing into the
            // room with a negative angle).
            PlaceDoor(parent, "Door_Library",   -2.65f, -16f, "",              false, -105f, palette);
            PlaceDoor(parent, "Door_Infirmary", -2.65f, -3f,  "",              false, -105f, palette);
            PlaceDoor(parent, "Door_Classroom", -2.65f, 10f,  "classroom_key", true,  -105f, palette);
            PlaceDoor(parent, "Door_Staff",     -2.65f, 22f,  "staff_key",     true,  -105f, palette);

            // East wing (wall x = +2.65; rooms on the +x side).
            PlaceDoor(parent, "Door_Physics",   2.65f, -16f, "",              false, 105f, palette);
            PlaceDoor(parent, "Door_Garden",    2.65f, 10f,  "",              false, 105f, palette);

            // The key chain (plain pickups for now; moving them behind puzzle
            // solves is the keys-through-puzzles follow-up).
            PlaceKey(parent, "Key_Classroom", "classroom_key", "the classroom key", "مفتاح الفصل",
                new Vector3(-19.5f, 0.98f, -18f), palette);
            PlaceKey(parent, "Key_Staff", "staff_key", "the staff room key", "مفتاح غرفة المعلمين",
                new Vector3(-19.5f, 0.98f, 8f), palette);

            // Two medkits, rare per the brief: the infirmary shelf and the
            // staff room. More arrive as puzzle rewards in a later batch.
            PlaceMedkit(parent, "Medkit_Infirmary", new Vector3(-21.4f, 1.05f, 0f), palette);
            PlaceMedkit(parent, "Medkit_Staff", new Vector3(-19.8f, 0.98f, 24.5f), palette);
        }

        // A hinged wooden door in a corridor-wall gap. The root sits at the
        // hinge edge of the gap (z = gapMin); the panel child extends +z.
        static void PlaceDoor(Transform parent, string name, float x, float zMin,
            string keyId, bool locked, float openAngle, Palette palette)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(x, 0f, zMin);

            var hinge = new GameObject("Hinge");
            hinge.transform.SetParent(root.transform, false);
            hinge.transform.localPosition = Vector3.zero;

            // Panel: thin wooden slab with a handle knob on the corridor side.
            Box(hinge.transform, "Panel", new Vector3(0f, 1.025f, 0.475f),
                new Vector3(0.08f, 2.05f, 0.95f), palette.wood, true);
            float handleSide = x < 0f ? 0.07f : -0.07f;
            Box(hinge.transform, "Handle", new Vector3(handleSide, 1.0f, 0.82f),
                new Vector3(0.05f, 0.05f, 0.14f), palette.metal, false);

            DoorController door = root.AddComponent<DoorController>();
            door.hinge = hinge.transform;
            door.keyId = keyId;
            door.startsLocked = locked;
            door.openAngle = openAngle;

            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.center = new Vector3(0f, 1.0f, 0.475f);
            obstacle.size = new Vector3(0.5f, 2.0f, 1.1f);
        }

        // A small key prop on the master-key pattern: collider under the
        // pickup root, hidden once carried (ItemPickup persistence).
        static void PlaceKey(Transform parent, string name, string itemId,
            string displayEn, string displayAr, Vector3 position, Palette palette)
        {
            var key = new GameObject(name);
            key.transform.SetParent(parent, false);
            Box(key.transform, "KeyShaft", position, new Vector3(0.04f, 0.16f, 0.03f), palette.lamp, false);
            Box(key.transform, "KeyRing", position + new Vector3(0f, 0.12f, 0f),
                new Vector3(0.07f, 0.07f, 0.03f), palette.lamp, true, LevelLayer);
            ItemPickup pickup = key.AddComponent<ItemPickup>();
            pickup.itemId = itemId;
            pickup.displayNameEn = displayEn;
            pickup.displayNameAr = displayAr;
        }

        // A white-cross medkit box prop with the pickup on the root.
        static void PlaceMedkit(Transform parent, string name, Vector3 position, Palette palette)
        {
            var kit = new GameObject(name);
            kit.transform.SetParent(parent, false);
            Box(kit.transform, "Case", position, new Vector3(0.34f, 0.22f, 0.26f), palette.paper, true, LevelLayer);
            Box(kit.transform, "CrossH", position + new Vector3(0f, 0.115f, 0f),
                new Vector3(0.20f, 0.012f, 0.06f), palette.lamp, false);
            Box(kit.transform, "CrossV", position + new Vector3(0f, 0.115f, 0f),
                new Vector3(0.06f, 0.012f, 0.20f), palette.lamp, false);
            kit.AddComponent<MedkitPickup>();
        }
    }
}
