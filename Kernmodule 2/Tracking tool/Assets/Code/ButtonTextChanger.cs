using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextChanger : MonoBehaviour {

    private Text text;
    public RealToIRCamera rtirCam;

    public string encodingText;
    public string notEncodingText;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void FixedUpdate()
    {
        UpdateText();
    }

    private void UpdateText() ////updates the button's text to display the correct string based on whether RealToIRCamera is encoding or not.
    {
        if (rtirCam.encodeToTextAsset && text.text != encodingText)
        {
            text.text = encodingText;
        }
        else if (!rtirCam.encodeToTextAsset && text.text != notEncodingText)
        {
            text.text = notEncodingText;
        }
    }

}
