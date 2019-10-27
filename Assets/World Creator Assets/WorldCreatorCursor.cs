using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCreatorCursor : MonoBehaviour
{
    public ItemHandler handler;
    Item current;
    // Start is called before the first frame update
    void Start()
    {
        current = handler.itemPack.items[0];
        current.obj = Instantiate(current.obj);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
