using System.Collections.ObjectModel;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class GraphViewModel<TVertex,TEdge> : NotifyPropertyChangedBase
    {
        private readonly IGraphArranger<IVertexViewModel<TVertex>,IEdgeViewModel<TEdge>> arranger;

        public GraphViewModel(
            IGraph<IVertexViewModel<TVertex>, IEdgeViewModel<TEdge>> graph, 
            IGraphArranger<IVertexViewModel<TVertex>,IEdgeViewModel<TEdge>> arranger)
        {
            Graph = graph;
            this.arranger = arranger;
            FillObservableCollections();

            Vertices.ForEach(v => v.RequestDelete += (sender, args) => DeleteVertex(((IVertexViewModel<TVertex>)sender).Object));
            RearrangeGraph();
        }

        private void FillObservableCollections()
        {
            Vertices.Clear();
            Edges.Clear();
            Graph.Vertices.Select(v => v.Object).ForEach(Vertices.Add);
            Graph.Edges.Select(v => v.Object).ForEach(Edges.Add);
        }
        public void SynchronizeGraphAndViewModel()
        {
            // Changes to view model are always applied to graph,
            // but if graph is modified from other sources,
            // we need to pull all vertices and edges again
            FillObservableCollections();
            RearrangeGraph();
        }

        public IGraph<IVertexViewModel<TVertex>, IEdgeViewModel<TEdge>> Graph { get; }

        public ObservableCollection<IVertexViewModel<TVertex>> Vertices { get; } = new ObservableCollection<IVertexViewModel<TVertex>>();
        public ObservableCollection<IEdgeViewModel<TEdge>> Edges { get; } = new ObservableCollection<IEdgeViewModel<TEdge>>();

        public IVertex AddVertex(IVertexViewModel<TVertex> vertexViewModel)
        {
            var vertex = new Vertex<IVertexViewModel<TVertex>>(vertexViewModel.VertexId)
            {
                Object = vertexViewModel
            };
            Graph.AddVertex(vertex);
            Vertices.Add(vertexViewModel);
            vertexViewModel.RequestDelete += (sender, args) => DeleteVertex(((IVertexViewModel<TVertex>)sender).Object);
            RearrangeGraph();
            return vertex;
        }



        public IEdge ConnectNodes(IEdgeViewModel<TEdge> connection, TVertex vertex1Object, TVertex vertex2Object, bool isDirected)
        {
            var vertex1 = Graph.Vertices.Single(v => v.Object.Object.Equals(vertex1Object));
            var vertex2 = Graph.Vertices.Single(v => v.Object.Object.Equals(vertex2Object));
            return ConnectNodes(connection, vertex1.Id, vertex2.Id, isDirected);
        }

        public IEdge ConnectNodes(IEdgeViewModel<TEdge> connection, uint vertex1Id, uint vertex2Id, bool isDirected)
        {
            var edge = new Edge<IEdgeViewModel<TEdge>>(
                Graph.GetUnusedEdgeId(),
                vertex1Id,
                vertex2Id)
            {
                IsDirected = isDirected,
                Object = connection
            };
            Graph.AddEdge(edge);
            Edges.Add(connection);
            RearrangeGraph();
            return edge;
        }

        public void RemoveConnection(uint vertex1Id, uint vertex2Id)
        {
            var matchingEdges = Graph.Edges
                .Where(e => e.HasVertex(vertex1Id) && e.HasVertex(vertex2Id))
                .ToList();
            foreach (var edge in matchingEdges)
            {
                Graph.RemoveEdge(edge);
                Edges.Remove(edge.Object);
            }
            RearrangeGraph();
        }

        public void DeleteVertex(TVertex vertex)
        {
            var matchingVertex = Graph.Vertices.SingleOrDefault(v => v.Object.Object.Equals(vertex));
            if(matchingVertex == null)
                return;
            DeleteVertex(matchingVertex);
        }
        private void DeleteVertex(IVertex<IVertexViewModel<TVertex>> vertex)
        {
            Graph.RemoveVertex(vertex);
            Vertices.Remove(vertex.Object);
            RearrangeGraph();
        }

        private void RearrangeGraph()
        {
            var vertexPositions = arranger.Arrange(Graph);
            foreach (var vertexPosition in vertexPositions)
            {
                var vertexId = vertexPosition.Key;
                Graph.GetVertexFromId(vertexId).Object.SetPosition(vertexPosition.Value.X, vertexPosition.Value.Y);
            }

            var centerOffset = new Point2D(arranger.NodeWidth/2, arranger.NodeHeight / 2);
            foreach (var edge in Graph.Edges)
            {
                edge.Object.Point1 = vertexPositions[edge.Vertex1Id] + centerOffset;
                edge.Object.Point2 = vertexPositions[edge.Vertex2Id] + centerOffset;
            }
        }
    }
}
