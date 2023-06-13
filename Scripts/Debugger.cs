using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryCrafting
{
    public class Debugger : MonoBehaviour
    {

        [Header("Object References")]
        public GameObject itemCountPrefab;
        public Transform totalsParent;
        public Transform realityParent;


        private void Update()
        {
            List<Transform> toDestroy = new List<Transform>();
            Dictionary<string, TMP_Text> acountedQaunTextByItemName = new Dictionary<string, TMP_Text>();
            foreach (Transform obj in totalsParent)
                toDestroy.Add(obj);
            foreach (Transform obj in toDestroy)
                Destroy(obj.gameObject);
            foreach (string i in InventoryController.current.itemSlotsByItemName.Keys)
            {
                int q = 0;
                foreach (ItemSlot IS in InventoryController.current.itemSlotsByItemName[i])
                    q += IS.Quantity;
                if (!acountedQaunTextByItemName.ContainsKey(i))
                {
                    GameObject totalOBJ = Instantiate(itemCountPrefab, totalsParent);
                    acountedQaunTextByItemName.Add(i, totalOBJ.transform.GetChild(0).GetComponent<TMP_Text>());
                    totalOBJ.GetComponent<Image>().sprite = InventoryController.current.itemByName[i].image;
                }
                acountedQaunTextByItemName[i].text = q.ToString();
            }
            toDestroy.Clear();
            foreach (Transform obj in realityParent)
                toDestroy.Add(obj);
            foreach (Transform obj in toDestroy)
                Destroy(obj.gameObject);
            Dictionary<string, int> quanByItemName = new Dictionary<string, int>();
            Dictionary<string, TMP_Text> realityQaunTextByItemName = new Dictionary<string, TMP_Text>();
            foreach (Transform obj in InventoryController.current.slotsParent.transform)
            {
                ItemSlot IS;
                if (obj.gameObject.TryGetComponent(out IS) && IS.Item != null)
                {
                    if (!quanByItemName.ContainsKey(IS.ItemName))
                        quanByItemName.Add(IS.ItemName, 0);
                    quanByItemName[IS.ItemName] += IS.Quantity;
                }
            }
            foreach (string i in quanByItemName.Keys)
            {
                if (!realityQaunTextByItemName.ContainsKey(i))
                {
                    GameObject totalOBJ = Instantiate(itemCountPrefab, realityParent);
                    realityQaunTextByItemName.Add(i, totalOBJ.transform.GetChild(0).GetComponent<TMP_Text>());
                    totalOBJ.GetComponent<Image>().sprite = InventoryController.current.itemByName[i].image;
                }
                realityQaunTextByItemName[i].text = quanByItemName[i].ToString();
            }
        }
    }
}
