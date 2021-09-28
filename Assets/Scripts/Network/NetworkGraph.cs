using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms.Observers;
using System.Threading;
using System.Collections.Concurrent;

public class NetworkGraph
{
    private static Thread plotter;
    private static Thread plotter2;


    private static NetworkGraph instance;
    private NetworkGraph() { }
    public static NetworkGraph Instance
    {
        get
        {
            if (instance == null) instance = new NetworkGraph();
            return instance;
        }
    }

    public Dictionary<string, Vertex> vertices = new Dictionary<string, Vertex>();
    private AdjacencyGraph<string, Edge<string>> graph = new AdjacencyGraph<string, Edge<string>>();
    private Dictionary<Edge<string>, double> edgeCost = new Dictionary<Edge<string>, double>();
    public void AddVertex(Vertex vertex)
    {
        vertices.Add(vertex.vertex, vertex);
        graph.AddVertex(vertex.vertex);
    }

    public void AddEdge(Vertex vertex1, Vertex vertex2, float costs)
    {
        Edge<string> edge = new Edge<string>(vertex1.vertex, vertex2.vertex);
        graph.AddEdge(edge);
        edgeCost.Add(edge, costs);
    }


    public Dictionary<int, string> buildingVertices = new Dictionary<int, string>();
    public void AddBuildingVertex(int ID, Vertex vertex)
    {
        buildingVertices.Add(ID, vertex.vertex);
    }

    public Vertex GetBuildingVertex(int ID)
    {
        return vertices[buildingVertices[ID]];
    }



    public List<Vertex> ShortestPath(Vertex start, Vertex end) //Make sure that both end and start vertex are included in path
    {    
        if(start == end)
        {
            return new List<Vertex> { start, end };
        }

        System.Func<Edge<string>, double> getWeight = edge => edgeCost[edge];

        DijkstraShortestPathAlgorithm<string, Edge<string>> dijkstra = new DijkstraShortestPathAlgorithm<string, Edge<string>>(graph, getWeight);
        VertexDistanceRecorderObserver<string, Edge<string>> distObserver = new VertexDistanceRecorderObserver<string, Edge<string>>(getWeight);
        distObserver.Attach(dijkstra);

        using (distObserver.Attach(dijkstra))
        {

            VertexPredecessorRecorderObserver<string, Edge<string>> predecessorObserver = new VertexPredecessorRecorderObserver<string, Edge<string>>();
            using (predecessorObserver.Attach(dijkstra))
            {
                dijkstra.Compute(start.vertex);

                IEnumerable<Edge<string>> path;
                predecessorObserver.TryGetPath(end.vertex, out path);

                List<Vertex> vertexRoute = new List<Vertex>();
                foreach (Edge<string> edge in path) vertexRoute.Add(vertices[edge.Source]);

                vertexRoute.Add(end); //Add the final vertex
                vertexRoute.Reverse(); //HUH? why though?

                return vertexRoute;
            }
        }
    }

    public static ConcurrentQueue<System.Tuple<PersonBase, Vertex, Vertex>> routeQueue = new ConcurrentQueue<System.Tuple<PersonBase, Vertex, Vertex>>();
    public static bool working = false;

    public void queueRoute(PersonBase person, Vertex start, Vertex end)
    {
        routeQueue.Enqueue(new System.Tuple<PersonBase, Vertex, Vertex>(person, start, end));
        if (!working) working = true;  
    }


    public void assignRoute()
    {
        while (true)
        {
            if(routeQueue.TryDequeue( out System.Tuple < PersonBase, Vertex, Vertex > tuple)) 
            {
                PersonBase person = tuple.Item1;
                Vertex start = tuple.Item2;
                Vertex end = tuple.Item3;

                List<Vertex> path = ShortestPath(start, end);
                Stack<Vertex> travelRoute = new Stack<Vertex>(path);
                if (travelRoute == null | travelRoute.Count == 0)
                {                        
                    throw new System.Exception("Route Empty");
                }
                person.prevTravelRoute = new Stack<Vertex>(travelRoute); //still dont know why this is necessary
                person.routeLength = travelRoute.Count; //bugfixing line
                person.travelRoute = travelRoute;
                person.waitingForRoute -= 1;
            }

        }
    }

    public void startThread()
    {
        plotter = new Thread(assignRoute);
        plotter2 = new Thread(assignRoute);
        plotter.Start();
        plotter2.Start();

    }
}