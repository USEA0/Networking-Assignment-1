using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour
{
    public int index;
    public void onClick()
    {
        UIManager.Instance.Join(index);
    }
}
