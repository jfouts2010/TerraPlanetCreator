using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickMe : MonoBehaviour
{
    public TectonicStaticSimulation sim;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnMouseDown()
    {
        Debug.Log(sim.Points[new Vector2(transform.position.x, transform.position.y)].climate.ToString());
    }
}
