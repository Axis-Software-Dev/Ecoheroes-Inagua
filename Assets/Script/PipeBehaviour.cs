using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public class PipeBehavior : MonoBehaviour
{
    enum sectionBehavior
    {
        wheel,
        screw,
        cables
    }
    [SerializeField]
    private sectionBehavior section;
    private InputFeatureUsage<bool> gripUse = CommonUsages.gripButton;
    private List<InputDevice> devices;
    private bool lastValue = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var device in devices)
        {
            Debug.Log(device.name + "\n" + device.characteristics);
        }
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Controller"))
        {
            switch (section)
            {
                case sectionBehavior.wheel:
                    wheelBehaviour(other.gameObject);
                    break;
                case sectionBehavior.screw:
                    //screwBehaviour(other.gameObject);
                    break;
                case sectionBehavior.cables:
                    //cablesBehaviour(other.gameObject);
                    break;
                default:
                    break;
            }
        }
    }

    #region Behaviors
    #region Wheel
    private Quaternion getControllerAngle(GameObject controller) { 
        return controller.transform.rotation;
    }
    private float getAngle(Quaternion angleA,Quaternion angleB)
    {
        if(angleA==null||angleB==null)return 0f;
         float angle = Quaternion.Angle(angleA, angleB);
        return angle;
    }
    private void setWheelRotation(float angle,float currentPosition)
    {
        this.transform.rotation=Quaternion.Euler(0,(currentPosition-angle),0);
    }
    private void wheelBehaviour(GameObject controller)
    {
        Quaternion rotationA=Quaternion.identity,rotationB=Quaternion.identity;
        foreach (var device in devices)
        {
            
            if (device.TryGetFeatureValue(gripUse, out bool gripValue))
            {
                if(gripValue && !lastValue)
                {
                    rotationA = getControllerAngle(controller);
                    Debug.Log("Grip Just Pressed");
                }
                else if(gripValue && lastValue)
                {
                    rotationB = getControllerAngle(controller);
                    Debug.Log("Grip Held");
                }
                   
                    float angle = getAngle(rotationA, rotationB);
                    float currentPosition = this.transform.rotation.eulerAngles.y;
                    setWheelRotation(angle, currentPosition);


            }
            else
            {
                lastValue = false;
            }
        }
    }


    #endregion
    #endregion
}
