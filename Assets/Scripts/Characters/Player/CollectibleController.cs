using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleController : MonoBehaviour
{
    public JackController.CollectibleType type = JackController.CollectibleType.HeavyMachineGun;
    private int collectiblePoints = 1000;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.IsPlayer(collision))
        {
            GameManager.GetPlayer().GetComponent<JackController>().getCollectible(type);
            GameManager.AddScore(collectiblePoints);
            if (type==JackController.CollectibleType.Ammo) // collectible sound
            {
                AudioManager.PlayAmmoGrab();
                AudioManager.PlayOkayVoice();
            }
            else if (type == JackController.CollectibleType.HeavyMachineGun)
            {
                AudioManager.PlayHeavyMachineGunVoice();
            }
            else if (type == JackController.CollectibleType.MedKit)
            {
                AudioManager.PlayMedKitGrab();
            }
            Destroy(gameObject);
        }
    }
}
