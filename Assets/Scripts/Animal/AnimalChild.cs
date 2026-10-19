using Assets.Scripts.Environment;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class AnimalChild : MonoBehaviour
{
    private float growthTimerMax = 150;
    private float currentTime = 0;
    private TileManager tileManager;
    private GameObject parent;
    private float childWalkSpeed;
    private Vector3 tempPosition;
    private bool resetJump;

    public GameObject Parent 
    { 
        get => parent;
        set => parent = value;
    } 
    public float ChildWalkSpeed 
    { 
        get => childWalkSpeed;
        set => childWalkSpeed = value;
    }

    private void Start()
    {
        tileManager = TileManager.Instance;
        tempPosition = parent.transform.position;
        resetJump = true;
    }
    // Update is called once per frame
    void Update()
    {
        
        currentTime += Time.deltaTime;
        StateDecider();
    }

    void StateDecider() 
    {
        if (currentTime > growthTimerMax) 
        {
            GrowUp();
        }
        else if (CheckDistanceToParent()) 
        {
            FollowParent(parent.transform.position);
        }
        else if (tempPosition == parent.transform.position) 
        { 
            FoolAround();
        }
        else 
        { 
        
        }
        tempPosition = parent.transform.position;
    }

    void ResetJumpTimer() 
    {
        currentTime -= Time.deltaTime;
        if(currentTime < 0) 
        {
            currentTime = 4;
        }
    }
    void GrowUp() 
    { 
        AnimalAI childAI = transform.GetComponent<AnimalAI>();
        childAI.enabled = true;
        transform.localScale *= 3;
        Destroy(this);
    }

    bool CheckDistanceToParent() 
    {
        float distance = Vector3.Distance(transform.position, parent.transform.position);
        if(distance > 2) 
        {
            return true;
        }
        else return false;
            
    }

    void FollowParent(Vector3 point) 
    {
        transform.LookAt(point);

        //Makes sure the animal stays at the tileheight its on
        Vector2 currentPostion = new Vector2((int)transform.position.x, (int)transform.position.z);
        Tile currentTile = tileManager.GetTile(currentPostion);
        Vector3 newPoint = new Vector3(point.x, currentTile.GetHeight() + transform.localScale.y / 2, point.z);
        transform.position = Vector3.MoveTowards(transform.position, newPoint, childWalkSpeed * Time.deltaTime);

        if (transform.position.y < currentTile.GetHeight() + transform.localScale.y / 2)
        {
            transform.position += new Vector3(0, 0.5f, 0) * Time.deltaTime;
        }
        else if (transform.position.y > currentTile.GetHeight() + transform.localScale.y / 2)
        {
            transform.position -= new Vector3(0, 0.5f, 0) * Time.deltaTime;
        }
    }

    bool isGrounded() 
    { 
        if(tileManager.GetTile(transform.position).GetHeight() <= transform.position.y) 
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    void FoolAround() 
    {
        transform.position += transform.forward * ChildWalkSpeed * 0.5f * Time.deltaTime;
        transform.position += transform.right * childWalkSpeed * 0.5f * Time.deltaTime;
        if (isGrounded()) 
        {
            resetJump = true;
        }
        if (transform.position.y < tileManager.GetTile(transform.position).GetHeight() + 1 && resetJump)
        {
            transform.position += new Vector3(0, 0.5f, 0) * Time.deltaTime;
        }
        else if (transform.position.y > tileManager.GetTile(transform.position).GetHeight() + 1) 
        {
            resetJump = false;
        }
    }
    void GetTrampled() 
    { 
        
    }
}
