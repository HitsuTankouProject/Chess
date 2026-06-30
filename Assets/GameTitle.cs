using UnityEngine;
using System.Collections;


public class GameTitle : MonoBehaviour
{
    public Camera camera;
    private enum GameTitleStage { Normal , Description , GameStart }
    private CameraView titleView = new(new Vector3(-65, 89.1f, -57), new Vector3(55, 0, 0));
    //private CameraView gameStartView = new(new Vector3(-116.4f, 186.5f, 3), new Vector3(55, 90, 0));
    private CameraView gameDescriptionView = new(new Vector3(-242, 243, 80), new Vector3(90, -90, 0));
    private CameraView gameStartView = new(new Vector3(-242, 243, -80), new Vector3(90, -90, 0));

    private CameraView TargetView(GameTitleStage gameTitleStage)
    {
        switch (gameTitleStage)
        {
            case GameTitleStage.Normal: return titleView;
            case GameTitleStage.Description: return gameDescriptionView;
            case GameTitleStage.GameStart: return gameStartView;
            default: return default;
        }

    }

    public float cameraTurnTime = 1.0f; 
    private IEnumerator TurnCamera(GameTitleStage cameraStage)
    {
        CameraView targetView = TargetView(cameraStage);

        if (Vector3.Distance(camera.transform.position, targetView.position) < 0.01f)
        {
            camera.transform.position = targetView.position;
            camera.transform.rotation = Quaternion.Euler(targetView.angle);
            yield break;
        }

        CameraView nowView = new CameraView(camera.transform.position, camera.transform.rotation.eulerAngles);
        float timer = 0;

        while (timer < cameraTurnTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / cameraTurnTime);

            camera.transform.position =
                Vector3.Lerp(nowView.position, targetView.position, t);

            camera.transform.rotation =
                Quaternion.Slerp(
                    Quaternion.Euler(nowView.angle),
                    Quaternion.Euler(targetView.angle),
                    t);

            yield return null;
        }

        camera.transform.position = targetView.position;
        camera.transform.rotation = Quaternion.Euler(targetView.angle);
    }


    private void Start()
    {
        camera.transform.position = titleView.position;
        camera.transform.rotation = Quaternion.Euler(titleView.angle);
    }

    public bool normal;
    public bool gameStart;

    private void Update()
    {
        if (normal)
        {
            normal = false;
            StartCoroutine(TurnCamera(GameTitleStage.Normal));

        }

        if (gameStart)
        {
            gameStart = false;
            StartCoroutine(TurnCamera(GameTitleStage.GameStart));

        }

    }




}
