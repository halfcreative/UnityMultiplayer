using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseMessageUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        KitchenGameMultiplayer.Instance.onFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        Hide();
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
        Show();
        messageText.text = NetworkManager.Singleton.DisconnectReason;
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
