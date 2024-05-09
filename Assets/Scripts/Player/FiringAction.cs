using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FiringAction : NetworkBehaviour
{
	[SerializeField] PlayerController playerController;
	[SerializeField] GameObject clientSingleBulletPrefab;
	[SerializeField] GameObject serverSingleBulletPrefab;
	[SerializeField] Transform bulletSpawnPoint;
	[SerializeField] float MinTimeBeweenShots;
	float lastShot = 0;

	public override void OnNetworkSpawn()
	{
		playerController.onFireEvent += Fire;
	}

	private void Fire(bool isShooting)
	{

		if (isShooting)
		{
			ShootLocalBullet();
		}
	}

	[ServerRpc]
	private void ShootBulletServerRpc()
	{
		//server side check
		if (Time.time < lastShot + MinTimeBeweenShots) return;
		lastShot = Time.time;
		GameObject bullet = Instantiate(serverSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
		bullet.GetComponent<SingleBulletDamage>().sourcse=playerController;
		Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
		ShootBulletClientRpc();
	}

	[ClientRpc]
	private void ShootBulletClientRpc()
	{

		if (IsOwner) return;
		GameObject bullet = Instantiate(clientSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
		Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());

	}

	private void ShootLocalBullet()
	{
		//client side check
		if (Time.time < lastShot + MinTimeBeweenShots) return;
		if (!IsHost)
			lastShot = Time.time; //if we are host, lastShot updated on server
		GameObject bullet = Instantiate(clientSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
		Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());

		ShootBulletServerRpc();
   
	}
}
