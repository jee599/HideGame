using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionUI : MonoBehaviour
{
    public MissionManager missionManager;
    public TextMeshProUGUI missionLabel;
    public Image progressFill;

    private void Start()
    {
        if (missionManager == null)
        {
            missionManager = FindFirstObjectByType<MissionManager>();
        }

        if (missionManager == null)
        {
            return;
        }

        missionManager.MissionAssigned += HandleMissionAssigned;
        missionManager.MissionCompleted += HandleMissionCompleted;
        missionManager.MissionProgressChanged += HandleMissionProgress;

        HandleMissionAssigned(missionManager.ActiveMission);
        HandleMissionProgress(missionManager.ActiveMission, missionManager.Progress01);
    }

    private void OnDestroy()
    {
        if (missionManager == null)
        {
            return;
        }

        missionManager.MissionAssigned -= HandleMissionAssigned;
        missionManager.MissionCompleted -= HandleMissionCompleted;
        missionManager.MissionProgressChanged -= HandleMissionProgress;
    }

    private void HandleMissionAssigned(MissionData mission)
    {
        if (missionLabel == null)
        {
            return;
        }

        missionLabel.text = mission != null ? $"Mission: {mission.description}" : "Mission: --";
    }

    private void HandleMissionCompleted(MissionData mission)
    {
        if (missionLabel != null)
        {
            missionLabel.text = "Mission complete";
        }

        if (progressFill != null)
        {
            progressFill.fillAmount = 1f;
        }
    }

    private void HandleMissionProgress(MissionData mission, float progress)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }
    }
}
