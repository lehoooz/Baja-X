using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    private Animator anim;
    private int chargeLayerIndex = -1;
    private float chargeLayerTargetWeight = 0f;
    [SerializeField] private float chargeBlendSpeed = 5f;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim != null)
            chargeLayerIndex = anim.GetLayerIndex("Charge");
    }

    private void Update()
    {
        if (anim == null || chargeLayerIndex < 0) return;
        float current = anim.GetLayerWeight(chargeLayerIndex);
        float next = Mathf.MoveTowards(current, chargeLayerTargetWeight, chargeBlendSpeed * Time.deltaTime);
        anim.SetLayerWeight(chargeLayerIndex, next);
    }

    public void PlayWalk(bool walking) { anim?.SetBool("isWalking", walking); }
    public void PlayTelegraph() { anim?.SetTrigger("Telegraph"); }
    public void PlayCharge()
    {
        anim?.SetBool("isWalking", true);
        chargeLayerTargetWeight = 1f;
    }
    public void StopChargeLayer()
    {
        chargeLayerTargetWeight = 0f;
    }
    public void PlaySlam() { anim?.SetTrigger("Slam"); }
    public void PlayShoot() { anim?.SetTrigger("Shoot"); }
    public void PlayRecover() { anim?.SetTrigger("Recover"); }
    public void PlayDie() { anim?.SetTrigger("Die"); }
}