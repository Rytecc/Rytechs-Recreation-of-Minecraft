using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using TMPro;
using System;

public class LoadMenuClass : MonoBehaviour, IDisposable
{
    [SerializeField] private GameObject LoadUIParent;
    [SerializeField] private Image BackgroundImage;
    [SerializeField] private TextMeshProUGUI StateDisplay;
    [SerializeField] private Image Loadbar;
    [Space]
    [SerializeField] private RawImage GenerationDisplay;
    [SerializeField] private Image GenerationDisplayBorder;
    private bool BackgroundFaded = false;
    private bool StateBarTweenComplete = false;

    public void SetProgressBarAmount(float Amount)
    {
        Loadbar.fillAmount = Amount;
    }

    public void SetProgressState(string State)
    {
        StateDisplay.SetText(State);
    }

    public void Dispose()
    {
        Tween t1 = StateDisplay.DOColor(new Color(255, 255, 255, 0), 1);
        Tween t2 = Loadbar.transform.parent.GetComponent<Image>().DOColor(new Color(255, 255, 255, 0), 1);
        GenerationDisplayBorder.DOColor(new Color(255, 255, 255, 0), 1);
        GenerationDisplay.DOColor(new Color(255, 255, 255, 0), 1);
        Tween t3 = Loadbar.DOColor(new Color(255, 255, 255, 0), 1);
        t3.onComplete += StateBarTweenOutComplete;

    }

    private void StateBarTweenOutComplete() 
    { 
        StateBarTweenComplete = true;

        Tween t4 = BackgroundImage.DOColor(new Color(255, 255, 255, 0), 1);
        t4.onComplete += BackgroundFadeComplete;

        CheckForDisposal();
    }

    private void BackgroundFadeComplete() 
    { 
        BackgroundFaded = true;
        CheckForDisposal();
    }

    private void CheckForDisposal()
    {
        if(BackgroundFaded && StateBarTweenComplete)
        {
            Destroy(LoadUIParent);
        }
    }
}
