using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    private Button mainMenuButton;
    [SerializeField]
    private Button createLobbyButton;
    [SerializeField]
    private Button quickJoinButton;
    [SerializeField]
    private Button codeJoinButton;
    [SerializeField]
    private TMP_InputField lobbyCodeInputField;
    [SerializeField]
    private TMP_InputField playerNameInputField;
    [SerializeField]
    private LobbyCreateUI lobbyCreateUI;
    [SerializeField]
    private Transform lobbyContainer;
    [SerializeField]
    private Transform lobbyTemplate;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });
        quickJoinButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.QuickJoin();
        });
        codeJoinButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);

    }

    private void Start()
    {
        playerNameInputField.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string name) =>
        {
            KitchenGameMultiplayer.Instance.SetPlayerName(name);
        });

        KitchenGameLobby.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, KitchenGameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);

            lobbyTransform.gameObject.SetActive(true);

            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);

        }

    }






}
