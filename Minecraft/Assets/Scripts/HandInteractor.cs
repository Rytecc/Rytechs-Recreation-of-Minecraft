using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteractor : MonoBehaviour
{

    [SerializeField] private PlayerBlockEditor editor;

    public void Interact()
    {
        editor.InteractFunction();
    }

}
