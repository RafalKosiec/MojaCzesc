using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnPlay : MonoBehaviour
{
    // Start is called before the first frame update
    public bool doIDoIt;
    void Start()
    {
        if(doIDoIt) gameObject.SetActive(false);
    }

}
