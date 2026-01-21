using UnityEngine;

// This is where power actually changes things.
public class PowerUpReceiver : MonoBehaviour
{
    public float speedMultiplier = 2f;
    public float powerUpDuration = 5f;

    public void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.SpeedBoost:
                StartCoroutine(SpeedBoost());
                break;

            case PowerUpType.DoubleJump:
                EnableDoubleJump();
                break;

            case PowerUpType.Shield:
                EnableShield();
                break;

            case PowerUpType.DamageBoost:
                EnableDamageBoost();
                break;
        }
    }

    private System.Collections.IEnumerator SpeedBoost()
    {
        var controller = GetComponent<Movement>();
        if (controller == null) yield break;

        controller.moveSpeed *= speedMultiplier;
        yield return new WaitForSeconds(powerUpDuration);
        controller.moveSpeed /= speedMultiplier;
    }

    private void EnableDoubleJump()
    {
        // Flip the switch. You know what this should do.
    }

    private void EnableShield()
    {
        // Temporary invincibility. Nothing gets through.
    }

    private void EnableDamageBoost()
    {
        // More damage. Same rules. Different outcome.
    }
}

