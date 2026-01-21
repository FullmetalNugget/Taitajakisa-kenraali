using UnityEngine;

// You walk into it.
// It disappears.
// Something changes.
// That’s the deal.

public class PowerUpPickup : MonoBehaviour
{
    // The choice. Controlled. Predictable.
    public PowerUpType powerUpType;

    // One touch is enough.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var receiver = other.GetComponent<PowerUpReceiver>();
        if (receiver == null) return;

        receiver.ApplyPowerUp(powerUpType);

        Destroy(gameObject);
    }
}

// The dropdown.
// Clean. No surprises.
public enum PowerUpType
{
    SpeedBoost,
    DoubleJump,
    Shield,
    DamageBoost
}

