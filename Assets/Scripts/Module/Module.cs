using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Module : MonoBehaviour
{
    public int ID;
    private static int nextID;
    protected NetworkGraph networkGraph = NetworkGraph.Instance;
    protected Data data = Data.Instance;
    public static Module focusedModule;
    public float edgeCostsModifier = 1;

    public static int maxActivePeople = 500;

    public GameObject frame;
    
    public static void UnFocusCurrentModule()
    {
        if(focusedModule != null) FocusModule(focusedModule);
    }

    public static void FocusModule(Module module)
    {
        Data data = Data.Instance;
        if (focusedModule != null)
        {
            focusedModule.frame.SetActive(false);
            
            List<PersonBase> activePersons = data.PersonsPresentInModule(focusedModule);
            foreach (PersonBase person in activePersons)
            {
                if(person.GetUpdateMode() != UpdateMode.INSIDE) person.SetUpdateModeOnFocusSwitch(UpdateMode.CITYOUTSIDE, focusedModule);
            }

        }
        if (focusedModule == module)
        {
            focusedModule = null;
            return;
        }

        focusedModule = module;
        focusedModule.frame.SetActive(true);

        List<PersonBase> personsToActivate = data.PersonsPresentInModule(focusedModule);
        int i = 0;
        foreach (PersonBase person in personsToActivate)
        {
            if (i > maxActivePeople) break;
            if (person.GetUpdateMode() != UpdateMode.INSIDE)
            {
                person.SetUpdateModeOnFocusSwitch(UpdateMode.MODULEOUTSIDE, focusedModule);
                i++;
            }
        }
    } 

    public bool IsFocused()
    {
        return (this == focusedModule);
    }

    /// <value> Property <c>vertices</c> lists all local vertices of the module</value>
    protected List<Vertex> vertices = new List<Vertex>();
   
    /// <value> Property <c>vertices</c> lists all 12 module connecting vertices of the module clockwise from top left corner</value>
    protected List<Vertex> connectingVertices = new List<Vertex>(); //Public so CityBuilder can connect them

    /// <value> Property <c>edges</c> lists all local edges</value>
    protected List<System.Tuple<Vertex, Vertex>> edges = new List<System.Tuple<Vertex, Vertex>>();

    protected void CreateLocalVerticesAndEdges()
    {
        Transform verticesHolder = transform.Find("Vertices");
        foreach (Transform child in verticesHolder)
        {
            Vertex vertex = child.gameObject.GetComponent<Vertex>();
            //vertex.position = vertex.transform.position; //Assigning here avoids race conditions
            if(vertex.connecting != "") connectingVertices.Add(vertex);
            vertices.Add(vertex);
            foreach(GameObject neighbour in vertex.neighbours)
            {
                Vertex neighbourVertex = neighbour.GetComponent<Vertex>();
                edges.Add(new System.Tuple<Vertex, Vertex>(vertex, neighbourVertex));
            }
        }
        connectingVertices = connectingVertices.OrderBy(vertex => int.Parse(vertex.connecting)).ToList();
    }


    //THIS NEEDS TO BE EXECUTED AFTER ALL IDs HAVE BEEN ASSIGNED TO BUILDINGS
    protected void AddToGlobalNetwork()
    {     
        //Assign global IDs, coordinates to Vertices and add them to global Network
        foreach(Vertex vertex in vertices)
        {

            vertex.moduleID = ID;
            networkGraph.AddVertex(vertex);
        }

        //Add Edges to global Network
        foreach (System.Tuple<Vertex, Vertex> edge in edges)
        {
            float distance = Vector3.Distance(edge.Item1.transform.position, edge.Item2.transform.position);
            networkGraph.AddEdge(edge.Item1, edge.Item2, distance * edgeCostsModifier);
        }

        //Deactivate Object?
    }

    protected void ConnectToNeighbours()
    {
        List<List<Module>> modules = CityBuilder.GetModules();
        int modulesPerRow = modules.Count;
        int distance = 5; //Distance between connecting vertices


        int x = -1;
        int y = -1;
        for(int row = 0; row < modules.Count; row++)
        {
            if (modules[row].Contains(this))
            {
                y = row;
                x = modules[row].IndexOf(this);
            }
        }

        float moduleRot = this.gameObject.transform.eulerAngles.y;       
        int rotStepsModule = (int)(moduleRot / 90);
        int moduleVertexOffset = mod(0 - rotStepsModule * 9, 36);


        //GET BY INDEX INSTEAD!!
        if (x < modulesPerRow - 1) //Right Neighbour exists
        {

            Module rightNeigh = modules[y][x + 1];
            float rightNeighRot = rightNeigh.gameObject.transform.eulerAngles.y;
            int rotStepsRight = (int)(rightNeighRot / 90);
            int rightNeighVertexOffset = mod(0 - rotStepsRight * 9, 36);

            for (int j = 0; j < 9; j++)
            {
                networkGraph.AddEdge(this.connectingVertices[(j + 9 + moduleVertexOffset) % 36], rightNeigh.connectingVertices[(35 - j + rightNeighVertexOffset) % 36], distance);
                networkGraph.AddEdge(rightNeigh.connectingVertices[(35 - j + rightNeighVertexOffset) % 36], this.connectingVertices[(j + 9 + moduleVertexOffset) % 36], distance);


                //Testpurposes:
                this.connectingVertices[(j + 9 + moduleVertexOffset) % 36].neighbours.Add(rightNeigh.connectingVertices[(35 - j + rightNeighVertexOffset) % 36].gameObject);
                rightNeigh.connectingVertices[(35 - j + rightNeighVertexOffset) % 36].neighbours.Add(this.connectingVertices[(j + 9 + moduleVertexOffset) % 36].gameObject);
            }
        }

        if(y < modulesPerRow - 1) //Top Neighbour exists
        {
            Module topNeigh = modules[y + 1][x];
            float topNeighRot = topNeigh.gameObject.transform.eulerAngles.y;
            int rotStepsTop = (int)(topNeighRot / 90);
            int topNeighVertexOffset = mod(0 - rotStepsTop * 9, 36);

            for (int j = 0; j < 9; j++){
                networkGraph.AddEdge(this.connectingVertices[(j + moduleVertexOffset) % 36], topNeigh.connectingVertices[(26 - j + topNeighVertexOffset) % 36], distance);
                networkGraph.AddEdge(topNeigh.connectingVertices[(26 - j + topNeighVertexOffset) % 36], this.connectingVertices[(j + moduleVertexOffset) % 36], distance);


                //Testpurposes:
                this.connectingVertices[(j + moduleVertexOffset) % 36].neighbours.Add(topNeigh.connectingVertices[(26 - j + topNeighVertexOffset) % 36].gameObject);
                topNeigh.connectingVertices[(26 - j + topNeighVertexOffset) % 36].neighbours.Add(this.connectingVertices[(j + moduleVertexOffset) % 36].gameObject);
            }
        }
    }

    //Small helper method. Need True mod
    private int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    

    protected virtual void Awake()
    {
        //IDs need to be assigned from top left to bottom right
        ID = nextID;
        nextID++;
        data.AddModule(this);
        CreateLocalVerticesAndEdges();
        AddToGlobalNetwork();
    }

    //THIS NEEDS TO BE EXECUTED AFTER CityBuilder Awake()
    protected virtual void Start()
    {
        ConnectToNeighbours();
    }
}
