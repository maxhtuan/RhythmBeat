using UnityEngine;

using DG.Tweening;
public class TargetBarController : MonoBehaviour
{
    [SerializeField] DOTweenAnimation onHitEffect, onMissEffect;

    public void PlayOnHitEffect()
    {
        Debug.Log("PlayOnHitEffect");
        CancelAll();
        onHitEffect?.DORestartAllById("Holding");
        // onHitEffect?.DORestartAllById("Shake");
        // transform.DOPunchPosition(new Vector3(0.01f, 0, 0), 0.2f, 36, 0.13f).SetLoops(-1, LoopType.Restart);
    }

    public void OnHolding()
    {
        Debug.Log("OnHolding");
        CancelAll();
        onHitEffect?.DORestartAllById("Holding");
        // this.transform.DOLocalMoveX(-0.4934f, 0.15f);
    }

    public void OnReleaseHitEffect()
    {
        CancelAll();
        onHitEffect?.DORestartAllById("Release");
    }

    public void PlayOnMissEffect()
    {
        CancelAll();
        onMissEffect?.DOPlay();
    }

    public void CancelAll()
    {
        this.transform.DOKill();
        onHitEffect?.DOPauseAllById("Shake");
        onHitEffect?.DOPauseAllById("Holding");
        onHitEffect?.DOPauseAllById("Release");

        onMissEffect?.DOComplete();

        this.transform.localPosition = new Vector3(-0.5f, -0.09f, 0);
    }
}
