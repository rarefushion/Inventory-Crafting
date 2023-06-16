using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Category : MonoBehaviour
{
    public GameObject content;

    public void Clicked()
    {
        Category c;
        foreach (Transform sibling in transform.parent)
            if (sibling == transform)
                content.SetActive(true);
            else if (sibling.TryGetComponent(out c))
                c.content.SetActive(false);
    }
}
