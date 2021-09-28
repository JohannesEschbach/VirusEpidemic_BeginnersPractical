using UnityEngine;


public class RTSCamera : MonoBehaviour

{

    // Public Variables


    public float panSpeed = 20f;


    public float rotSpeed = 5f;


    public float zoomSpeed = 100f;


    public float borderWidth = 10f;


    public bool edgeScrolling = true;


    public Camera cam;



    private float zoomMin = 10.0f;

    private float zoomMax = 100.0f;

    private float mouseX, mouseY;


    private Vector3 currentCenter = new Vector3(150, 150, 150);

    void Start()

    {

        // On start, get a reference to the Main Camera

        cam = Camera.main;

    }


    void Update()

    {

        Movement();

        Rotation();

        Zoom();

    }


    void Movement()

    {

        // Local variable to hold the camera target's position during each frame

        Vector3 pos = transform.position;

        // Local variable to reference the direction the camera is facing (Which is driven by the Camera target's rotation)

        Vector3 forward = transform.forward;

        // Ensure the camera target doesn't move up and down

        forward.y = 0;

        // Normalize the X, Y & Z properties of the forward vector to ensure they are between 0 & 1

        forward.Normalize();


        // Local variable to reference the direction the camera is facing + 90 clockwise degrees (Which is driven by the Camera target's rotation)

        Vector3 right = transform.right;

        // Ensure the camera target doesn't move up and down

        right.y = 0;

        // Normalize the X, Y & Z properties of the right vector to ensure they are between 0 & 1

        right.Normalize();


        // Move the camera (camera_target) Forward relative to current rotation if "W" is pressed or if the mouse moves within the borderWidth distance from the top edge of the screen

        if (Input.GetKey("w") || edgeScrolling == true && Input.mousePosition.y >= Screen.height - borderWidth)

        {

            pos += forward * panSpeed * Time.deltaTime;

        }


        // Move the camera (camera_target) Backward relative to current rotation if "S" is pressed or if the mouse moves within the borderWidth distance from the bottom edge of the screen

        if (Input.GetKey("s") || edgeScrolling == true && Input.mousePosition.y <= borderWidth)

        {

            pos -= forward * panSpeed * Time.deltaTime;

        }


        // Move the camera (camera_target) Right relative to current rotation if "D" is pressed or if the mouse moves within the borderWidth distance from the right edge of the screen

        if (Input.GetKey("d") || edgeScrolling == true && Input.mousePosition.x >= Screen.width - borderWidth)

        {

            pos += right * panSpeed * Time.deltaTime;

        }


        // Move the camera (camera_target) Left relative to current rotation if "A" is pressed or if the mouse moves within the borderWidth distance from the left edge of the screen

        if (Input.GetKey("a") || edgeScrolling == true && Input.mousePosition.x <= borderWidth)

        {

            pos -= right * panSpeed * Time.deltaTime;

        }


        // Setting the camera target's position to the modified pos variable

        if(Vector3.Distance(pos, currentCenter) < 120) transform.position = pos;

    }


    void Rotation()

    {

        // If Mouse Button 1 is pressed, (the secondary (usually right) mouse button)

        if (Input.GetMouseButton(1))

        {

            // Our mouseX variable gets set to the X position of the mouse multiplied by the rotation speed added to it.

            mouseX += Input.GetAxis("Mouse X") * rotSpeed;



            // Our mouseX variable gets set to the Y position of the mouse multiplied by the rotation speed added to it.

            mouseY -= Input.GetAxis("Mouse Y") * rotSpeed;

            
            //allow steeper angle when closer to ground
            float factor = (transform.position.y - 10) / 100;
            // Clamp the minimum and maximum angle of how far the camera can look up and down.
            mouseY = Mathf.Clamp(mouseY, 35 + factor * 45, 90);

            // Set the rotation of the camera target along the X axis (pitch) to mouseY (up & down) & Y axis (yaw) to mouseX (left & right), the Z axis (roll) is always set to 0 as we do not want the camera to roll.

            transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {

            //allow steeper angle when closer to ground
            float factor = (transform.position.y - 10) / 100;
            // Clamp the minimum and maximum angle of how far the camera can look up and down.
            mouseY = Mathf.Clamp(mouseY, 35 + factor * 45, 90);

            // Set the rotation of the camera target along the X axis (pitch) to mouseY (up & down) & Y axis (yaw) to mouseX (left & right), the Z axis (roll) is always set to 0 as we do not want the camera to roll.

            transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        }
    }


    void Zoom()

    {

        // Local variable to temporarily store our camera's position

        Vector3 camPos = cam.transform.position;

        // Local variable to store the distance of the camera from the camera_target

        //float distance = Vector3.Distance(transform.position, cam.transform.position);

        // Cast ray to mouse positionon
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            float distance = Vector3.Distance(transform.position, hit.transform.position);



            // zoom in

            if (Input.GetAxis("Mouse ScrollWheel") > 0f && distance > zoomMin)

            {

                cam.transform.Translate(ray.direction * zoomSpeed * Time.deltaTime, Space.World);                
            }


            // zoom out

            if (Input.GetAxis("Mouse ScrollWheel") < 0f && distance < zoomMax)

            {

                cam.transform.Translate(-1 * ray.direction * zoomSpeed * Time.deltaTime, Space.World);
            }
            if (cam.transform.position.y < 0) cam.transform.position = new Vector3(cam.transform.position.x, 1, cam.transform.position.z);
        }
    }


    public void placeCamera(int x, int y)
    {
        int moduleWidth = 300;
        currentCenter = new Vector3(x * moduleWidth + moduleWidth / 2, transform.position.y, y * moduleWidth + moduleWidth / 2);
        transform.position = currentCenter;        
    }
}