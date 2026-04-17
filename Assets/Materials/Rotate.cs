using Unity.Mathematics;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 startRot;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //startRot = new Vector3(180, 60, 0);
        startRot = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        
        startRot.x -= 15 * Time.deltaTime;

        Quaternion rot = Quaternion.Euler(startRot);
        
        //rot += 15 * Time.deltaTime;

        //if(rot.x > 360)
        //{
         //   rot.x = 0;
       // }


        transform.SetLocalPositionAndRotation(transform.localPosition, rot);




    }
}
