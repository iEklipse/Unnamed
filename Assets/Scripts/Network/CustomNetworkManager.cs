﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    //private Dictionary<NetworkConnection, int> scores = new Dictionary<NetworkConnection, int>();
    //private Dictionary<NetworkConnection, int> characters = new Dictionary<NetworkConnection, int>();
    private Dictionary<NetworkConnection, GameObject> players = new Dictionary<NetworkConnection, GameObject>();
    private Dictionary<NetworkConnection, bool> hostClient = new Dictionary<NetworkConnection, bool>();
    //private int clientScore;
    private int readyPlayers = 0;
    private bool sceneSet = false;

    private void Start()
    {
        Debug.Log("Start()");
        //client.RegisterHandler(ReadyToAddPlayerMsg.id, OnReadyToAddPlayerMsg);
        //NetworkServer.RegisterHandler(QuestionnaireDataStorage.id, OnScoreMsg);
        //clientScore = GameObject.FindGameObjectWithTag("ScoreCarrier").GetComponent<ScoreCarrier>().score;
        NetworkServer.RegisterHandler(CharacterMsg.id, OnCharacterMsg);
        PauseMenuController.isPaused = true;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect()");

        //CharacterMsg msg = new CharacterMsg();
        //msg.characterId = Random.Range(0, 2);
        //msg.character = msg.characterId == 1 ? "Raven" : "Rabbit";
        //Debug.Log("Picked: " + msg.character);
        //singleton.client.Send(CharacterMsg.id, msg);

        client.RegisterHandler(ReadyToAddPlayerMsg.id, OnReadyToAddPlayerMsg);
        ClientScene.Ready(conn);

        CharacterMsg msg = new CharacterMsg();
        msg.isHost = GameObject.FindGameObjectWithTag("ScoreCarrier").GetComponent<ScoreCarrier>().isHost;
        client.Send(CharacterMsg.id, msg);

        //QuestionnaireDataStorage scoreMsg = new QuestionnaireDataStorage();
        //scoreMsg.score = clientScore;
        //client.Send(QuestionnaireDataStorage.id, scoreMsg);

        //ClientScene.AddPlayer(0);
    }

    public override void OnStartHost()
    {
        Debug.Log("OnStartHost()");
    }

    //public void OnScoreMsg(NetworkMessage msg)
    //{
    //    Debug.Log("OnScoreMsg()");
    //    QuestionnaireDataStorage scoreMsg = msg.ReadMessage<QuestionnaireDataStorage>();
    //    if (!scores.ContainsKey(msg.conn))
    //        scores.Add(msg.conn, scoreMsg.score);

    //    if (scores.Count == 2)
    //        PickCharacters();
    //}

    public void OnCharacterMsg(NetworkMessage msg)
    {
        Debug.Log("OnCharacterMsg()");
        CharacterMsg charMsg = msg.ReadMessage<CharacterMsg>();

        if (!hostClient.ContainsKey(msg.conn))
        {
            hostClient.Add(msg.conn, charMsg.isHost);
            ++readyPlayers;

            if (readyPlayers == 2)
                PreparePrefabs();
        }
    }

    public void OnReadyToAddPlayerMsg(NetworkMessage msg)
    {
        Debug.Log("OnReadyToAddPlayerMsg()");
        ReadyToAddPlayerMsg readyMsg = msg.ReadMessage<ReadyToAddPlayerMsg>();
        ClientScene.AddPlayer(0);
        GameObject.FindGameObjectWithTag("WaitingForPlayer").SetActive(false);
    }

    public void PickCharacters()
    {
        Debug.Log("PickCharacters()");
        //List<NetworkConnection> conns = new List<NetworkConnection>(scores.Keys);
        //NetworkConnection maxConnection = conns[0];
        //NetworkConnection minConnection = conns[0];
        //int maxScore = int.MinValue;
        //int minScore = int.MaxValue;
        //bool tied = false;

        //foreach (KeyValuePair<NetworkConnection, int> entry in scores)
        //{
        //    if (entry.Value > maxScore)
        //    {
        //        maxConnection = entry.Key;
        //        maxScore = entry.Value;
        //    }
        //    else if (entry.Value < minScore)
        //    {
        //        minConnection = entry.Key;
        //        minScore = entry.Value;
        //    }
        //    else if (entry.Value == maxScore || entry.Value == minScore)
        //        tied = true;
        //}

        //if (tied)
        //{
        //    Debug.Log("Tied");
        //    int random0 = Random.Range(0, 2);
        //    int random1 = (random0 == 0) ? 1 : 0;

        //    characters[conns[0]] = random0;
        //    characters[conns[1]] = random1;
        //}
        //else
        //{
        //    Debug.Log("Not tied");
        //    characters[maxConnection] = 0;
        //    characters[minConnection] = 1;
        //}

        //PreparePrefabs();
    }

    public void PreparePrefabs()
    {
        Debug.Log("PreparePrefabs()");
        readyPlayers = 0;
        //ServerChangeScene("TheGame");
        foreach (KeyValuePair<NetworkConnection, bool> entry in hostClient)
        {
            int spawnIndex = entry.Value ? 0 : 1;
            GameObject prefab = spawnPrefabs[spawnIndex];
            players.Add(entry.Key, prefab);

            ReadyToAddPlayerMsg msg = new ReadyToAddPlayerMsg();
            NetworkServer.SendToClient(entry.Key.connectionId, ReadyToAddPlayerMsg.id, msg);
            Debug.Log("Player ready for client");

            //NetworkServer.AddPlayerForConnection(entry.Key, player, 0);
            //Debug.Log("Added player for connection");
        }
        //ServerChangeScene("TheGame");
    }

    //public override void OnServerReady(NetworkConnection conn)
    //{
    //    //base.OnServerReady(conn);
    //    ++readyPlayers;

    //    if (readyPlayers == 2)
    //        PickCharacters();
    //}

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("OnServerAddPlayer()");
        if (players.ContainsKey(conn))
        {
            //GameObject prefab = spawnPrefabs[characters[conn]];
            GameObject player = Instantiate(players[conn], startPositions[conn.connectionId].position, startPositions[conn.connectionId].rotation);
            DontDestroyOnLoad(player);
            NetworkServer.AddPlayerForConnection(conn, player, 0);
            Debug.Log("Added player for connection: " + conn);
        }
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        if (SceneManager.GetActiveScene().name == "Main Menu" && PlayerPrefs.GetInt("GameFinished") == 1)
        {
            GameObject.FindGameObjectWithTag("Menu").SetActive(false);
            GameObject.FindGameObjectWithTag("MenuEndGame").SetActive(true);
        }

    }
}