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
    }
}
