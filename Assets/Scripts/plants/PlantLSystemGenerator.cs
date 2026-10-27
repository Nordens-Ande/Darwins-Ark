using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class PlantLSystemGenerator : MonoBehaviour
{
    [Header("Controls")]
    public bool reGenerate = false; //makeshift button in the inspector 
    public bool randomize = true; // random displacements 20%+- depending on settings

    [Header("L-System Settings")]
    // F is a stright line
    public string axiom = "F";
    public string ruleF = "F[+F][-F][&F][^F]";
    [Range(1, 6)]
    public int iterations = 3;

    [Header("Turtle Graphics Settings")]
    public float length = 0.06f;
    public float angle = 45f;
    public float lengthDecay = 0.75f;
    public float thickness = 0.014f;
    public Material branchMaterial;

    private string currentString = "";

    private struct TurtleState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float currentLength;
        public float currentThickness;
    }

    // we automaticly generate a plant when starting the game
    void Start()
    {
        GeneratePlant();
    }

    void Update()
    {
        // makeshift button for the inspector to generate a new plant
        if (reGenerate)
        {
            reGenerate = false;
            GeneratePlant();
        }
    }

    public void GeneratePlant()
    {
        ClearPlant();

        currentString = axiom; //init string with our value
        for (int i = 0; i < iterations; i++) // we rewirte the string x iternation
        {
            currentString = ApplyRules(currentString);
        }

        // this is the final string
        Debug.Log("generated string: " + currentString);
        InterpretString(currentString); // actually making the gameobject of the final string
    }


    // goes through all the childs on the plant to make sure it's compleatly gone 
    public void ClearPlant()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }


    // this is the string expansion
    string chiaroneRules(string input)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char _char in input) // we replace every f with the rule
        {
            if (_char == 'F')
            {
                stringBuilder.Append(ruleF);
            }
            else
            {
                stringBuilder.Append(_char);
            }
        }
        return stringBuilder.ToString();
    }

    string ApplyRules(string input)
    {
        return chiaroneRules(input);
    }


    // this translates the L system in 3d geometry
    void InterpretString(string l_system)
    {
        Stack<TurtleState> stack = new Stack<TurtleState>(); // to remember the tutles states when branching

        //initial values
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        float curLen = length;
        float curThick = thickness;

        foreach (char _char in l_system)
        {
            // we get angle and length
            float currentAngle = angle;
            float currentStepLen = curLen;

            if (randomize)
            {
                //we give them +-20% on the vlaues so it doesnt generate the exact same bush/plant always
                currentAngle *= Random.Range(0.8f, 1.2f);
                currentStepLen *= Random.Range(0.8f, 1.2f);
            }

            // handles each symbol
            switch (_char)
            {
                case 'F':
                    // move forward and calculate the end position of the branch using current rotation and length
                    // this is the basic branch, rest is rules to change where and how to put this one
                    Vector3 nextPosition = currentPosition + currentRotation * Vector3.up * currentStepLen;
                    DrawBranch(currentPosition, nextPosition, curThick);
                    currentPosition = nextPosition;
                    break;

                case '+':
                    // rotate it around z+
                    currentRotation *= Quaternion.Euler(0, 0, currentAngle);
                    break;

                case '-':
                    // rotate it around z-
                    currentRotation *= Quaternion.Euler(0, 0, -currentAngle);
                    break;

                case '&':
                    // rotate it around x+
                    currentRotation *= Quaternion.Euler(currentAngle, 0, 0);
                    break;

                case '^':
                    // rotate it around x+
                    currentRotation *= Quaternion.Euler(-currentAngle, 0, 0);
                    break;

                case '[':
                    // create a new branch to work from
                    stack.Push(new TurtleState
                    {
                        position = currentPosition,
                        rotation = currentRotation,
                        currentLength = curLen,
                        currentThickness = curThick
                    });
                    // reduce length and thickness for child branches to make them look natural
                    curLen *= lengthDecay;
                    curThick *= 0.8f;
                    break;

                case ']':
                    // go back to where the branch was created before
                    if (stack.Count > 0)
                    {
                        TurtleState state = stack.Pop();
                        currentPosition = state.position;
                        currentRotation = state.rotation;
                        curLen = state.currentLength;
                        curThick = state.currentThickness;
                    }
                    break;
            }
        }
    }

    // helper function to create an model from the start pos to an end pos
    void DrawBranch(Vector3 start, Vector3 end, float thick)
    {
        GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        branch.transform.SetParent(this.transform);

        Vector3 center = (start + end) * 0.5f;
        float dist = Vector3.Distance(start, end);

        branch.transform.position = center;
        branch.transform.localScale = new Vector3(thick, dist * 0.5f, thick);
        branch.transform.up = (end - start).normalized;

        if (branchMaterial != null)
        {
            branch.GetComponent<Renderer>().material = branchMaterial;
        }
    }
}