using Unity.Netcode;
using UnityEngine;

public class StandardMine : NetworkBehaviour
{
    
   [SerializeField] GameObject minePrefab;
   [SerializeField] PlayerController source;

   void OnTriggerEnter2D(Collider2D other)
   {

    
    if(IsServer){

        Health health = other.GetComponent<Health>();
        if(!health) return;
        health.TakeDamage(25, source);
        
        int xPosition = Random.Range(-4, 4);
        int yPosition = Random.Range(-2, 2);


        GameObject newMine = Instantiate(minePrefab, new Vector3(xPosition, yPosition, 0), Quaternion.identity);
        NetworkObject no = newMine.GetComponent<NetworkObject>();
        no.Spawn();
        
        

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();



    }


   }




}
