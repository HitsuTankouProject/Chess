using UnityEngine;
using UnityEngine.VFX;

public class ChessBreak : MonoBehaviour
{
    [SerializeField] VisualEffect vfx;
    void Start()
    {
        Debug.Log("Asset = " + vfx.visualEffectAsset);
        Debug.Log("Enabled = " + vfx.enabled);
        Debug.Log("Pause = " + vfx.pause);

        vfx.Reinit();
        vfx.Play();

        Debug.Log("Awake = " + vfx.HasAnySystemAwake());
        Debug.Log("Alive = " + vfx.aliveParticleCount);
    }
}