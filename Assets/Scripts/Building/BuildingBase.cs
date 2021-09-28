using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    //Identifier
    public int ID { get; private set; }
    protected static int nextID;

    //Data Singleton
    protected Data data = Data.Instance;
    
    //Network-Graph
    private NetworkGraph networkGraph = NetworkGraph.Instance;
    public Vertex buildingVertex;    

    /// <summary>
    /// Function is called by <see cref="PersonBase"/> whenever the person enters the building.
    /// PersonBases logs the person's entrance into data. Add here additional functionalities if needed.
    /// </summary>
    public virtual void OnBuildingEnter(PersonBase person) { }

    /// <summary>
    /// Function is called by <see cref="PersonBase"/> whenever the person exits the building.
    /// PersonBases logs the person's exit into data. Add here additional functionalities if needed.
    /// </summary>
    public virtual void OnBuildingExit(PersonBase person) { }

    /// <summary>
    /// Adds the building to the <see cref="Data"/> singleton and attaches it to the <see cref="NetworkGraph"/>
    /// </summary>
    private void SetupBuilding()
    {
        //Find module object by traversing GameObject hierarchy
        GameObject obj = gameObject;
        while (obj.GetComponent<Module>() == null)
        {
            obj = obj.transform.parent.gameObject;
        }

        //Add Building to Data and Module
        data.AddBuilding(this);
        data.AddBuildingToModule(this, obj.GetComponent<Module>());

        //Find closest Vertex to assign the Building to
        Transform verticesObj = obj.transform.GetChild(0);
        Vertex closestVertex = null;
        float shortestDistance = float.PositiveInfinity;
        foreach (Transform child in verticesObj)
        {
            Vertex vertex = child.gameObject.GetComponent<Vertex>();            
            float distance = Vector3.Distance(vertex.transform.position, this.transform.position);
            if ((distance <= shortestDistance) && vertex.isPedestrian)
            {
                shortestDistance = distance;
                closestVertex = vertex;
            }
        }

        //Attach Building to Vertex
        if (closestVertex == null) throw new System.Exception("No Vertex Found");
        networkGraph.AddBuildingVertex(ID, closestVertex);
        buildingVertex = closestVertex;
    }

    #region Infection Tracking

    public abstract float GetIndoorExposure(PersonBase person1, PersonBase person2, float timeExposed);

    #endregion

    protected virtual void Awake()
    {
        ID = nextID;
        nextID++;

        SetupBuilding();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }
}
