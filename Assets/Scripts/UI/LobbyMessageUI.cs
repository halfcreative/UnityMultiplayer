using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        KitchenGameMultiplayer.Instance.onFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFailed;
        KitchenGameLobby.Instance.OnJoinStarted += KitchenGameLobby_OnJoinStarted;
        KitchenGameLobby.Instance.OnJoinFailed += KitchenGameLobby_OnJoinFailed;

        Hide();
    }

    private void KitchenGameLobby_OnJoinFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to join lobby");
    }

    private void KitchenGameLobby_OnJoinStarted(object sender, EventArgs e)
    {
        ShowMessage("Joining lobby...");
    }

    private void KitchenGameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to create lobby");
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating lobby...");
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.onFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
    }

}
