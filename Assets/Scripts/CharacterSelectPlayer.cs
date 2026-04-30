using System;
using UnityEngine;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyText;
    [SerializeField] private PlayerVisual playerVisual;

    private void Start()
    {
        CharacterSelectReady.Instance.onPlayerReadyChanged += CharacterSelectReady_OnPlayerReadyChanged;
        KitchenGameMultiplayer.Instance.onPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        readyText.SetActive(false);

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnPlayerReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (KitchenGameMultiplayer.Instance.isPlayerIndexConnected(playerIndex))
        {
            Show();
            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyText.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));
            playerVisual.SetColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
        }
        else
        {
            Hide();
        }

    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
