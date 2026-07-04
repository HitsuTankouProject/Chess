using UnityEditor;
using UnityEngine;

public class ExplosionAnimationWindow : EditorWindow
{
    private GameObject rootObject;

    private ChessType chessType;
    private string saveFolder = "Assets/Animations";

    private int frameRate = 60;
    private int durationFrame = 30;

    private float explosionRadius = 2.5f;
    private float upForce = 1.0f;
    private float rotationAmount = 720f;


    private bool generateExplode = true;
    private bool generateRecover = true;
    private bool randomRotation = true;
    private bool easeOut = true;

    [MenuItem("Tools/Explosion Animation Generator")]
    public static void Open()
    {
        GetWindow<ExplosionAnimationWindow>("Explosion Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Explosion Animation Generator", EditorStyles.boldLabel);

        GUILayout.Space(8);

        rootObject = (GameObject)EditorGUILayout.ObjectField(
            "Root Object",
            rootObject,
            typeof(GameObject),
            true);
        //clipBaseName = EditorGUILayout.TextField("Clip Base Name", clipBaseName);

        chessType = (ChessType)EditorGUILayout.EnumPopup("Chess Type",chessType);


        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        GUILayout.Space(8);

        frameRate = EditorGUILayout.IntField("Frame Rate", frameRate);
        durationFrame = EditorGUILayout.IntField("Duration Frame", durationFrame);

        explosionRadius = EditorGUILayout.FloatField("Explosion Radius", explosionRadius);
        upForce = EditorGUILayout.FloatField("Up Force", upForce);
        rotationAmount = EditorGUILayout.FloatField("Rotation Amount", rotationAmount);

        GUILayout.Space(8);

        generateExplode = EditorGUILayout.Toggle("Generate Explode", generateExplode);
        generateRecover = EditorGUILayout.Toggle("Generate Recover", generateRecover);
        randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);
        easeOut = EditorGUILayout.Toggle("Ease Out", easeOut);

        GUILayout.Space(12);

        GUI.enabled = rootObject != null;

        if (GUILayout.Button("Generate Animation"))
        {
            ExplosionAnimationGenerator.Generate(
                rootObject,
                chessType.ToString(),
                saveFolder,
                frameRate,
                durationFrame,
                explosionRadius,
                upForce,
                rotationAmount,
                generateExplode,
                generateRecover,
                randomRotation,
                easeOut
            );
        }

        GUI.enabled = true;
    }
}