using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RealToIRCamera : MonoBehaviour {

    
    private WebCamDevice cam; ////The first camera in the WebCamTexture.devices array.
    private WebCamTexture camTexture; ////The texture of cam's video feed
    private RenderTexture renderTex; ////The RenderTexture that first contains the converted WebCamTexture, and then gets assigned the processed image.
    private Material renderTexMat; ////The material that camTexture gets assigned to during the conversion from WebCamTexture to RenderTexture.
    public ComputeShader decodingShader; ////The ComputeShader that gets used to process the converted video feed into an IR-decodable image.
    public Transform decodedFeedTransform; ////The transform of the plane that will get the IR-decodable image displayed in the scene.
    private Material decodedFeedMaterial; ////The material that will display the IR-decodable image in the scene.

    public bool encodeToTextAsset; ////Are we currently encoding the IR-decodable image to a .asset file?
    public string encodedImageName; ////Name that the encoded images will have.
    private int encodedImageIterator; ////Amount of images already encoded. This is for file management.

    private void Awake()
    {
        SetupWebcamToRenderTexture();
        
    }

    private void SetupWebcamToRenderTexture() ////Sets up the pipeline to be able to convert a WebCamTexture to a RenderTexture. This makes use of materials because it seems to the only easy way to convert from WebCamTexture to RenderTexture.
    {
        cam = WebCamTexture.devices[0];

        camTexture = new WebCamTexture(cam.name, 1280, 1024);
        camTexture.Play();

        renderTexMat = GetComponent<MeshRenderer>().material;
        renderTexMat.mainTexture = camTexture;

        renderTex = new RenderTexture(1280, 1024, 16, RenderTextureFormat.ARGB32);
        renderTex.enableRandomWrite = true;
        renderTex.Create();

        decodedFeedMaterial = decodedFeedTransform.GetComponent<MeshRenderer>().material;



    }

    private void FixedUpdate()
    {
        ProcessWebcamToIRDecodableOnGPU();
    }

    public void StartStopRecording() ////Starts or stops recording/encoding the GPU-processed video feed.
    {
        encodeToTextAsset = !encodeToTextAsset;
        StartCoroutine(EncodeIRImageToTextAsset(15));
    }

    private void ProcessWebcamToIRDecodableOnGPU() ////Sets renderTex to camTexture and converts it into XY-coordinates on the GPU using a compute shader. Only call AFTER SetupWebcamToRenderTexture has been called at least once before.
    {
        Graphics.Blit(renderTexMat.mainTexture, renderTex);

        int kernelHandle = decodingShader.FindKernel("WebcamToXYPos");

        decodingShader.SetTexture(kernelHandle, "Result", renderTex);
        decodingShader.Dispatch(kernelHandle, 1280 / 8, 1024 / 8, 1);

        decodedFeedMaterial.SetTexture("_MainTex", renderTex);

    }

    private IEnumerator EncodeIRImageToTextAsset(float framesPerSecond)
    {
        float waitTime = 1f / framesPerSecond;
        while (encodeToTextAsset)
        {
            yield return new WaitForSeconds(waitTime);
            Texture2D encodableTex2D = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, true);
            RenderTexture.active = renderTex;
            encodableTex2D.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            encodableTex2D.Apply();
            byte[] tex2DBytes = encodableTex2D.EncodeToPNG();
            encodedImageIterator++;
            File.WriteAllBytes(Application.dataPath + "/IRImages/" + encodedImageName + encodedImageIterator + ".bytes", tex2DBytes);
        }
    }

}
