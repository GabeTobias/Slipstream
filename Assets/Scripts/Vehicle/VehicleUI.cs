using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VehicleUI : MonoBehaviour
{
    public Text Speed;
    public Text Slip;
    public Text Gas;
    public Text Steer;
    public Text Place;
    public Text Lap;

    public RectTransform Drafting;
    public Image SpeedGuage;
    public GUISkin skin;

    public RectTransform[] Placements;

    private Vehicle vehicle;
    private RaceTracker tracker;
    private RaceManager Race;

    // Start is called before the first frame update
    void Start()
    {
        vehicle = GetComponent<Vehicle>();
        tracker = GetComponent<RaceTracker>();
        Race = FindObjectOfType<RaceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Create Input Manager System
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return; // No gamepad connected.

        //Get Input Values
        Vector2 steer = gamepad.leftStick.ReadValue();
        float gas = gamepad.rightTrigger.ReadValue();
        float brake = gamepad.leftTrigger.ReadValue();

        //Calculate power input from gas and break
        float power = Mathf.Clamp(gas - (brake * 2), -1.5f, 1f);

        //Set Text from Values
        Speed.text = ((int)vehicle.rigidbody.velocity.magnitude).ToString();
        Slip.text = vehicle.GetSlip().ToString("N2");
        Gas.text = power.ToString("N2");
        Steer.text = steer.x.ToString("N2");

        //Toggle Drafting Popup
        Drafting.localScale = (vehicle.inDraft) ? Vector3.one : Vector3.zero;

        //Set Speed Gauge Percentage
        SpeedGuage.fillAmount = vehicle.rigidbody.velocity.magnitude / (vehicle.max_speed*1.5f);

        //Check if RaceManager Exists
        if (Race != null)
        {
            //Set Place and Lap text from values
            Place.text = getPlace(tracker.position);
            Lap.text = tracker.laps.ToString() + " / " + Race.totalLaps.ToString();

            for (int i = 0; i < Placements.Length; i++) 
            {
                RectTransform rect = Placements[i];
                
                Vector2 np = new Vector2(rect.anchoredPosition.x, -Race.GetPlacement(i + 1) * 27);

                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, np, 0.04f);

                //Hide Unused Placements Boxes
                if (Race.GetPlacement(i + 1) == 0) rect.localScale = new Vector3(0,0,0);
                else rect.localScale = new Vector3(1, 1, 1);

            }
        }
    }

    string getPlace(int val) 
    {
        switch (val) 
        {
            default:
            case 1:
                return "1st";
            case 2:
                return "2nd";
            case 3:
                return "3rd";
            case 4:
                return "4th";
            case 5:
                return "5th";
            case 6:
                return "6th";
            case 7:
                return "7th";
            case 8:
                return "8th";
        }
    }
}
