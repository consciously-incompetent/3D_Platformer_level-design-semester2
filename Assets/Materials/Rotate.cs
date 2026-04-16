using UnityEngine;

public class Rotate : MonoBehaviour
{
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rot = transform.localEulerAngles;
        rot += new Vector3(1,0,0) * 15 * Time.deltaTime;

        //if(rot.x > 360)
        //{
         //   rot.x = 0;
       // }


        transform.localEulerAngles = rot;




    }
}
