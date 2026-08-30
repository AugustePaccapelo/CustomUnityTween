using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Tweening;
using Tweening.EditorScripts;

namespace Tweening.Tests
{
    /// <summary>
    /// Covers the 1.0 asset upgrader.
    ///
    /// The behaviour it has to have: rewrite the managed reference identifiers that 1.0 wrote
    /// (global namespace, Assembly-CSharp) to the ones 1.1 uses (Tweening, TweenCore), touch
    /// nothing else in the file, and be safe to run twice.
    ///
    /// The last test is the one that matters: it runs the upgrader over the committed
    /// SampleScene - which is still in the broken 1.0 form - and checks that Unity can then
    /// actually deserialize the properties. Everything above it is detail.
    /// </summary>
    public class AssetUpgraderTests
    {
        private const string OLD_REF =
            "    - rid: 123456789\n" +
            "      type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: , asm: Assembly-CSharp}\n";

        private const string NEW_REF =
            "    - rid: 123456789\n" +
            "      type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: Tweening, asm: TweenCore}\n";

        // ----- The rewrite itself ----- \\

        [Test]
        public void AV10Identifier_IsRewrittenToTheV11Form()
        {
            int rewritten;

            string result = TweenCoreAssetUpgrader.UpgradeText(OLD_REF, out rewritten);

            Assert.AreEqual(1, rewritten);
            Assert.AreEqual(NEW_REF, result);
        }

        [Test]
        public void EveryIdentifierInTheFile_IsRewritten()
        {
            int rewritten;

            TweenCoreAssetUpgrader.UpgradeText(OLD_REF + OLD_REF + OLD_REF, out rewritten);

            Assert.AreEqual(3, rewritten);
        }

        [Test]
        public void AnAlreadyUpgradedFile_IsLeftAlone()
        {
            // Running the upgrader twice must be safe.
            int rewritten;

            string result = TweenCoreAssetUpgrader.UpgradeText(NEW_REF, out rewritten);

            Assert.AreEqual(0, rewritten);
            Assert.AreEqual(NEW_REF, result);
        }

        [Test]
        public void AnUnrelatedManagedReference_IsNotTouched()
        {
            // Somebody else's type, also in Assembly-CSharp. Not ours to move.
            string other =
                "    - rid: 42\n" +
                "      type: {class: PlayerInventory, ns: , asm: Assembly-CSharp}\n";

            int rewritten;
            string result = TweenCoreAssetUpgrader.UpgradeText(other, out rewritten);

            Assert.AreEqual(0, rewritten);
            Assert.AreEqual(other, result);
        }

        [Test]
        public void AnIdentifierInADifferentAssembly_IsNotTouched()
        {
            // A TweenCoreProperty already living somewhere else is not a 1.0 asset.
            //
            // The namespace is left empty deliberately. With `ns: Tweening` the namespace guard
            // rejects this case first and the assembly guard is never reached - which is exactly
            // how it went unverified: dropping the assembly check broke nothing.
            string elsewhere =
                "      type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: , asm: SomeOtherAssembly}\n";

            int rewritten;
            string result = TweenCoreAssetUpgrader.UpgradeText(elsewhere, out rewritten);

            Assert.AreEqual(0, rewritten);
            Assert.AreEqual(elsewhere, result);
        }

        [Test]
        public void AnIdentifierAlreadyInANamespace_IsNotTouched()
        {
            // Still in Assembly-CSharp, but it declares a namespace, so it is not the 1.0 layout
            // this upgrader knows how to move. Isolates the namespace guard: every other case here
            // is also caught by the assembly guard, so without this one that check is unverified.
            string namespaced =
                "      type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: SomebodyElse, asm: Assembly-CSharp}\n";

            int rewritten;
            string result = TweenCoreAssetUpgrader.UpgradeText(namespaced, out rewritten);

            Assert.AreEqual(0, rewritten);
            Assert.AreEqual(namespaced, result);
        }

        [Test]
        public void EveryValueTypeIsHandled_NotJustVector3()
        {
            string floatRef =
                "      type: {class: 'TweenCoreProperty`1[[System.Single, mscorlib]]', ns: , asm: Assembly-CSharp}\n";

            int rewritten;
            string result = TweenCoreAssetUpgrader.UpgradeText(floatRef, out rewritten);

            Assert.AreEqual(1, rewritten);
            StringAssert.Contains("ns: Tweening, asm: TweenCore", result);
            StringAssert.Contains("System.Single", result, "The value type must survive the rewrite.");
        }

        [Test]
        public void TheRestOfTheFile_IsUntouched()
        {
            string file =
                "%YAML 1.1\n" +
                "GameObject:\n" +
                "  m_Name: Circle\n" +
                OLD_REF +
                "  m_Something: 3\n";

            int rewritten;
            string result = TweenCoreAssetUpgrader.UpgradeText(file, out rewritten);

            Assert.AreEqual(1, rewritten);
            StringAssert.StartsWith("%YAML 1.1\n", result);
            StringAssert.Contains("m_Name: Circle", result);
            StringAssert.EndsWith("  m_Something: 3\n", result);
        }

        [Test]
        public void AFileWithNothingToDo_IsReturnedUnchanged()
        {
            string file = "%YAML 1.1\nGameObject:\n  m_Name: Circle\n";

            int rewritten;
            string result = TweenCoreAssetUpgrader.UpgradeText(file, out rewritten);

            Assert.AreEqual(0, rewritten);
            Assert.AreEqual(file, result);
        }

        // ----- The one that proves it works ----- \\

        [Test]
        public void ARealV10Scene_HasItsPropertiesRestored()
        {
            // The fixture is a copy of SampleScene as 1.0 authored it, kept as .txt so Unity does
            // not import it. Upgrade it, write the result as a scene, open it, and count what
            // Unity manages to deserialize.
            //
            // It is a committed fixture rather than the live SampleScene on purpose: once the real
            // scene is upgraded there would be nothing broken left to test against, and this test
            // would quietly start proving nothing.
            const string fixture = "Assets/TweenCore/Tests/Editor/Fixtures/SampleScene-v1.0.unity.txt";
            const string copy = "Assets/Scenes/_UpgraderFixture.unity";

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string full = Path.Combine(projectRoot, copy);

            try
            {
                int rewritten;
                string upgraded = TweenCoreAssetUpgrader.UpgradeText(
                    File.ReadAllText(Path.Combine(projectRoot, fixture)),
                    out rewritten);

                Assert.AreEqual(10, rewritten, "The 1.0 fixture holds ten managed references to upgrade.");

                File.WriteAllText(full, upgraded);
                UnityEditor.AssetDatabase.Refresh();

                EditorSceneManager.OpenScene(copy, OpenSceneMode.Single);

                TweenCoreComponent[] components = Object.FindObjectsByType<TweenCoreComponent>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);

                Assert.Greater(components.Length, 0, "The fixture scene should contain tween components.");

                int alive = 0;
                foreach (TweenCoreComponent component in components)
                {
                    alive += CountDeserializedProperties(component);
                }

                Assert.AreEqual(10, alive,
                    "All ten real managed references in SampleScene should deserialize after the upgrade.");
            }
            finally
            {
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
                if (File.Exists(full)) File.Delete(full);
                if (File.Exists(full + ".meta")) File.Delete(full + ".meta");
                UnityEditor.AssetDatabase.Refresh();
            }
        }

        private static int CountDeserializedProperties(TweenCoreComponent component)
        {
            System.Reflection.FieldInfo field = typeof(TweenCoreComponent).GetField(
                "_properties",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            System.Collections.IList list = field.GetValue(component) as System.Collections.IList;
            if (list == null) return 0;

            int alive = 0;
            foreach (object entry in list)
            {
                if (entry != null) alive++;
            }

            return alive;
        }
    }
}
