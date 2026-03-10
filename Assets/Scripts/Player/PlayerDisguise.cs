using System;
using System.Collections;
using UnityEngine;

public class PlayerDisguise : MonoBehaviour
{
    public int maxDisguises = 3;
    public int remainingDisguises;
    public float disguiseTime = 3f;
    public bool isDisguising;

    public event Action<float> ProgressChanged;
    public event Action<int> ChargesChanged;
    public event Action<bool> DisguiseStateChanged;

    private PlayerController _playerController;
    private SuspicionSystem _suspicionSystem;
    private DisguiseOutfitSwap _outfitSwap;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _suspicionSystem = GetComponent<SuspicionSystem>();
        _outfitSwap = GetComponent<DisguiseOutfitSwap>();
        remainingDisguises = maxDisguises;
    }

    public bool TryStartDisguise()
    {
        if (isDisguising || remainingDisguises <= 0)
        {
            return false;
        }

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return false;
        }

        StartCoroutine(DisguiseRoutine());
        return true;
    }

    public void ResetDisguises()
    {
        remainingDisguises = maxDisguises;
        ChargesChanged?.Invoke(remainingDisguises);
    }

    private IEnumerator DisguiseRoutine()
    {
        isDisguising = true;
        var seenDuringDisguise = false;

        _playerController?.LockMovement(true);
        DisguiseStateChanged?.Invoke(true);
        ProgressChanged?.Invoke(0f);

        var elapsed = 0f;
        while (elapsed < disguiseTime)
        {
            elapsed += Time.deltaTime;
            if (HunterAI.AnyHunterSeeingPlayer())
            {
                seenDuringDisguise = true;
            }

            ProgressChanged?.Invoke(Mathf.Clamp01(elapsed / disguiseTime));
            yield return null;
        }

        var outfitId = _outfitSwap != null ? _outfitSwap.ApplyRandomDisguise() : string.Empty;
        remainingDisguises = Mathf.Max(0, remainingDisguises - 1);

        if (seenDuringDisguise)
        {
            _suspicionSystem?.AddFailedDisguisePenalty();
        }

        HunterAI.NotifyPlayerDisguised(seenDuringDisguise, outfitId);

        _playerController?.LockMovement(false);
        isDisguising = false;

        ProgressChanged?.Invoke(0f);
        ChargesChanged?.Invoke(remainingDisguises);
        DisguiseStateChanged?.Invoke(false);
    }
}
