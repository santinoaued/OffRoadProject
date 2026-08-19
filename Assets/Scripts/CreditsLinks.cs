using UnityEngine;
using System.Collections.Generic;

public class CreditsLinks : MonoBehaviour
{
    [System.Serializable]
    public class CreditLink
    {
        public string assetName;
        public string url;
    }

    [Header("Asset Store Links")]
    [SerializeField] private List<CreditLink> creditLinks = new List<CreditLink>();

    public void OpenLink(int index)
    {
        if (index < 0 || index >= creditLinks.Count)
        {
            Debug.LogWarning($"CreditsLinks: index {index} fuera de rango");
            return;
        }

        Application.OpenURL(creditLinks[index].url);
    }
}