using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerDashVisual : MonoBehaviour
{
    [Header("Trail")]
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private float trailWidth = 1.6f;

    [Header("Tint")]
    [SerializeField] private Color dashTint = new Color(1.5f, 2.5f, 3f, 1f);
    [SerializeField] private float tintSmooth = 20f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP

    private PlayerMovement movement;
    private Renderer[] renderers;
    private MaterialPropertyBlock block;
    private Color currentTint = Color.white;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();

        if (dashTrail == null) dashTrail = GetComponentInChildren<TrailRenderer>();
        if (dashTrail != null)
        {
            dashTrail.widthMultiplier = trailWidth;
            dashTrail.emitting = false;
        }
    }

    private void Update()
    {
        if (dashTrail != null)
            dashTrail.emitting = movement.isDashing;

        Color target = movement.isDashing ? dashTint : Color.white;
        currentTint = Color.Lerp(currentTint, target, tintSmooth * Time.deltaTime);

        foreach (var r in renderers)
        {
            if (r is TrailRenderer) continue;
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, currentTint);
            r.SetPropertyBlock(block);
        }
    }
}