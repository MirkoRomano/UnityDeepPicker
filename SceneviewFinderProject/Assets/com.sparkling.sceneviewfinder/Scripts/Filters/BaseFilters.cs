using System;
using System.Collections.Generic;
using System.Linq;

namespace Sparkling.SceneFinder
{
    public class ComponentFilter : IFilterable
    {
        public string FilterKeyword => "t:";
        public uint FilterIndex => 0;

        public bool Evaluate(IFilterContext context)
        {
            return context.SearchFilter.StartsWith(FilterKeyword, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<QueryableItem> Filter(IFilterContext context)
        {
            return context.Objects.Where(o => o.HasComponent(context.FilterWord));
        }
    }

    public class LabelFilter : IFilterable
    {
        public string FilterKeyword => "l:";
        public uint FilterIndex => 1;

        public bool Evaluate(IFilterContext context)
        {
            return context.SearchFilter.StartsWith(FilterKeyword, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<QueryableItem> Filter(IFilterContext context)
        {
            return context.Objects.Where(o => o.HasLabel(context.FilterWord));
        }
    }

    public class TagFilter : IFilterable
    {
        public string FilterKeyword => "tag:";
        public uint FilterIndex => 2;

        public bool Evaluate(IFilterContext context)
        {
            return context.SearchFilter.StartsWith(FilterKeyword, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<QueryableItem> Filter(IFilterContext context)
        {
            return context.Objects.Where(o => o.HasTag(context.FilterWord));
        }
    }

    public class NameFilter : IFilterable
    {
        public string FilterKeyword => "";
        public uint FilterIndex => uint.MaxValue;

        public bool Evaluate(IFilterContext context)
        {
            return true;
        }

        public IEnumerable<QueryableItem> Filter(IFilterContext context)
        {
            return context.Objects.Where(o => o.NameContains(context.SearchFilter));
        }
    }
}
