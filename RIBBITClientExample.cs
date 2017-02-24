using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( RIBBITClient ) )]
public class RIBBITClientExample : MonoBehaviour {
    public Text tipText;

    private RIBBITClient client;

    void OnEnable() {
        client = this.GetComponent<RIBBITClient>();

        var ui = new GameObject( "TIPUI" );
        ui.AddComponent<RectTransform>();
        ui.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        ui.AddComponent<CanvasScaler>();
        ui.AddComponent<GraphicRaycaster>();
        ui.AddComponent<CanvasGroup>();

        var txt = new GameObject( "Text" );

        txt.transform.SetParent( ui.transform );
        var trt = txt.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        trt.offsetMax = new Vector2( Screen.width, Screen.height );
        trt.offsetMin = Vector2.zero;

        txt.AddComponent<CanvasRenderer>();

        tipText = txt.AddComponent<Text>();
        tipText.font = Resources.GetBuiltinResource<Font>( "Arial.ttf" );
        tipText.alignment = TextAnchor.MiddleCenter;
        tipText.resizeTextForBestFit = true;
        tipText.resizeTextMinSize = 10;
        tipText.resizeTextMaxSize = 40;
        tipText.color = Color.black;
    }

    public void GetTip() {
        tipText.text = client.FrogTip();
    }

    void Update() {
        if( Input.GetMouseButtonDown( 0 ) ) {
            GetTip();
        }
    }
}