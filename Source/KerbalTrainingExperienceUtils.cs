using UnityEngine;

namespace KerbalTrainingExperience
{
    class KerbalTrainingExperienceUtils
    {
        internal static string modName = "Kerbal Training Experience";

        internal static void Log(string msg)
        {
            Debug.Log(string.Format("[{0}] {1}", modName, msg));
        }
    }
}
