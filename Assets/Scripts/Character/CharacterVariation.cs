using UnityEngine;

public class CharacterVariation : MonoBehaviour
{
    [Header("Modular Parts")]
    public GameObject[] headVariants;
    public GameObject[] hairVariants;
    public GameObject[] bodyVariants;
    public GameObject[] legVariants;
    public GameObject[] accessoryVariants;

    [Header("Shared Tint")]
    public Renderer[] tintRenderers;
    public Color[] palette =
    {
        new Color(0.88f, 0.32f, 0.28f),
        new Color(0.18f, 0.50f, 0.83f),
        new Color(0.24f, 0.67f, 0.42f),
        new Color(0.90f, 0.72f, 0.21f),
        new Color(0.64f, 0.35f, 0.78f),
        new Color(0.85f, 0.48f, 0.20f),
        new Color(0.34f, 0.72f, 0.77f),
        new Color(0.65f, 0.25f, 0.28f),
        new Color(0.56f, 0.58f, 0.20f),
        new Color(0.30f, 0.30f, 0.34f)
    };

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock _propertyBlock;

    public void Randomize()
    {
        ApplySelection(headVariants, RandomIndex(headVariants));
        ApplySelection(hairVariants, RandomIndex(hairVariants));
        ApplySelection(bodyVariants, RandomIndex(bodyVariants));
        ApplySelection(legVariants, RandomIndex(legVariants));
        ApplySelection(accessoryVariants, RandomIndex(accessoryVariants));
        ApplyRandomColor();
    }

    public void ApplyOutfit(OutfitData outfit)
    {
        if (outfit == null)
        {
            Randomize();
            return;
        }

        ApplySelection(headVariants, outfit.headIndex, false);
        ApplySelection(hairVariants, outfit.hairIndex, false);
        ApplySelection(bodyVariants, outfit.bodyIndex, false);
        ApplySelection(legVariants, outfit.legIndex, false);
        ApplySelection(accessoryVariants, outfit.accessoryIndex, false);

        if (outfit.useExplicitColor)
        {
            ApplyColor(outfit.tintColor);
            return;
        }

        if (outfit.paletteOverride != null && outfit.paletteOverride.Length > 0)
        {
            ApplyColor(outfit.paletteOverride[Random.Range(0, outfit.paletteOverride.Length)]);
            return;
        }

        ApplyRandomColor();
    }

    public void ApplyRandomColor()
    {
        if (palette == null || palette.Length == 0)
        {
            return;
        }

        ApplyColor(palette[Random.Range(0, palette.Length)]);
    }

    private void ApplyColor(Color color)
    {
        if (tintRenderers == null || tintRenderers.Length == 0)
        {
            return;
        }

        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        for (var i = 0; i < tintRenderers.Length; i++)
        {
            var rendererTarget = tintRenderers[i];
            if (rendererTarget == null)
            {
                continue;
            }

            rendererTarget.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _propertyBlock.SetColor(ColorId, color);
            rendererTarget.SetPropertyBlock(_propertyBlock);
        }
    }

    private void ApplySelection(GameObject[] variants, int index, bool randomWhenInvalid = true)
    {
        if (variants == null || variants.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= variants.Length)
        {
            if (!randomWhenInvalid)
            {
                return;
            }

            index = RandomIndex(variants);
        }

        for (var i = 0; i < variants.Length; i++)
        {
            if (variants[i] != null)
            {
                variants[i].SetActive(i == index);
            }
        }
    }

    private static int RandomIndex(GameObject[] variants)
    {
        return variants == null || variants.Length == 0 ? -1 : Random.Range(0, variants.Length);
    }
}

public static class CharacterAnimatorDriver
{
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int HorHash = Animator.StringToHash("Hor");
    private static readonly int VertHash = Animator.StringToHash("Vert");
    private static readonly int IsJumpHash = Animator.StringToHash("IsJump");

    private static readonly System.Collections.Generic.Dictionary<int, AnimatorParameterProfile> ParameterProfiles =
        new System.Collections.Generic.Dictionary<int, AnimatorParameterProfile>();

    private struct AnimatorParameterProfile
    {
        public bool HasStateInt;
        public bool HasStateFloat;
        public bool HasSpeedFloat;
        public bool HasHorFloat;
        public bool HasVertFloat;
        public bool HasJumpBool;
    }

    public static void ApplyLocomotion(
        Animator animator,
        Transform referenceTransform,
        Vector3 worldVelocity,
        float referenceSpeed,
        CitizenAnimationState requestedState,
        bool isRunning,
        bool isAirborne = false)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        var profile = GetProfile(animator);
        var speedMagnitude = worldVelocity.magnitude;
        var normalizedSpeed = referenceSpeed > 0.01f ? Mathf.Clamp01(speedMagnitude / referenceSpeed) : 0f;
        var localVelocity = referenceTransform != null
            ? referenceTransform.InverseTransformDirection(worldVelocity)
            : worldVelocity;

        var horizontal = referenceSpeed > 0.01f
            ? Mathf.Clamp(localVelocity.x / referenceSpeed, -1f, 1f)
            : 0f;
        var forward = referenceSpeed > 0.01f
            ? Mathf.Clamp(localVelocity.z / referenceSpeed, -1f, 1f)
            : 0f;

        if (profile.HasSpeedFloat)
        {
            animator.SetFloat(SpeedHash, speedMagnitude);
        }

        if (profile.HasStateInt)
        {
            animator.SetInteger(StateHash, (int)requestedState);
        }

        if (profile.HasStateFloat)
        {
            animator.SetFloat(StateHash, isRunning || requestedState == CitizenAnimationState.Run ? 1f : 0f);
        }

        if (profile.HasHorFloat)
        {
            animator.SetFloat(HorHash, horizontal);
        }

        if (profile.HasVertFloat)
        {
            animator.SetFloat(VertHash, forward == 0f && normalizedSpeed > 0f ? normalizedSpeed : forward);
        }

        if (profile.HasJumpBool)
        {
            animator.SetBool(IsJumpHash, isAirborne);
        }
    }

    private static AnimatorParameterProfile GetProfile(Animator animator)
    {
        var controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            return default;
        }

        var key = controller.GetInstanceID();
        if (ParameterProfiles.TryGetValue(key, out var profile))
        {
            return profile;
        }

        profile = default;
        var parameters = animator.parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.nameHash == StateHash)
            {
                profile.HasStateInt |= parameter.type == AnimatorControllerParameterType.Int;
                profile.HasStateFloat |= parameter.type == AnimatorControllerParameterType.Float;
                continue;
            }

            if (parameter.nameHash == SpeedHash)
            {
                profile.HasSpeedFloat |= parameter.type == AnimatorControllerParameterType.Float;
                continue;
            }

            if (parameter.nameHash == HorHash)
            {
                profile.HasHorFloat |= parameter.type == AnimatorControllerParameterType.Float;
                continue;
            }

            if (parameter.nameHash == VertHash)
            {
                profile.HasVertFloat |= parameter.type == AnimatorControllerParameterType.Float;
                continue;
            }

            if (parameter.nameHash == IsJumpHash)
            {
                profile.HasJumpBool |= parameter.type == AnimatorControllerParameterType.Bool;
            }
        }

        ParameterProfiles[key] = profile;
        return profile;
    }
}
