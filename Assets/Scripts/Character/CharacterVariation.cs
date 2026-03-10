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
