#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Author : Auguste Paccapelo

namespace Tweening.EditorScripts
{
    /// <summary>
    /// Repairs scenes and prefabs authored with TweenCore 1.0.
    ///
    /// 1.0 lived in the global namespace inside Assembly-CSharp. 1.1 moved it to the Tweening
    /// namespace in its own TweenCore assembly. Unity records the type of a [SerializeReference]
    /// field by namespace and assembly, so every property serialized by 1.0 is stored as
    ///
    ///   type: {class: 'TweenCoreProperty`1[[...]]', ns: , asm: Assembly-CSharp}
    ///
    /// which 1.1 cannot resolve : the properties come back null and the tween does nothing. The
    /// values are still in the file, only the label is wrong, so rewriting the label restores
    /// them.
    ///
    /// Run this BEFORE opening and saving any 1.0 asset in 1.1. Unity drops managed references it
    /// cannot resolve when it writes a file, and once an asset has been re-saved in the broken
    /// state the values really are gone and nothing can recover them.
    /// </summary>
    public static class TweenCoreAssetUpgrader
    {
        private const string NEW_NAMESPACE = "Tweening";
        private const string NEW_ASSEMBLY = "TweenCore";
        private const string OLD_ASSEMBLY = "Assembly-CSharp";

        /// <summary>Every serializable property type this package has ever had starts with this.</summary>
        private const string TWEEN_PROPERTY_PREFIX = "TweenCoreProperty";

        private static readonly Regex ManagedReference = new Regex(
            @"type: \{class: (?<cls>'[^']*'|[^,{}]*), ns: (?<ns>[^,]*), asm: (?<asm>[^}]*)\}",
            RegexOptions.Compiled);

        /// <summary>
        /// Rewrites the managed reference type identifiers in the text of a scene or prefab.
        /// Identifiers that are not ours, or that already name the 1.1 assembly, are left alone,
        /// so running this twice is safe.
        /// </summary>
        /// <param name="text">The whole file contents.</param>
        /// <param name="rewritten">How many identifiers were changed.</param>
        /// <returns>The new contents, or the original text when there was nothing to do.</returns>
        public static string UpgradeText(string text, out int rewritten)
        {
            int count = 0;

            string result = ManagedReference.Replace(text, match =>
            {
                string cls = match.Groups["cls"].Value;
                string ns = match.Groups["ns"].Value.Trim();
                string asm = match.Groups["asm"].Value.Trim();

                if (!cls.Trim('\'').StartsWith(TWEEN_PROPERTY_PREFIX)) return match.Value;
                if (ns.Length != 0) return match.Value;
                if (asm != OLD_ASSEMBLY) return match.Value;

                count++;
                return string.Format("type: {{class: {0}, ns: {1}, asm: {2}}}", cls, NEW_NAMESPACE, NEW_ASSEMBLY);
            });

            rewritten = count;
            return count == 0 ? text : result;
        }

        /// <summary>
        /// Upgrades one file in place. Returns how many identifiers were rewritten; the file is
        /// only written when that is greater than zero.
        /// </summary>
        public static int UpgradeFile(string absolutePath)
        {
            int rewritten;
            string original = File.ReadAllText(absolutePath);
            string upgraded = UpgradeText(original, out rewritten);

            if (rewritten > 0) File.WriteAllText(absolutePath, upgraded);

            return rewritten;
        }

        // ----- Menu ----- \\

        [MenuItem("Tools/TweenCore/Upgrade 1.0 scenes and prefabs")]
        private static void UpgradeProject()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            List<string> candidates = new List<string>();
            candidates.AddRange(Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories));
            candidates.AddRange(Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories));

            List<string> touched = new List<string>();
            int total = 0;

            foreach (string path in candidates)
            {
                int rewritten;
                string original = File.ReadAllText(path);
                UpgradeText(original, out rewritten);

                if (rewritten > 0)
                {
                    touched.Add(path);
                    total += rewritten;
                }
            }

            if (total == 0)
            {
                EditorUtility.DisplayDialog("TweenCore",
                    "Nothing to upgrade : no asset holds a TweenCore 1.0 property reference.", "OK");
                return;
            }

            StringBuilder list = new StringBuilder();
            for (int i = 0; i < touched.Count && i < 20; i++)
            {
                list.AppendLine("  " + touched[i].Substring(projectRoot.Length + 1));
            }
            if (touched.Count > 20) list.AppendLine(string.Format("  ... and {0} more", touched.Count - 20));

            bool go = EditorUtility.DisplayDialog("TweenCore",
                string.Format("{0} property reference(s) in {1} asset(s) were written by TweenCore 1.0 " +
                              "and cannot be loaded by 1.1.\n\n{2}\nUpgrade them now? Close any open scene first, " +
                              "and back the project up if it is not under version control.",
                              total, touched.Count, list),
                "Upgrade", "Cancel");

            if (!go) return;

            int changed = 0;
            foreach (string path in touched) changed += UpgradeFile(path);

            AssetDatabase.Refresh();

            Debug.Log(string.Format("{0} : upgraded {1} property reference(s) across {2} asset(s).",
                nameof(TweenCoreAssetUpgrader), changed, touched.Count));

            EditorUtility.DisplayDialog("TweenCore",
                string.Format("Upgraded {0} property reference(s) across {1} asset(s).", changed, touched.Count), "OK");
        }
    }
}
#endif
