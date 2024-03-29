﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP;
using KSPAssets;
using KSP.Localization;

namespace KerbalTrainingExperience
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]{GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION})]
    public class KerbalTrainingExperience : ScenarioModule
    {
        internal static bool ModEnable { get { return HighLogic.CurrentGame.Parameters.CustomParams<KerbalTrainingExperienceOptions>().enable; }}
        internal static int HoursPerExpPilot { get { return HighLogic.CurrentGame.Parameters.CustomParams<KerbalTrainingExperienceOptions>().hoursPerExpPilot; }}
        internal static int HoursPerExp { get { return HighLogic.CurrentGame.Parameters.CustomParams<KerbalTrainingExperienceOptions>().hoursPerExp; }}

        internal static Dictionary<string, KerbalTrainingInfo> kerbalsTrainingInfo = new Dictionary<string, KerbalTrainingInfo>();

        public void Start()
        {
            GameEvents.onKerbalStatusChanged.Add(onKerbalStatusChanged);
            GameEvents.onVesselRecovered.Add(onVesselRecovered);

            KerbalTrainingExperienceUtils.Log("Update all crews' training exp and level on Start");
            foreach (ProtoCrewMember crew in HighLogic.CurrentGame.CrewRoster.Kerbals(
                ProtoCrewMember.KerbalType.Crew, ProtoCrewMember.RosterStatus.Assigned, ProtoCrewMember.RosterStatus.Available
            ))
            {
                if (kerbalsTrainingInfo.ContainsKey(crew.name))
                {
                    UpdateCrewTrainingExp(crew, 0);
                    KerbalTrainingExperienceUtils.Log(string.Format("{0} has {1} XP and level {2}", crew.name, crew.experience, crew.experienceLevel));
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            KerbalTrainingExperienceUtils.Log("Load kerbals' training info");
            base.OnLoad(node);

            if (node.HasNode("Kerbals"))
            {
                node = node.GetNode("Kerbals");
                foreach (ConfigNode kerbalNode in node.GetNodes())
                {
                    string kerbalName = "";
                    double trainingTime = -1.0f;
                    int assignedExtraExp = -1;
                    kerbalNode.TryGetValue("name", ref kerbalName);
                    kerbalNode.TryGetValue("trainingTime", ref trainingTime);
                    kerbalNode.TryGetValue("assignedExtraExp", ref assignedExtraExp);

                    if (kerbalName != string.Empty && trainingTime > 0 && assignedExtraExp >= 0)
                    {
                        if (kerbalsTrainingInfo.ContainsKey(kerbalName))
                        {
                            kerbalsTrainingInfo[kerbalName].trainingTime = trainingTime;
                            kerbalsTrainingInfo[kerbalName].assignedExtraExp = assignedExtraExp;
                        }
                        else
                        {
                            KerbalTrainingInfo info = new KerbalTrainingInfo();
                            info.name = kerbalName;
                            info.trainingTime = trainingTime;
                            info.assignedExtraExp = assignedExtraExp;
                            kerbalsTrainingInfo.Add(kerbalName, info);
                        }
                    }
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            KerbalTrainingExperienceUtils.Log("Save kerbals' training info");
            base.OnSave(node);

            node.RemoveNode("Kerbals");
            ConfigNode kerbals = new ConfigNode("Kerbals");

            foreach (var trainingInfo in kerbalsTrainingInfo)
            {
                ConfigNode kerbal = new ConfigNode("Kerbal");
                kerbal.AddValue("name", trainingInfo.Key);
                kerbal.AddValue("trainingTime", trainingInfo.Value.trainingTime);
                kerbal.AddValue("assignedExtraExp", trainingInfo.Value.assignedExtraExp);
                kerbals.AddNode(kerbal);
            }

            node.AddNode(kerbals);
        }

        public void Update()
        {
            if (!ModEnable)
            {
                return;
            }

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (vessel.LandedOrSplashed || vessel.GetCrewCount() == 0)
                {
                    continue;
                }

                vessel.GetVesselCrew().ForEach(crew =>
                {
                    int trainingHoursPerExp = -1;
                    if (crew.trait == KerbalRoster.pilotTrait)
                    {
                        trainingHoursPerExp = HoursPerExpPilot;
                    }
                    else
                    {
                        trainingHoursPerExp = HoursPerExp;
                    }

                    int crewTrainingExp = GetCrewTrainingExp(crew);
                    int crewExpectedExp = (int)(GetCrewTrainingTime(crew) / (trainingHoursPerExp * 3600));
                    if (crewTrainingExp != crewExpectedExp)
                    {
                        UpdateCrewTrainingExp(crew, crewExpectedExp - crewTrainingExp);
                        KerbalTrainingExperienceUtils.Log(
                            string.Format("{0} has totally {1} XP and level {2} after {3} seconds training",
                            crew.name, crew.experience, crew.experienceLevel, GetCrewTrainingTime(crew))
                        );
                    }
                });
            }
        }

        private int GetCrewTrainingExp(ProtoCrewMember crew)
        {
            if (kerbalsTrainingInfo.ContainsKey(crew.name))
            {
                return kerbalsTrainingInfo[crew.name].assignedExtraExp;
            }

            return 0;
        }

        private void UpdateCrewTrainingExp(ProtoCrewMember crew, int deltaExp)
        {
            if (!kerbalsTrainingInfo.ContainsKey(crew.name))
            {
                return;
            }

            if (deltaExp < -kerbalsTrainingInfo[crew.name].assignedExtraExp)
            {
                deltaExp = -kerbalsTrainingInfo[crew.name].assignedExtraExp;
            }

            kerbalsTrainingInfo[crew.name].assignedExtraExp += deltaExp;
            crew.experience = crew.CalculateExperiencePoints(HighLogic.CurrentGame) + kerbalsTrainingInfo[crew.name].assignedExtraExp;
            crew.experienceLevel = KerbalRoster.CalculateExperienceLevel(crew.experience);
        }

        private void ClearCrewTrainingExp(ProtoCrewMember crew)
        {
            crew.experience = crew.CalculateExperiencePoints(HighLogic.CurrentGame);
            crew.experienceLevel = KerbalRoster.CalculateExperienceLevel(crew.experience);

            if (kerbalsTrainingInfo.ContainsKey(crew.name))
            {
                kerbalsTrainingInfo.Remove(crew.name);
            }
        }

        private double GetCrewTrainingTime(ProtoCrewMember crew)
        {
            if (kerbalsTrainingInfo.ContainsKey(crew.name))
            {
                return kerbalsTrainingInfo[crew.name].trainingTime;
            }

            return 0.0f;
        }

        //Game events

        private void onKerbalStatusChanged(ProtoCrewMember crew, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
        {
            if (newStatus == ProtoCrewMember.RosterStatus.Dead || newStatus == ProtoCrewMember.RosterStatus.Missing)
            {
                ClearCrewTrainingExp(crew);
            }
        }

        private void onVesselRecovered(ProtoVessel vessel, bool value)
        {
            vessel.GetVesselCrew().ForEach(crew =>
            {
                if (kerbalsTrainingInfo.ContainsKey(crew.name))
                {
                    UpdateCrewTrainingExp(crew, 0);
                }
            });
        }
    }
}