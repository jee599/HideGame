using UnityEngine;

public enum TransportMode
{
    Walk,
    Bus,
    Fast
}

[CreateAssetMenu(menuName = "BlendIn/ScheduleTable")]
public class ScheduleTable : ScriptableObject
{
    [System.Serializable]
    public struct ScheduleSlot
    {
        [Range(0f, 24f)] public float gameHour;
        public ScheduleOption[] options;
    }

    [System.Serializable]
    public struct ScheduleOption
    {
        public string destinationTag;
        [Min(0f)] public float probability;
        public TransportMode transport;
        [Min(0f)] public float waitSeconds;
    }

    public ScheduleSlot[] slots;

    public ScheduleOption GetNextAction(float currentHour)
    {
        return GetActionAtSlot(GetCurrentSlotIndex(currentHour));
    }

    public int GetCurrentSlotIndex(float currentHour)
    {
        if (slots == null || slots.Length == 0)
        {
            return -1;
        }

        var selectedIndex = 0;
        for (var i = 0; i < slots.Length; i++)
        {
            if (currentHour >= slots[i].gameHour)
            {
                selectedIndex = i;
                continue;
            }

            break;
        }

        return selectedIndex;
    }

    public ScheduleOption GetActionAtSlot(int slotIndex)
    {
        if (slots == null || slots.Length == 0 || slotIndex < 0 || slotIndex >= slots.Length)
        {
            return default;
        }

        var options = slots[slotIndex].options;
        if (options == null || options.Length == 0)
        {
            return default;
        }

        var totalWeight = 0f;
        for (var i = 0; i < options.Length; i++)
        {
            totalWeight += Mathf.Max(0f, options[i].probability);
        }

        if (totalWeight <= 0f)
        {
            return options[Random.Range(0, options.Length)];
        }

        var roll = Random.Range(0f, totalWeight);
        var runningWeight = 0f;
        for (var i = 0; i < options.Length; i++)
        {
            runningWeight += Mathf.Max(0f, options[i].probability);
            if (roll <= runningWeight)
            {
                return options[i];
            }
        }

        return options[options.Length - 1];
    }
}
