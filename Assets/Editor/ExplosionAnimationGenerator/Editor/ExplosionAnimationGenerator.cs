using System.IO;
using UnityEditor;
using UnityEngine;

public static class ExplosionAnimationGenerator
{
    public static void Generate(
        GameObject rootObject,
        string clipBaseName,
        string saveFolder,
        int frameRate,
        int durationFrame,
        float explosionRadius,
        float upForce,
        float rotationAmount,
        bool generateExplode,
        bool generateRecover,
        bool randomRotation,
        bool easeOut)
    {
        if (rootObject == null)
        {
            Debug.LogError("Root Object is null.");
            return;
        }

        CreateFolder(saveFolder);

        float endTime = durationFrame / (float)frameRate;

        Transform root = rootObject.transform;
        Transform[] pieces = new Transform[root.childCount];

        for (int i = 0; i < root.childCount; i++)
            pieces[i] = root.GetChild(i);

        Vector3 center = GetCenter(pieces);

        if (generateExplode)
        {
            AnimationClip explodeClip = CreateAnimationClip(frameRate);

            foreach (Transform piece in pieces)
            {
                AddPieceCurves(
                    explodeClip,
                    piece,
                    root,
                    center,
                    endTime,
                    explosionRadius,
                    upForce,
                    rotationAmount,
                    randomRotation,
                    easeOut,
                    false);
            }

            SaveClip(explodeClip, saveFolder, clipBaseName + "_Explode");
        }

        if (generateRecover)
        {
            AnimationClip recoverClip = CreateAnimationClip(frameRate);

            foreach (Transform piece in pieces)
            {
                AddPieceCurves(
                    recoverClip,
                    piece,
                    root,
                    center,
                    endTime,
                    explosionRadius,
                    upForce,
                    rotationAmount,
                    randomRotation,
                    easeOut,
                    true);
            }

            SaveClip(recoverClip, saveFolder, clipBaseName + "_Recover");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Animation Clips Created!", "OK");
    }

    private static AnimationClip CreateAnimationClip(int frameRate)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;
        clip.legacy = false;
        return clip;
    }

    private static Vector3 GetCenter(Transform[] pieces)
    {
        Vector3 center = Vector3.zero;

        foreach (Transform piece in pieces)
            center += piece.position;

        return center / pieces.Length;
    }

    private static void AddPieceCurves(
        AnimationClip clip,
        Transform piece,
        Transform root,
        Vector3 center,
        float endTime,
        float radius,
        float upForce,
        float rotationAmount,
        bool randomRotation,
        bool easeOut,
        bool reverse)
    {
        string path = piece.name;

        Vector3 startPos = piece.localPosition;
        Vector3 startRot = piece.localEulerAngles;

        Vector3 dir = piece.position - center;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Random.onUnitSphere;

        dir.Normalize();
        dir += Vector3.up * upForce;
        dir.Normalize();

        Vector3 endWorldPos = piece.position + dir * Random.Range(radius * 0.6f, radius);
        Vector3 endPos = root.InverseTransformPoint(endWorldPos);

        Vector3 endRot = startRot + GetRandomRotation(rotationAmount, randomRotation);

        if (!reverse)
        {
            SetVector3Curve(clip, path, "m_LocalPosition", startPos, endPos, endTime, easeOut);
            SetEulerCurve(clip, path, startRot, endRot, endTime, easeOut);
        }
        else
        {
            SetVector3Curve(clip, path, "m_LocalPosition", endPos, startPos, endTime, easeOut);
            SetEulerCurve(clip, path, endRot, startRot, endTime, easeOut);
        }
    }

    private static Vector3 GetRandomRotation(float amount, bool random)
    {
        if (!random)
            return new Vector3(amount, amount, amount);

        return new Vector3(
            Random.Range(-amount, amount),
            Random.Range(-amount, amount),
            Random.Range(-amount, amount));
    }

    private static void SetVector3Curve(
        AnimationClip clip,
        string path,
        string property,
        Vector3 start,
        Vector3 end,
        float endTime,
        bool easeOut)
    {
        SetCurve(clip, path, property + ".x", start.x, end.x, endTime, easeOut);
        SetCurve(clip, path, property + ".y", start.y, end.y, endTime, easeOut);
        SetCurve(clip, path, property + ".z", start.z, end.z, endTime, easeOut);
    }

    private static void SetEulerCurve(
        AnimationClip clip,
        string path,
        Vector3 start,
        Vector3 end,
        float endTime,
        bool easeOut)
    {
        SetCurve(clip, path, "localEulerAnglesRaw.x", start.x, end.x, endTime, easeOut);
        SetCurve(clip, path, "localEulerAnglesRaw.y", start.y, end.y, endTime, easeOut);
        SetCurve(clip, path, "localEulerAnglesRaw.z", start.z, end.z, endTime, easeOut);
    }

    private static void SetCurve(
        AnimationClip clip,
        string path,
        string propertyName,
        float startValue,
        float endValue,
        float endTime,
        bool easeOut)
    {
        AnimationCurve curve = new AnimationCurve();

        Keyframe startKey = new Keyframe(0f, startValue);
        Keyframe endKey = new Keyframe(endTime, endValue);

        if (easeOut)
        {
            startKey.outTangent = (endValue - startValue) * 3f;
            endKey.inTangent = 0f;
        }

        curve.AddKey(startKey);
        curve.AddKey(endKey);

        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
            path,
            typeof(Transform),
            propertyName);

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void SaveClip(AnimationClip clip, string folder, string clipName)
    {
        string path = AssetDatabase.GenerateUniqueAssetPath(
            folder + "/" + clipName + ".anim");

        AssetDatabase.CreateAsset(clip, path);

        Debug.Log("Created Animation Clip: " + path);
    }

    private static void CreateFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}