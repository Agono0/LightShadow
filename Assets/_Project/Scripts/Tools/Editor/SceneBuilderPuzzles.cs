using LightShadow.Gameplay;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    // Wing puzzle placement (finish-the-game plan, workstream 5). Puzzle 1:
    // the classroom chalkboard cipher (Docs/PUZZLES.md). The sums are chalked
    // on the classroom board; the lock plate and drawer sit on the teacher's
    // desk under one root that carries the puzzle component, so the
    // InteractionRaycaster resolves a hit on any part to the puzzle.
    public static partial class SceneBuilder
    {
        static void CreateChalkboardCipher(Prefabs prefabs, Transform parent, Palette palette)
        {
            // The chalk sums each hide one digit; read top to bottom: 6371.
            // The board face points into the room (-Z), so the text rotation is
            // yaw 180 (CreateWorldText reads correctly on the -forward side).
            CreateWorldText(parent, "Cipher_Chalk",
                "7 × 8 = 5_\n9 × 4 = _6\n11 × 7 = 7_\n9 × 9 = 8_",
                new Vector3(-12f, 1.76f, 18.06f), Quaternion.Euler(0f, 180f, 0f),
                0.024f, 72, new Color(0.88f, 0.90f, 0.86f));

            var root = new GameObject("Puzzle_ChalkboardCipher");
            root.transform.SetParent(parent, false);

            // The drawer under the teacher desk top (the desk is an open frame:
            // top slab spans y 0.73-0.83, so the drawer hangs just below it and
            // slides out of the front on solve).
            GameObject drawer = Box(root.transform, "Drawer", new Vector3(-12f, 0.60f, 16.02f),
                new Vector3(0.9f, 0.22f, 0.5f), palette.wood, true, LevelLayer);

            // The small lock plate proud of the drawer front.
            Box(root.transform, "LockPlate", new Vector3(-12f, 0.60f, 15.74f),
                new Vector3(0.3f, 0.12f, 0.06f), palette.metal, true, LevelLayer);

            // The payout: a battery resting on the drawer (inactive until
            // solved). Parented to the puzzle root, not the drawer - the drawer
            // is a scaled cube and would squash a child. On solve the puzzle
            // slides both out together.
            GameObject battery = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.battery);
            battery.name = "CipherReward_Battery";
            battery.transform.SetParent(root.transform, false);
            battery.transform.position = new Vector3(-12f, 0.72f, 16.02f);
            battery.SetActive(false);

            ChalkboardCipherPuzzle puzzle = root.AddComponent<ChalkboardCipherPuzzle>();
            puzzle.displayNameEn = "the teacher's desk lock";
            puzzle.displayNameAr = "قفل درج المدرّسة";
            puzzle.hintsEn = new[]
            {
                "Each chalk line hides exactly one digit.",
                "Do each sum, then find the digit that was rubbed away.",
                "Read the four missing digits from top to bottom.",
            };
            puzzle.hintsAr = new[]
            {
                "كل سطر على السبورة يخفي رقمًا واحدًا.",
                "احسب كل عملية، ثم اعثر على الرقم الممحو.",
                "اقرأ الأرقام الأربعة الناقصة من فوق إلى تحت.",
            };
            puzzle.drawer = drawer.transform;
            puzzle.rewardRoot = battery;
        }

        // Wing puzzle 2 (Docs/PUZZLES.md): the library card catalogue. A cabinet
        // with four subject drawer fronts against the library's west wall,
        // facing east into the room; the return tray slides out on solve.
        static void CreateCatalogueSort(Prefabs prefabs, Transform parent, Palette palette)
        {
            var root = new GameObject("Puzzle_CatalogueSort");
            root.transform.SetParent(parent, false);

            Box(root.transform, "Cabinet", new Vector3(-21.3f, 0.9f, -11.5f),
                new Vector3(0.6f, 1.8f, 1.6f), palette.wood, true, LevelLayer);
            for (int i = 0; i < 4; i++)
            {
                Box(root.transform, "DrawerFront_" + (i + 1), new Vector3(-20.97f, 0.38f + i * 0.4f, -11.5f),
                    new Vector3(0.05f, 0.36f, 1.4f), palette.wood, true, LevelLayer);
            }

            // The return tray low on the cabinet front; slides east on solve.
            GameObject tray = Box(root.transform, "ReturnTray", new Vector3(-20.9f, 0.14f, -11.5f),
                new Vector3(0.26f, 0.14f, 0.9f), palette.metal, true, LevelLayer);

            GameObject battery = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.battery);
            battery.name = "CatalogueReward_Battery";
            battery.transform.SetParent(root.transform, false);
            battery.transform.position = new Vector3(-20.85f, 0.28f, -11.5f);
            battery.SetActive(false);

            CatalogueSortPuzzle puzzle = root.AddComponent<CatalogueSortPuzzle>();
            puzzle.displayNameEn = "the card catalogue";
            puzzle.displayNameAr = "فهرس البطاقات";
            puzzle.hintsEn = new[]
            {
                "Every card names a book; every drawer names a kind of book.",
                "The Mu'allaqa is a poem, and the Canon will not cure your shelving.",
                "Taha Hussein wrote about his own life; al-Tabari wrote about everyone's.",
            };
            puzzle.hintsAr = new[]
            {
                "كل بطاقة تذكر كتابًا، وكل درج يذكر نوعًا من الكتب.",
                "المعلقة قصيدة، والقانون لن يعالج رفوفك.",
                "طه حسين كتب عن حياته، والطبري كتب عن حياة الجميع.",
            };
            puzzle.tray = tray.transform;
            puzzle.rewardRoot = battery;
        }

        // Wing puzzle 3 (plan, workstream 5): the science-lab optics bench. A
        // torch emitter, two mirrors and a prism on Bench_2, a sensor at the
        // far end, and the lab cabinet's drawer as the payout. The rig is
        // decorative in the world; the overlay owns the rotations.
        static void CreateOpticsBench(Prefabs prefabs, Transform parent, Palette palette)
        {
            var root = new GameObject("Puzzle_OpticsBench");
            root.transform.SetParent(parent, false);

            // Bench_2 spans x 12-16 at z -18 with its top near y 0.9.
            Box(root.transform, "Emitter", new Vector3(12.6f, 1.02f, -18f),
                new Vector3(0.3f, 0.24f, 0.24f), palette.lamp, true, LevelLayer);
            GameObject mirrorA = Box(root.transform, "MirrorA", new Vector3(13.4f, 1.06f, -18f),
                new Vector3(0.06f, 0.32f, 0.26f), palette.metal, true, LevelLayer);
            mirrorA.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            GameObject prism = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prism.name = "Prism";
            prism.transform.SetParent(root.transform, false);
            prism.transform.localPosition = new Vector3(14f, 1.04f, -18f);
            prism.transform.localScale = new Vector3(0.16f, 0.14f, 0.16f);
            Object.DestroyImmediate(prism.GetComponent<Collider>());
            prism.GetComponent<Renderer>().sharedMaterial = palette.water;
            GameObject mirrorB = Box(root.transform, "MirrorB", new Vector3(14.6f, 1.06f, -18f),
                new Vector3(0.06f, 0.32f, 0.26f), palette.metal, true, LevelLayer);
            mirrorB.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
            Box(root.transform, "Sensor", new Vector3(15.4f, 1.02f, -18f),
                new Vector3(0.2f, 0.24f, 0.24f), palette.indicatorOff, true, LevelLayer);

            // The payout drawer low on the lab cabinet's west face (the cabinet
            // spans x 19.7-21.3, z -10.2--7.8), sliding west on solve.
            GameObject drawer = Box(root.transform, "CabinetDrawer", new Vector3(19.64f, 0.62f, -9f),
                new Vector3(0.16f, 0.34f, 1.2f), palette.wood, true, LevelLayer);

            GameObject battery = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.battery);
            battery.name = "OpticsReward_Battery";
            battery.transform.SetParent(root.transform, false);
            battery.transform.position = new Vector3(19.64f, 0.86f, -9f);
            battery.SetActive(false);

            OpticsBenchPuzzle puzzle = root.AddComponent<OpticsBenchPuzzle>();
            puzzle.displayNameEn = "the optics bench";
            puzzle.displayNameAr = "منصة البصريات";
            puzzle.hintsEn = new[]
            {
                "Light bounces off a mirror at the same angle it arrives.",
                "The prism bends the beam; it needs the steepest turn of the three.",
                "Mirror A turns the beam once, the prism twice, Mirror B three quarter-turns.",
            };
            puzzle.hintsAr = new[]
            {
                "يرتد الضوء عن المرآة بنفس الزاوية التي يصل بها.",
                "المنشور يكسر الشعاع؛ يحتاج أكبر انعطافة بين العناصر الثلاثة.",
                "المرآة أ تدور ربع دورة، والمنشور ربعين، والمرآة ب ثلاثة أرباع.",
            };
            puzzle.drawer = drawer.transform;
            puzzle.rewardRoot = battery;
        }
    }
}
