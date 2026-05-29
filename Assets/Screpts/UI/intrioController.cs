using UnityEngine;

public class intrioController : MonoBehaviour
{
    [SerializeField] private GameObject intrioPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject[] intemsPrefabs;
    [SerializeField] private int slotCount;
    
    
    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, intrioPanel.transform).GetComponent<Slot>();
            if(i < intemsPrefabs.Length)
            {
                GameObject item = Instantiate(intemsPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.CurrentItem = item; 
            }
        }
    }
}
