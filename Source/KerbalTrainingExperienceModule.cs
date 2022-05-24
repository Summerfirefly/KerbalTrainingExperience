using System.Collections.Generic;
using System.Linq;

namespace KerbalTrainingExperience
{
    class KerbalTrainingExperienceModule : VesselModule
    {
        private double lastUpdateTime = 0.0f;

        new public void Start()
        {
            lastUpdateTime = Planetarium.GetUniversalTime();
            KerbalTrainingExperienceUtils.Log(string.Format("Start Universal Time of {0} = {1}", vessel.GetName(), lastUpdateTime));
            KerbalTrainingExperienceUtils.Log(string.Format("Vessel's mainBody is {0}", vessel.mainBody.name));
        }

        public void Update()
        {
            if (!KerbalTrainingExperience.ModEnable)
            {
                return;
            }

            double currentTime = Planetarium.GetUniversalTime();
            double deltaTime = currentTime - lastUpdateTime;
            if (vessel.GetCrewCount() == 0)
            {
                return;
            }

            if (vessel.LandedOrSplashed && vessel.mainBody.isHomeWorld)
            {
                return;
            }

            Dictionary<string, int> maxTraitLevel = new Dictionary<string, int>();
            maxTraitLevel.Add(KerbalRoster.pilotTrait, 0);
            maxTraitLevel.Add(KerbalRoster.engineerTrait, 0);
            maxTraitLevel.Add(KerbalRoster.scientistTrait, 0);

            vessel.GetVesselCrew().ForEach(crew =>
            {
                if (crew.experienceLevel > maxTraitLevel[crew.trait])
                {
                    maxTraitLevel[crew.trait] = crew.experienceLevel;
                }
            });

            vessel.GetVesselCrew().ForEach(crew =>
            {
                if (crew.trait == KerbalRoster.pilotTrait)
                {
                    if ((!HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet || vessel.Connection.CanComm) ||
                        maxTraitLevel[crew.trait] > crew.experienceLevel)
                    {
                        UpdateCrewTrainingTime(crew, deltaTime);
                    }
                }
                else
                {
                    if ((vessel.situation & Vessel.Situations.ORBITING) != 0 || (vessel.situation & Vessel.Situations.ESCAPING) != 0)
                    {
                        if ((!HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet || vessel.Connection.CanComm) ||
                            maxTraitLevel[crew.trait] > crew.experienceLevel)
                        {
                            UpdateCrewTrainingTime(crew, deltaTime);
                        }
                    }
                }
            });

            lastUpdateTime = currentTime;
        }

        void UpdateCrewTrainingTime(ProtoCrewMember crew, double deltaTime)
        {
            if (KerbalTrainingExperience.kerbalsTrainingInfo.ContainsKey(crew.name))
            {
                KerbalTrainingExperience.kerbalsTrainingInfo[crew.name].trainingTime += deltaTime;
            }
            else
            {
                KerbalTrainingInfo info = new KerbalTrainingInfo();
                info.name = crew.name;
                info.trainingTime = deltaTime;
                info.assignedExtraExp = 0;
                KerbalTrainingExperience.kerbalsTrainingInfo.Add(crew.name, info);
            }
        }
    }
}
