using System.Collections.Generic;
using Commons.Mathematics;

namespace SharedViewModels.Helpers
{
    public interface IGraphArranger<TVertex, in TEdge>
    {
        double NodeWidth { get; }
        double NodeHeight { get; }
        Dictionary<uint, Point2D> Arrange(IGraph<TVertex, TEdge> graph);
    }
}