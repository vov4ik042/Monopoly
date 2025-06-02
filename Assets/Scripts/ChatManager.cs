using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private ScrollRect ScrollView;
    [SerializeField] private Transform Container;
    [SerializeField] private Transform Template;
    [SerializeField] private TMP_InputField InputField;

    public static ChatManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Template.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendMessageClient(InputField.text);
            InputField.text = "";
        }
    }

    private void SendMessageClient(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        int clientId = (int)NetworkManager.Singleton.LocalClientId;
        string playerName = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId((int)clientId).ToString();
        string result = playerName + ": " + text;
        SendMessageServerRpc(result, clientId);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRpc(string text, int clientId)
    {
        Debug.Log(text);
        SendMessageClientRpc(text, clientId);
    }
    [ClientRpc]
    private void SendMessageClientRpc(string text, int clientId)
    {
        AddMessage(text, clientId);
    }
    private void AddMessage(string text, int clientId)
    {
        Color color;
        if (clientId != -1)
        {
            color = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(clientId);

        }
        else
        {
            color = Color.white;
        }
        Transform chatManager = Instantiate(Template, Container);
        chatManager.GetComponent<ChatMessage>().SetTextAndColor(text, color);
        chatManager.gameObject.SetActive(true);
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // подождать один кадр
        ScrollView.verticalNormalizedPosition = 0f;
    }
}
