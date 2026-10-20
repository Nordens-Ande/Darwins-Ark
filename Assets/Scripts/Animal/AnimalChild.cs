using Assets.Scripts.Environment;
using UnityEngine;

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
        if (Physics.Raycast(transform.position, transform.up, out RaycastHit hit, 1)) //Trampled
        { 
            AnimalAI animal = hit.transform.GetComponent<AnimalAI>();
            if (animal != null) 
            {
                transform.localScale = new Vector3(transform.localScale.x, 0.01f, transform.localScale.z);
            }
        }
        else if (currentTime > growthTimerMax) //Grew up
        {
            GrowUp();
        }
        else if (CheckDistanceToParent()) //Far away from parent
        {
            FollowParent(parent.transform.position);
        }
        else if (tempPosition == parent.transform.position) //Parent stands still
        {
            FoolAround();
        }
        else
        {

        }
        tempPosition = parent.transform.position;
        ResetSize();
    }

    private void ResetSize()
    {
        transform.localScale = parent.transform.localScale * 0.3f;
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
