using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

class PlayerScorePair
{
	public PlayerController player;
	public int score;

	public PlayerScorePair(PlayerController player, int score)
	{
		this.player = player;
		this.score = score;
	}
}

public class Leaderboard : NetworkBehaviour
{
	readonly Dictionary<ulong, PlayerScorePair> playersAndScores = new();
	private Text textDisplay;

	public override void OnNetworkSpawn()
	{
		textDisplay = GetComponent<Text>();
		if (IsServer)
		{
			NetworkManager.OnClientConnectedCallback += NewPlayer;
			NewPlayer(0);
		}
	}

	private void NewPlayer(ulong id)
	{
		if (!IsServer) return;
		Debug.Log("---------------------------------------------------------------playerJoined");
		var playerObject = NetworkManager.ConnectedClients[id].PlayerObject;
		playersAndScores.Add(id, 
			new PlayerScorePair(
				playerObject.GetComponent<PlayerController>(), 
				0));
		playerObject.GetComponent<Health>().OnHealthZeroServer += ScoreIncrament;

		UpdateScores();
	}

	void ScoreIncrament(ulong id)
	{
		if (!IsServer) return;
		Debug.Log("---------------------------------------------------------------player got point");
		playersAndScores[id].score += 1;
		UpdateScores();
	}

	void UpdateScores()
	{
		if (!IsServer) return;
		var x = playersAndScores.Select(kv=>(kv.Key,kv.Value.score)).OrderBy(t=>t.score).Reverse().ToList();
		string newText = "";
		foreach (var name_score in x)
		{
			newText += name_score.Key + " " + name_score.Item2 +'\n';
		}
		UpdateScores_ClientRPC(newText);
	}

	[ClientRpc]
	void UpdateScores_ClientRPC(string text)
	{
		textDisplay.text = text;
	}
}
