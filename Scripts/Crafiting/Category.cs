using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Category : MonoBehaviour
{
    public GameObject content;
    
    public void Selected()
    {
        foreach (Transform sibling in transform.parent)
        {
            if (sibling == transform) 
            {
                content.SetActive(true);
                continue;
            }
            Category SC = sibling.GetComponent<Category>();
            if (SC != null) SC.content.SetActive(false);
        }
    }
}
