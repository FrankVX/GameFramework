using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : BaseBehaviour
{
    List<IManager> managers;

    private void Awake()
    {
        managers = new List<IManager>()
        {
            AssetManager.Instance,
            ConfigManager.Instance,
        };
        RegisterListener("OnGameStart", () => print("GameStarted!"));
    }

    void Start()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        foreach (var m in managers)
        {
            if (!m.IsInitialized)
                yield return m.Initialize();
        }

        OnStarted();
    }

    void OnStarted()
    {
        DispatchEventAsync("OnGameStart");
    }

}
