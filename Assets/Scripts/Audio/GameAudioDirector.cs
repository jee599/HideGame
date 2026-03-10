using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAudioDirector : MonoBehaviour
{
    public AudioClip uiClickClip;
    public AudioClip missionCompleteClip;
    public AudioClip hunterAlertClip;
    public AudioClip lockdownClip;
    public AudioClip gameOverClip;
    public AudioClip disguiseClip;
    public AudioClip footstepConcreteClip;
    public AudioClip footstepGrassClip;
    public AudioClip[] menuMusicClips;
    public AudioClip[] gameplayMusicClips;
    public AudioClip[] resultMusicClips;

    [Range(0f, 1f)] public float uiVolume = 0.7f;
    [Range(0f, 1f)] public float worldVolume = 0.85f;
    [Range(0f, 1f)] public float footstepVolume = 0.45f;
    [Range(0f, 1f)] public float musicVolume = 0.28f;
    public Vector2 musicGapRange = new Vector2(0.35f, 1.15f);
    public float walkStepInterval = 0.48f;
    public float runStepInterval = 0.30f;

    private readonly List<Button> _hookedButtons = new List<Button>();

    private AudioSource _uiSource;
    private AudioSource _worldSource;
    private AudioSource _footstepSource;
    private AudioSource _musicSource;
    private MissionManager _missionManager;
    private GameManager _gameManager;
    private PlayerDisguise _playerDisguise;
    private PlayerController _player;
    private HunterAI _hunter;
    private HunterState _lastHunterState;
    private bool _hasHunterState;
    private float _footstepTimer;
    private float _musicDelayTimer;
    private int _lastMusicIndex = -1;
    private string _activeSceneName;
    private AudioClip[] _currentPlaylist;
    private bool _missionSubscribed;
    private bool _gameSubscribed;
    private bool _disguiseSubscribed;

    private void Awake()
    {
        _uiSource = CreateSource("UI Audio");
        _worldSource = CreateSource("World Audio");
        _footstepSource = CreateSource("Footsteps");
        _musicSource = CreateSource("Music");
        _footstepSource.spatialBlend = 0f;
        _musicSource.loop = false;
        _musicSource.ignoreListenerPause = true;
        _musicSource.volume = musicVolume;
    }

    private void Start()
    {
        BindSceneSystems();
        HookButtons();
        RefreshMusicPlaylist(true);
    }

    private void Update()
    {
        BindSceneSystems();
        HookButtons();
        UpdateHunterStateAudio();
        UpdateFootsteps();
        UpdateMusic();
    }

    private void OnDestroy()
    {
        if (_missionSubscribed && _missionManager != null)
        {
            _missionManager.MissionCompleted -= HandleMissionCompleted;
        }

        if (_gameSubscribed && _gameManager != null)
        {
            _gameManager.StateChanged -= HandleGameStateChanged;
        }

        if (_disguiseSubscribed && _playerDisguise != null)
        {
            _playerDisguise.DisguiseStateChanged -= HandleDisguiseStateChanged;
        }

        for (var i = 0; i < _hookedButtons.Count; i++)
        {
            if (_hookedButtons[i] != null)
            {
                _hookedButtons[i].onClick.RemoveListener(HandleButtonClicked);
            }
        }
    }

    private AudioSource CreateSource(string name)
    {
        var child = new GameObject(name, typeof(AudioSource));
        child.transform.SetParent(transform, false);
        var source = child.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        return source;
    }

    private void BindSceneSystems()
    {
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
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

        if (!_gameSubscribed)
        {
            _gameManager = GameManager.Instance;
            if (_gameManager != null)
            {
                _gameManager.StateChanged += HandleGameStateChanged;
                _gameSubscribed = true;
            }
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

        if (_hunter == null)
        {
            _hunter = HunterAI.GetPrimaryHunter();
        }
    }

    private void HookButtons()
    {
        var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button == null || _hookedButtons.Contains(button))
            {
                continue;
            }

            button.onClick.AddListener(HandleButtonClicked);
            _hookedButtons.Add(button);
        }
    }

    private void HandleButtonClicked()
    {
        PlayUi(uiClickClip);
    }

    private void HandleMissionCompleted(MissionData _)
    {
        PlayWorld(missionCompleteClip);
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            PlayWorld(gameOverClip);
        }
    }

    private void HandleDisguiseStateChanged(bool disguising)
    {
        if (disguising)
        {
            PlayWorld(disguiseClip);
        }
    }

    private void UpdateHunterStateAudio()
    {
        if (_hunter == null)
        {
            _hunter = HunterAI.GetPrimaryHunter();
            if (_hunter == null)
            {
                return;
            }
        }

        if (!_hasHunterState)
        {
            _lastHunterState = _hunter.currentState;
            _hasHunterState = true;
            return;
        }

        if (_lastHunterState == _hunter.currentState)
        {
            return;
        }

        if (_hunter.currentState == HunterState.Investigate || _hunter.currentState == HunterState.Chase)
        {
            PlayWorld(hunterAlertClip);
        }
        else if (_hunter.currentState == HunterState.Lockdown)
        {
            PlayWorld(lockdownClip);
        }

        _lastHunterState = _hunter.currentState;
    }

    private void UpdateFootsteps()
    {
        if (_player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (!_player.IsMoving)
        {
            _footstepTimer = 0f;
            return;
        }

        _footstepTimer += Time.deltaTime;
        var interval = _player.IsRunning ? runStepInterval : walkStepInterval;
        if (_footstepTimer < interval)
        {
            return;
        }

        _footstepTimer = 0f;
        var clip = ShouldUseGrassFootstep(_player.CurrentZoneTag) ? footstepGrassClip : footstepConcreteClip;
        PlayFootstep(clip);
    }

    private void UpdateMusic()
    {
        RefreshMusicPlaylist(false);

        if (_musicSource == null || _currentPlaylist == null || _currentPlaylist.Length == 0)
        {
            return;
        }

        _musicSource.volume = musicVolume;
        if (_musicSource.isPlaying)
        {
            return;
        }

        if (_musicDelayTimer > 0f)
        {
            _musicDelayTimer = Mathf.Max(0f, _musicDelayTimer - Time.unscaledDeltaTime);
            return;
        }

        var clip = PickNextMusicClip(_currentPlaylist);
        if (clip == null)
        {
            return;
        }

        _musicSource.clip = clip;
        _musicSource.Play();
        _musicDelayTimer = Random.Range(
            Mathf.Max(0f, musicGapRange.x),
            Mathf.Max(Mathf.Max(0f, musicGapRange.x), musicGapRange.y));
    }

    private void RefreshMusicPlaylist(bool force)
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (!force && string.Equals(_activeSceneName, sceneName))
        {
            return;
        }

        _activeSceneName = sceneName;
        _currentPlaylist = GetPlaylistForScene(sceneName);
        _lastMusicIndex = -1;
        _musicDelayTimer = 0f;
        if (_musicSource != null)
        {
            _musicSource.Stop();
            _musicSource.clip = null;
        }
    }

    private AudioClip[] GetPlaylistForScene(string sceneName)
    {
        if (string.Equals(sceneName, SceneFlow.MainMenuSceneName))
        {
            return menuMusicClips;
        }

        if (string.Equals(sceneName, SceneFlow.ResultSceneName))
        {
            return resultMusicClips;
        }

        return gameplayMusicClips;
    }

    private AudioClip PickNextMusicClip(IReadOnlyList<AudioClip> playlist)
    {
        if (playlist == null || playlist.Count == 0)
        {
            return null;
        }

        if (playlist.Count == 1)
        {
            _lastMusicIndex = 0;
            return playlist[0];
        }

        var nextIndex = Random.Range(0, playlist.Count);
        if (nextIndex == _lastMusicIndex)
        {
            nextIndex = (nextIndex + 1) % playlist.Count;
        }

        _lastMusicIndex = nextIndex;
        return playlist[nextIndex];
    }

    private static bool ShouldUseGrassFootstep(string zoneTag)
    {
        if (string.IsNullOrEmpty(zoneTag))
        {
            return false;
        }

        return zoneTag == "Park" || zoneTag == "Bench" || zoneTag == "Plaza";
    }

    private void PlayUi(AudioClip clip)
    {
        if (clip == null || _uiSource == null)
        {
            return;
        }

        _uiSource.PlayOneShot(clip, uiVolume);
    }

    private void PlayWorld(AudioClip clip)
    {
        if (clip == null || _worldSource == null)
        {
            return;
        }

        _worldSource.PlayOneShot(clip, worldVolume);
    }

    private void PlayFootstep(AudioClip clip)
    {
        if (clip == null || _footstepSource == null)
        {
            return;
        }

        _footstepSource.PlayOneShot(clip, footstepVolume);
    }
}

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
