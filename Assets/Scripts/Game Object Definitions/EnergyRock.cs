using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRock : MonoBehaviour {
    public GameObject energySpherePrefab;


    float maxTime = 8f;
    float targetTime = 0f;

    private void Start()
    {
        targetTime = Time.fixedTime + maxTime;
        if(!transform.Find("Minimap Image"))
        {
            GameObject childObject = new GameObject("Minimap Image");
            childObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
            renderer.transform.localScale = new Vector3(2F, 2F, 2F);
            childObject.AddComponent<MinimapLockRotationScript>().Initialize();
        }
    }

    private void Awake()
    {
        AIData.energyRocks.Add(this);
    }

    private void OnDestroy()
    {
        AIData.energyRocks.Remove(this);
    }

    void Update () {
        if (Time.fixedTime > targetTime) {
            targetTime = Time.fixedTime + maxTime;

            var x = Instantiate(energySpherePrefab, null, false);
            x.GetComponent<SpriteRenderer>().sortingLayerID = 0;
            x.transform.position = transform.position;

            float dir = Random.Range(0f, 2 * Mathf.PI);
            x.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sin(dir), Mathf.Cos(dir)) * Random.Range(180f, 240f));
        }
	}
}
