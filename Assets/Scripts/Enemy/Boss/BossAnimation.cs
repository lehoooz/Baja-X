using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    private Animator anim;
    private Renderer[] rends;
    private Color[] baseColors;
    private Color telegraphColor = Color.red;

    public void Setup(Color telegraphColor)
    {
        this.telegraphColor = telegraphColor;
    }

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rends = GetComponentsInChildren<Renderer>();
        baseColors = new Color[rends.Length];
        for (int i = 0; i < rends.Length; i++)
            baseColors[i] = rends[i].material.color;
    }

    public void PlayWalk(bool walking) { anim?.SetBool("isWalking", walking); }
    public void PlayTelegraph() { anim?.SetTrigger("Telegraph"); }
    public void PlayCharge() { anim?.SetTrigger("Charge"); }
    public void PlaySlam() { anim?.SetTrigger("Slam"); }
    public void PlayShoot() { anim?.SetTrigger("Shoot"); }
    public void PlayRecover() { anim?.SetTrigger("Recover"); }
    public void PlayDie() { anim?.SetTrigger("Die"); }

    public void SetTelegraphColor() { SetColor(telegraphColor); }

    public void RestoreColor()
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++)
            rends[i].material.color = baseColors[i];
    }

    private void SetColor(Color c)
    {
        if (rends == null) return;
        foreach (Renderer r in rends) r.material.color = c;
    }
}