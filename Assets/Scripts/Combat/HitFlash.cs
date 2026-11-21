using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Flash putih dengan tukar material. Dipakai kalau flash
/// lewat _BaseColor tidak kelihatan (warna dari tekstur). </summary>
public class HitFlash : MonoBehaviour
{
    [SerializeField] private Material flashMaterial; // drag FlashWhite ke sini
    [SerializeField] private float flashDuration = 0.1f;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Coroutine flashRoutine;

    private void Awake()
    {
        var list = new List<Renderer>();
        list.AddRange(GetComponentsInChildren<MeshRenderer>());
        list.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
        renderers = list.ToArray();

        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
            originalMaterials[i] = renderers[i].sharedMaterials;

        Health health = GetComponent<Health>();
        if (health != null)
            health.OnDamaged.AddListener(OnDamaged);
    }

    private void OnDestroy()
    {
        Health health = GetComponent<Health>();
        if (health != null)
            health.OnDamaged.RemoveListener(OnDamaged);
    }

    private void OnDamaged(int damage)
    {
        if (flashMaterial == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetMaterials(true);
        yield return new WaitForSeconds(flashDuration);
        SetMaterials(false);
    }

    private void SetMaterials(bool flash)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (flash)
            {
                var mats = new Material[originalMaterials[i].Length];
                for (int j = 0; j < mats.Length; j++)
                    mats[j] = flashMaterial;
                renderers[i].sharedMaterials = mats;
            }
            else
            {
                renderers[i].sharedMaterials = originalMaterials[i];
            }
        }
    }
}