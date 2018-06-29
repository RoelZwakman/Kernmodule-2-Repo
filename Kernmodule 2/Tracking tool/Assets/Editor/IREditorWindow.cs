using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IREditorWindow : EditorWindow {

    [MenuItem("Window/Infrared mo-cap editor")]
    static void Init()
    {
        IREditorWindow window = (IREditorWindow)GetWindow(typeof(IREditorWindow));
        window.Show();
    }

    private int frameToDecode = 0;
    private bool showAssets;
    private InfraredImageDecoder decoder;

    public List<TextAsset> assets = new List<TextAsset>(5);

    public List<Vector2Int> deserialisedList = new List<Vector2Int>(1);

    //public Animation anim; SAVEN ALS ANIMATIE GING HELAAS NIET, OMDAT UNITY'S OBJECTFIELD BIJ ANIMATION'S NOG DE LEGACYVERSIE GEBRUIKT, WAARDOOR JE GEEN ANIMATION KAN ASSIGNEN.

    private AnimationCurve curveX = AnimationCurve.Linear(0, 0, 0, 0);
    private AnimationCurve curveY = AnimationCurve.Linear(0, 0, 0, 0);

    private bool saveAsText;
    private string fileLocation = "../Tracking tool/Assets/Output/txt files/";
    private string fileName = "FrameFile";

    private void OnGUI()
    {
        showAssets = EditorGUILayout.Foldout(showAssets, "TEXT ASSETS");
        if (showAssets)
        {
            if (GUILayout.Button("ADD FRAME"))
            {
                assets.Add(new TextAsset());
            }
            if(GUILayout.Button("REMOVE FRAME") && assets.Count >= 1)
            {
                assets.RemoveAt(assets.Count - 1);
            }

            EditorGUILayout.LabelField("Drop text-assets here:");
            for(int i = 0; i < assets.Count; i++)
            {
                assets[i] = EditorGUILayout.ObjectField(assets[i], typeof(TextAsset), true) as TextAsset;
            }
        }

        frameToDecode = EditorGUILayout.IntField("FRAME TO DECODE", frameToDecode);


        EditorGUILayout.LabelField("NOTE: TEXT ASSETS ARE NOT THE SAME THING AS .TXT FILES!", EditorStyles.boldLabel);
        if (GUILayout.Button("DECODE SINGLE TEXT ASSET"))
        {
            DecodeSingleImage(frameToDecode);
        }

        if(GUILayout.Button("DECODE ALL TEXT ASSETS"))
        {
            DecodeAllImages();
        }

        saveAsText = EditorGUILayout.Toggle("Save as text file", saveAsText);

        EditorGUILayout.LabelField(".TXT FILE SAVING AND LOADING VARIABLES", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("FILE LOCATION");
        fileLocation = EditorGUILayout.TextField(fileLocation);
        EditorGUILayout.LabelField("FILE NAME");
        fileName = EditorGUILayout.TextField(fileName);

        EditorGUILayout.LabelField("ANIMATIONS");
        if(curveX != null && curveY != null)
        {
            curveX = EditorGUILayout.CurveField("Animation on X", curveX);
            curveY = EditorGUILayout.CurveField("Animation on Y", curveY);
        }

        if(GUILayout.Button("DESERIALISE TEXT FILE INTO XY COÖRDINATES"))
        {
            deserialisedList = JsonSerialising.DeserialiseVector2IntList(fileLocation, fileName);
        }

        EditorGUILayout.LabelField("DESERIALISED .TXT FILE DATA", EditorStyles.boldLabel);
        foreach(Vector2Int pair in deserialisedList)
        {
            EditorGUILayout.LabelField("X: " + pair.x.ToString() + ", Y: " + pair.y.ToString());
        }

        //EditorGUILayout.LabelField("Add animation here:"); SAVEN ALS ANIMATIE GING HELAAS NIET, OMDAT UNITY'S OBJECTFIELD BIJ ANIMATION'S NOG DE LEGACYVERSIE GEBRUIKT, WAARDOOR JE GEEN ANIMATION KAN ASSIGNEN IN EEN CUSTOM EDITOR.
        //anim = EditorGUILayout.ObjectField(anim, typeof(Animation), true) as Animation;

        /*
        if(GUILayout.Button("SAVE AS ANIMATIONCLIP"))    
        {
            //SaveAsAnimationClip(curveX, curveY);    
        }
        */

    }

    private void DecodeSingleImage(int img)
    {
        decoder = new InfraredImageDecoder();
        decoder.images = assets;
        decoder.PrepareAllIRImagesToVector2IntListList();
        decoder.imageToDecode = img;

        decoder.fileLocation = fileLocation;
        decoder.fileName = fileName;

        decoder.DecodeImage(saveAsText);
        Debug.Log(decoder.parsedTrackedPositions[0]);
        if(decoder.parsedTrackedPositions != null)
        {
            AddKeyToCurves(decoder.parsedTrackedPositions);
        }
        
    }

    private void DecodeAllImages()
    {
        decoder = new InfraredImageDecoder();
        decoder.images = assets;
        decoder.PrepareAllIRImagesToVector2IntListList();
        decoder.fileLocation = fileLocation;
        decoder.fileName = fileName;

        for (int i = 0; i < assets.Count; i++)
        {
            decoder.imageToDecode = i;
            decoder.DecodeImage(saveAsText);
            if (decoder.parsedTrackedPositions != null)
            {
                AddKeyToCurves(decoder.parsedTrackedPositions);
            }
        }

    }

    private void AddKeyToCurves(List<Vector2Int> positions)
    {
        for(int i = 0; i < positions.Count; i++)
        {
            curveX.AddKey(curveX.length, positions[i].x);
            curveY.AddKey(curveX.length, positions[i].y);
        }
        
    }

    /* SAVEN ALS ANIMATIE GING HELAAS NIET, OMDAT UNITY'S OBJECTFIELD BIJ ANIMATION'S NOG DE LEGACYVERSIE GEBRUIKT, WAARDOOR JE GEEN ANIMATION KAN ASSIGNEN IN EEN CUSTOM EDITOR.
    private void SaveAsAnimationClip(AnimationCurve cx, AnimationCurve cy)
    {
        anim = Resources.Load("MyAnimation.anim") as Animation;
        //Debug.Log(anim.name);

        AnimationClip[] clip = new AnimationClip[1];
        
        clip[0].SetCurve("", typeof(Transform), "localPosition.x", cx);
        clip[0].SetCurve("", typeof(Transform), "localPosition.y", cy);
        AnimationUtility.SetAnimationClips(anim, clip);
        //clip.AddEvent(new AnimationEvent());
        //anim.AddClip(clip, "AnimName");

        /*
        anim.Set
        anim.clip = new AnimationClip();
        anim.clip.SetCurve("", typeof(Transform), "localPosition.x", cx);
        anim.clip.SetCurve("", typeof(Transform), "localPosition.y", cy);
        anim.name = "Animation1";
    }*/

}
