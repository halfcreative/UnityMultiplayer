using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{

    private void Start()
    {
        KitchenGameMultiplayer.Instance.onTryingToJoinGame += KitchenGameMultiplayer_OnTryingToJoinGame;
        KitchenGameMultiplayer.Instance.onFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        Hide();
    }

    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenGameMultiplayer_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
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
        if (KitchenGameMultiplayer.Instance != null)
        {
            KitchenGameMultiplayer.Instance.onTryingToJoinGame -= KitchenGameMultiplayer_OnTryingToJoinGame;
            KitchenGameMultiplayer.Instance.onFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
        }
    }
}
