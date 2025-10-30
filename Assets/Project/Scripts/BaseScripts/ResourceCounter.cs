using UnityEngine;
using UnityEngine.UI;

public class ResourceCounter : MonoBehaviour
{
    [SerializeField]
    private Text text;

    private int countResource = 0;

    public void AddResource()
    {
        countResource++;
        text.text = countResource.ToString();
    }

    public int GetResourceCount()
    {
        return countResource;
    }
    public void SetResourceCount(int count)
    {
        countResource = count;
        text.text = countResource.ToString();
    }

}
