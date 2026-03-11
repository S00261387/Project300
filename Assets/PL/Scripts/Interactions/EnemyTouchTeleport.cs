using UnityEngine;
using System.Collections;

public class EnemyTouchTeleport : MonoBehaviour
{
    public float hitCooldown = 1f;

    private bool canHit = true;

    void OnTriggerEnter(Collider other)
    {
        TryHitPlayer(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryHitPlayer(other);
    }

    void TryHitPlayer(Collider other)
    {
        if (!canHit)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (DungeonBuilder.Instance == null)
            return;

        canHit = false;
        DungeonBuilder.Instance.TeleportPlayerToCurrentSpawn();
        StartCoroutine(HitCooldown());
    }

    IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        canHit = true;
    }
}