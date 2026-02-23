using System.Collections.Generic;

namespace Sparkling.SceneFinder
{
    public interface IFilterable
    {
        string FilterKeyword { get; }
        uint FilterIndex { get; }
        bool Evaluate(IFilterContext context);
        IEnumerable<QueryableItem> Filter(IFilterContext context);
    }

    public interface IFilterContext
    {
        IEnumerable<QueryableItem> Objects { get; }
        IEnumerable<QueryableItem> FilteredObjects { get; }
        IEnumerable<QueryableItem> RootObjects { get; }

        int ObjectsCount { get; }
        int FilteredObjectsCount { get; }
        string SearchFilter { get; }
        string FilterWord { get; }
    }
}
