using UnityEngine;
using FlutterUnityIntegration;

public class ShareController : MonoBehaviour
{
    public void ShowShareDialog()
    {
        // Send message to Flutter
        UnityMessageManager.Instance.SendMessageToFlutter("show_share_dialog");
    }

    public void ShowConversation()
    {
        UnityMessageManager.Instance.SendMessageToFlutter("call_uncle_tom__prompt_talking_about_burger");
    }
}