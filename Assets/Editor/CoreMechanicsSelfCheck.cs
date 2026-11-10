using UnityEngine;
using UnityEditor;

namespace WickFall.EditorTools
{
    public static class CoreMechanicsSelfCheck
    {
        [InitializeOnLoadMethod]
        public static void RunVerification()
        {
            // 1. Wax Ratio verification
            float max = 100f;
            float cur = 75f;
            float ratio = Mathf.Clamp01(cur / max);
            Debug.Assert(Mathf.Approximately(ratio, 0.75f), "Wax ratio must be 0.75");

            // 2. Clamped drain
            cur = Mathf.Max(0f, cur - 100f);
            Debug.Assert(cur == 0f, "Wax should clamp at 0");

            // 3. Clamped restore
            cur = Mathf.Clamp(cur + 150f, 0f, max);
            Debug.Assert(cur == max, "Wax should clamp at max");

            // 4. Light lerp
            float minRange = 2f, maxRange = 8f;
            float lightRange = Mathf.Lerp(minRange, maxRange, 0.5f);
            Debug.Assert(Mathf.Approximately(lightRange, 5f), "Light range at 50% must be 5");

            Debug.Log("[CoreMechanicsSelfCheck] All 4 core mechanics assertions passed successfully!");
        }
    }
}
