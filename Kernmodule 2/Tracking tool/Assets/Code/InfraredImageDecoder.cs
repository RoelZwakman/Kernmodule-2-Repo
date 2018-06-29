using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfraredImageDecoder : MonoBehaviour {

    public List<TextAsset> images; /////RealToIRCamera-processed images in .bytes format.
    public int imageToDecode;
    private List<List<Vector2Int>> preparedImagesColoredPixels = new List<List<Vector2Int>>(0); /////List of all images's lists of colored pixels' XY coördinates.
    private float minDotDistance = 40;



    [Header("Output parameters")]
    public bool saveAsText;
    public string fileLocation;
    public string fileName;

    [Header("Positional output")]
    public List<Vector2Int> parsedTrackedPositions = new List<Vector2Int>(0);
    //public List<Vector2Int> deserialisedPositions = new List<Vector2Int>(0);

    private void Start()
    {
        PrepareAllIRImagesToVector2IntListList();
    }

    public void DecodeImage(bool saveToText)
    {
        saveAsText = saveToText;
        Debug.Log("Spawning " + preparedImagesColoredPixels[imageToDecode].Count + " temporary cubes.");
        ColoredPixelsFinalDecodingPass(preparedImagesColoredPixels[imageToDecode]);
        //Debug.Log(deserialisedPositions[0]);
    }



    public void PrepareAllIRImagesToVector2IntListList(int startIndex = 0, int amount = 0) 
    {
        if(amount == 0) /////Amount is the default value, so no amount was provided. In this case, just run through all of them from startIndex.
        {
            for(int i = startIndex; i < images.Count; i++)
            {
                preparedImagesColoredPixels.Add(DecodeTexture2DToVector2IntList(images[i]));
                
            }
        }
        else ////Amount isn't the default amount, so in this case you should iterate from startIndex to startIndex + amount.
        {
            for (int i = startIndex; i < startIndex + amount; i++)
            {
                preparedImagesColoredPixels.Add(DecodeTexture2DToVector2IntList(images[i]));
            }
        }

    }

    private List<Vector2Int> DecodeTexture2DToVector2IntList(TextAsset texToDecode) ////Returns all of the colored pixels as XY coördinates.
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(texToDecode.bytes);
        List<Vector2Int> coloredPixels = new List<Vector2Int>(0);

        Color colorCheck = new Color(1, 0, 0, 1);

        for(int x = 0; x < tex.width; x++)
        {
            for(int y = 0; y < tex.height; y++)
            {

                if (tex.GetPixel(x, y) == colorCheck)
                {
                    coloredPixels.Add(new Vector2Int(x, y));
                }
            }
        }

        return coloredPixels;
    }

    public void ColoredPixelsFinalDecodingPass(List<Vector2Int> pixels) /////Does cleaning up of the data that was parsed
    {
        if (pixels.Count != 0)
        {
            List<GameObject> spawnedCubes = new List<GameObject>(0);
            int i = 0;
            while (i < pixels.Count)
            {
                Vector3 spawnPosXYZ = new Vector3(pixels[i].x, pixels[i].y, 0);

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = spawnPosXYZ;


                spawnedCubes.Add(cube);
                parsedTrackedPositions.Add(new Vector2Int((int)spawnPosXYZ.x, (int)spawnPosXYZ.y));

                Collider[] hitColliders = Physics.OverlapSphere(cube.transform.position, minDotDistance);
                for (int j = 0; j < hitColliders.Length; j++)
                {
                    if (hitColliders[j].gameObject != cube)
                    {
                        if (parsedTrackedPositions.Contains(new Vector2Int((int)hitColliders[j].transform.position.x, (int)hitColliders[j].transform.position.y)))
                        {
                            parsedTrackedPositions.Remove(new Vector2Int((int)hitColliders[j].transform.position.x, (int)hitColliders[j].transform.position.y));
                        }
                        spawnedCubes.Remove(hitColliders[j].gameObject);
                        DestroyImmediate(hitColliders[j].gameObject);
                    }
                }

                i++;

            }
            if (saveAsText)
            {
                JsonSerialising.SerialiseList(fileLocation, fileName, parsedTrackedPositions);
                //deserialisedPositions = JsonSerialising.DeserialiseVector2IntList(fileLocation, fileName);
            }

            for (int j = 0; j < spawnedCubes.Count; j++)
            {
                DestroyImmediate(spawnedCubes[j]);
                spawnedCubes.RemoveAt(j);
            }
        }
        else
        {
            Debug.Log("No red pixels in this image.");
        }

    }

    

}
