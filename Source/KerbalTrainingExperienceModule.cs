using System.Linq;

namespace KerbalTrainingExperience
{
    class KerbalTrainingExperienceModule : VesselModule
    {
        private double lastUpdateTime = 0.0f;

        new public void Start()
        {
            lastUpdateTime = Planetarium.GetUniversalTime();
            KerbalTrainingExperienceUtils.Log(string.Format("Start Universal Time of {0} = {1}", Vessel.GetName(), lastUpdateTime));
        }

        public void Update()
        {
            double currentTime = Planetarium.GetUniversalTime();
            double deltaTime = currentTime - lastUpdateTime;
            if (Vessel.LandedOrSplashed || Vessel.GetCrewCount() == 0)
            {
                return;
            }

            Vessel.GetVesselCrew().ForEach(crew =>
            {
                if (crew.trait == KerbalRoster.pilotTrait)
                {
                    if (Vessel.Connection.CanComm)
                    {
                        UpdateCrewTrainingTime(crew, deltaTime);
                    }
                    // TODO: if a higher level pilot is on the same vessel, kerbals can also be training
                }
                else
                {
                    if ((Vessel.situation & Vessel.Situations.ORBITING) != 0 || (Vessel.situation & Vessel.Situations.ESCAPING) != 0)
                    {
                        if (Vessel.Connection.CanComm)
                        {
                            UpdateCrewTrainingTime(crew, deltaTime);
                        }
                        // TODO: if a higher level engineer/scientist is on the same vessel, kerbals can also be training
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
