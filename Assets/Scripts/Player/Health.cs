using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public enum HealthChangeReason
{
    death,
}



public class Health : NetworkBehaviour
{
    public static int starterHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private PlayerController myPlayer;

    public Action<ulong> OnHealthZeroServer;
    public UnityEvent OnHealthZeroClient;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        currentHealth.Value = starterHealth;
        myPlayer = GetComponentInParent<PlayerController>();
    }

    [ServerRpc]
    public void RequestHealthChange_ServerRPC(HealthChangeReason Reason, int amount = 0)
    {
        switch (Reason)
        {
            case HealthChangeReason.death:

                //validate?
                currentHealth.Value = starterHealth;
                break;

        }
    }

    public void TakeDamage(int damage,PlayerController source)
    {
        if (!IsServer) return;
        damage = damage < 0 ? damage : -damage;
        currentHealth.Value += damage;

        if (currentHealth.Value <= 0)
        {
            OnHealthZeroServer.Invoke(source.OwnerClientId);
            helthZero_ClientRPC();
            currentHealth.Value = starterHealth;

        }
    }

    [ClientRpc]
    private void helthZero_ClientRPC()
    {
        OnHealthZeroClient.Invoke();
    }
}
