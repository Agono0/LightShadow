using System;
using System.Collections.Generic;

namespace LightShadow.Contrib.Abdelraouf.InventoryScreen
{
    // Pure view model behind the inventory screen: a stable, sorted snapshot
    // of the carried item ids plus grid selection state. No UnityEngine
    // references, so the EditMode tests drive it without a scene.
    //
    // PROVISIONAL SNAPSHOT CONTRACT (agreed with the owner's side 2026-11-27):
    // Inventory.Carried today is an unordered IEnumerable over a HashSet and
    // Inventory.Remove fires no event. The owner's side will add a stable
    // ordered Carried snapshot API and an Inventory.ItemRemoved event; the
    // exact signatures are pending, so they are NOT used here. Until they
    // land, Refresh copies Carried into a List and sorts it with
    // StringComparer.Ordinal, which keeps the order stable across refreshes
    // while the set of ids is unchanged. When their snapshot API ships,
    // feed it into Refresh and delete the sort.
    public class InventoryScreenModel
    {
        readonly List<string> items = new List<string>();
        int columns = 4;
        int selectedIndex;

        public int Columns
        {
            get => columns;
            set => columns = Math.Max(1, value);
        }

        public int Count => items.Count;
        public int SelectedIndex => selectedIndex;
        public string SelectedId => items.Count == 0 ? null : items[selectedIndex];
        public IReadOnlyList<string> Items => items;

        // Rebuilds the snapshot from the live inventory and keeps the
        // selection on a valid row when items disappeared.
        public void Refresh(IEnumerable<string> ids)
        {
            items.Clear();
            if (ids != null)
            {
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id)) items.Add(id);
                }
            }
            items.Sort(StringComparer.Ordinal);
            ClampSelection();
        }

        void ClampSelection()
        {
            if (items.Count == 0) selectedIndex = 0;
            else if (selectedIndex >= items.Count) selectedIndex = items.Count - 1;
            if (selectedIndex < 0) selectedIndex = 0;
        }

        public bool Select(int index)
        {
            if (index < 0 || index >= items.Count) return false;
            selectedIndex = index;
            return true;
        }

        // Left/right wrap inside the row; up/down wrap between rows and keep
        // the column where the target row has one.
        public bool MoveLeft() => Move(-1, 0);
        public bool MoveRight() => Move(1, 0);
        public bool MoveUp() => Move(0, -1);
        public bool MoveDown() => Move(0, 1);

        public bool Move(int deltaColumn, int deltaRow)
        {
            if (items.Count == 0) return false;

            int row = selectedIndex / columns;
            int col = selectedIndex % columns;
            int rows = (items.Count + columns - 1) / columns;

            if (deltaColumn != 0)
            {
                int rowStart = row * columns;
                int colsInRow = Math.Min(columns, items.Count - rowStart);
                col = PositiveMod(col + deltaColumn, colsInRow);
                selectedIndex = rowStart + col;
                return true;
            }

            if (deltaRow != 0)
            {
                row = PositiveMod(row + deltaRow, rows);
                int next = row * columns + col;
                if (next >= items.Count) next = items.Count - 1;
                selectedIndex = next;
                return true;
            }

            return false;
        }

        static int PositiveMod(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
