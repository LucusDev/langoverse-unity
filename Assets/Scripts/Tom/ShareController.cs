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
        string jsonData = "{\"prompt\":\"talking_about_burger\",\"id\":\"123\",\"importNumber\":\"Uncle Tommy\"}";
        UnityMessageManager.Instance.SendMessageToFlutter(jsonData);
    }
}