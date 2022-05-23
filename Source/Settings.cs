namespace KerbalTrainingExperience
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class KerbalTrainingExperienceOptions : GameParameters.CustomParameterNode
    {
        public override string Title { get { return KerbalTrainingExperienceUtils.modName; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return KerbalTrainingExperienceUtils.modName; } }
        public override string DisplaySection { get { return KerbalTrainingExperienceUtils.modName; } }
        public override int SectionOrder { get { return 2; } }


        [GameParameters.CustomParameterUI("#KerbalTrainingExperience_modEnable",
            newGameOnly = false,
            unlockedDuringMission = true)]
        public bool enable = true;

        [GameParameters.CustomIntParameterUI("#KerbalTrainingExperience_hoursPerExpPilot",
            toolTip = "#KerbalTrainingExperience_hoursPerExpPilot_toolTip",
            minValue = 1,
            maxValue = 72,
            newGameOnly = false,
            unlockedDuringMission = true)]
        public int hoursPerExpPilot = 36;

        [GameParameters.CustomIntParameterUI("#KerbalTrainingExperience_hoursPerExp",
            toolTip = "#KerbalTrainingExperience_hoursPerExp_toolTip",
            minValue = 1,
            maxValue = 36,
            newGameOnly = false,
            unlockedDuringMission = true)]
        public int hoursPerExp = 18;

        public override bool HasPresets { get { return true; } }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    enable = true;
                    hoursPerExpPilot = 18;
                    hoursPerExp = 9;
                    break;

                case GameParameters.Preset.Normal:
                    enable = true;
                    hoursPerExpPilot = 36;
                    hoursPerExp = 18;
                    break;

                case GameParameters.Preset.Moderate:
                    enable = true;
                    hoursPerExpPilot = 48;
                    hoursPerExp = 24;
                    break;

                case GameParameters.Preset.Hard:
                    enable = true;
                    hoursPerExpPilot = 60;
                    hoursPerExp = 30;
                    break;
            }
        }
    }
}
