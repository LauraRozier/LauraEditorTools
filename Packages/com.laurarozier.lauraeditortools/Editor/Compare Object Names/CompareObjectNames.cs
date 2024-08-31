#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LauraEditor.Tools.Editor.Extensions;

namespace LauraEditor.Tools.Editor {
    public static class CompareObjectNames
    {
        private static readonly Dictionary<string, List<Transform>> Selections =
            new Dictionary<string, List<Transform>>();

        [MenuItem("Tools/LauraRozier/Compare Object Names/Clear Selection", false, 1)]
        private static void ClearSelection() =>
            Selections.Clear();

        [MenuItem("Tools/LauraRozier/Compare Object Names/Add Transforms To Selection", true, 2)]
        private static bool SomethingSelected() =>
            Selection.transforms != null;
        [MenuItem("Tools/LauraRozier/Compare Object Names/Add Transforms To Selection", false, 2)]
        private static void AddSelections()
        {
            foreach (Transform selection in Selection.transforms)
                AddSelection(selection);
        }

        [MenuItem("Tools/LauraRozier/Compare Object Names/Compare Selection", true, 3)]
        public static bool SomethingToCompare() =>
            Selections.Count > 1;
        [MenuItem("Tools/LauraRozier/Compare Object Names/Compare Selection", false, 3)]
        private static void CompareSelection()
        {
            bool flag = false;
            Debug.Log("Searching differences in selections.");

            foreach (var selection in Selections.Keys)
                flag |= selection.Compare();

            if (!flag)
                Debug.Log("No differences found!");
        }

        private static void AddSelection(Transform transform)
        {
            string name = transform.parent.name;

            if (Selections.Keys.Contains(name))
                return;

            Selections.Add(name, new List<Transform>());

            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
                Selections[name].Add(child);
        }

        private static bool Compare(this string selection)
        {
            bool flag = false;

            foreach (var toCompare in Selections.Keys.Where(toCompare => selection != toCompare))
            {
                List<Transform> differences = Selections[toCompare].Except(
                    Selections[selection],
                    (s, c) => s.name == c.name
                ).ToList();

                if (differences.Count == 0)
                    continue;

                flag = true;
                Debug.Log($"Comparing {selection} to {toCompare}: ");

                foreach (Transform difference in differences)
                    Debug.Log(difference.name, difference.gameObject);
            }

            return flag;
        }
    }
}
#endif
