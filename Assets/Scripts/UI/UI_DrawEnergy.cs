using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NestedParadox.Players;
using UniRx;

public class UI_DrawEnergy : MonoBehaviour
{
    private Slider DrawEnegrySlider;
    void Start()
    {
        DrawEnegrySlider = GetComponent<Slider>();
        DrawEnegrySlider.maxValue = 10;

        PlayerCore.I.PlayerDrawEnergy
        .Subscribe(x=>UpdateDrawEnergyUI(x))
        .AddTo(this);
    }

    private void UpdateDrawEnergyUI(int x){
        DrawEnegrySlider.value = x;
    }
}
