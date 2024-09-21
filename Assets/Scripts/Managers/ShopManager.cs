using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject _profile;

    public void PressProfile()
    {
        _profile.SetActive(true);

        Time.timeScale = 0;
        //Set the clip for effect audio
        AudioManager.PlayMenuSelect();
    }

    public void BackProfile()
    {
        _profile.SetActive(false);

        Time.timeScale = 1;
        //Set the clip for effect audio
        AudioManager.PlayMenuSelect();
    }
}
