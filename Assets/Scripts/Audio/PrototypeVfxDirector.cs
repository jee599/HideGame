using System.Collections.Generic;
using UnityEngine;

public class PrototypeVfxDirector : MonoBehaviour
{
    public Material disguiseSmokeMaterial;
    public Material disguiseSparkMaterial;
    public Material missionCompleteMaterial;
    public Material hunterAlertMaterial;

    [Range(0.5f, 3f)] public float effectScale = 1f;

    private readonly HashSet<HunterAI> _hookedHunters = new HashSet<HunterAI>();

    private PlayerController _player;
    private PlayerDisguise _playerDisguise;
    private MissionManager _missionManager;
    private bool _disguiseSubscribed;
    private bool _missionSubscribed;

    private void Start()
    {
        BindSystems();
    }

    private void Update()
    {
        BindSystems();
    }

    private void OnDestroy()
    {
        if (_disguiseSubscribed && _playerDisguise != null)
        {
            _playerDisguise.DisguiseStateChanged -= HandleDisguiseStateChanged;
        }

        if (_missionSubscribed && _missionManager != null)
        {
            _missionManager.MissionCompleted -= HandleMissionCompleted;
        }

        foreach (var hunter in _hookedHunters)
        {
            if (hunter != null)
            {
                hunter.StateChanged -= HandleHunterStateChanged;
            }
        }

        _hookedHunters.Clear();
    }

    private void BindSystems()
    {
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        if (!_disguiseSubscribed)
        {
            _playerDisguise = FindFirstObjectByType<PlayerDisguise>();
            if (_playerDisguise != null)
            {
                _playerDisguise.DisguiseStateChanged += HandleDisguiseStateChanged;
                _disguiseSubscribed = true;
            }
        }

        if (!_missionSubscribed)
        {
            _missionManager = MissionManager.Instance;
            if (_missionManager != null)
            {
                _missionManager.MissionCompleted += HandleMissionCompleted;
                _missionSubscribed = true;
            }
        }

        var hunters = FindObjectsByType<HunterAI>(FindObjectsSortMode.None);
        for (var i = 0; i < hunters.Length; i++)
        {
            var hunter = hunters[i];
            if (hunter == null || _hookedHunters.Contains(hunter))
            {
                continue;
            }

            hunter.StateChanged += HandleHunterStateChanged;
            _hookedHunters.Add(hunter);
        }
    }

    private void HandleDisguiseStateChanged(bool disguising)
    {
        if (_player == null)
        {
            return;
        }

        var origin = _player.transform.position + Vector3.up * 0.9f;
        if (disguising)
        {
            SpawnBurst(
                "DisguiseSmoke",
                origin,
                disguiseSmokeMaterial,
                new Color(0.78f, 0.90f, 1f, 0.70f),
                18,
                new Vector2(0.35f, 0.55f),
                new Vector2(0.25f, 0.85f),
                new Vector2(0.85f, 1.45f),
                0.35f);
            return;
        }

        SpawnBurst(
            "DisguiseSpark",
            origin + Vector3.up * 0.15f,
            disguiseSparkMaterial,
            new Color(0.26f, 0.95f, 0.72f, 1f),
            24,
            new Vector2(0.16f, 0.28f),
            new Vector2(1.1f, 2.4f),
            new Vector2(0.45f, 0.9f),
            0.12f);
    }

    private void HandleMissionCompleted(MissionData _)
    {
        if (_player == null)
        {
            return;
        }

        SpawnBurst(
            "MissionCelebrate",
            _player.transform.position + Vector3.up * 1.2f,
            missionCompleteMaterial,
            new Color(1f, 0.85f, 0.26f, 1f),
            30,
            new Vector2(0.18f, 0.30f),
            new Vector2(1.6f, 3.1f),
            new Vector2(0.6f, 1.1f),
            0.14f);
    }

    private void HandleHunterStateChanged(HunterAI hunter, HunterState state)
    {
        if (hunter == null || state == HunterState.Patrol)
        {
            return;
        }

        var color = state switch
        {
            HunterState.Investigate => new Color(1f, 0.78f, 0.22f, 1f),
            HunterState.Chase => new Color(1f, 0.38f, 0.18f, 1f),
            HunterState.Lockdown => new Color(1f, 0.12f, 0.12f, 1f),
            _ => Color.white
        };

        var count = state == HunterState.Lockdown ? 22 : 14;
        var speed = state == HunterState.Lockdown ? new Vector2(1.3f, 2.2f) : new Vector2(0.6f, 1.4f);
        var lifetime = state == HunterState.Lockdown ? new Vector2(0.55f, 0.95f) : new Vector2(0.35f, 0.75f);
        SpawnBurst(
            "HunterAlert",
            hunter.transform.position + Vector3.up * 2.15f,
            hunterAlertMaterial,
            color,
            count,
            new Vector2(0.16f, 0.28f),
            speed,
            lifetime,
            0.18f);
    }

    private void SpawnBurst(
        string effectName,
        Vector3 worldPosition,
        Material material,
        Color color,
        int count,
        Vector2 sizeRange,
        Vector2 speedRange,
        Vector2 lifetimeRange,
        float radius)
    {
        var go = new GameObject(effectName, typeof(ParticleSystem));
        go.transform.SetParent(transform, true);
        go.transform.position = worldPosition;

        var particles = go.GetComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = Mathf.Max(count, 8);
        main.duration = Mathf.Max(lifetimeRange.y + 0.25f, 0.6f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedRange.x, speedRange.y);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x * effectScale, sizeRange.y * effectScale);
        main.startColor = color;
        main.gravityModifier = 0.08f;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius * effectScale;

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(Color.Lerp(color, Color.white, 0.25f), 0.4f),
                new GradientColorKey(Color.Lerp(color, Color.clear, 0.35f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(color.a, 0f),
                new GradientAlphaKey(color.a * 0.85f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.45f);
        sizeCurve.AddKey(0.22f, 1f);
        sizeCurve.AddKey(1f, 1.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        particles.Emit(count);
        particles.Play();
        Destroy(go, main.duration + 0.75f);
    }
}
