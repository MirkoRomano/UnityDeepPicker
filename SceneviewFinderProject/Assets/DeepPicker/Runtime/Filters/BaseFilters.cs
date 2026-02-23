using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public class LaerFilter : IFilterable
    {
        public string FilterKeyword => "lay:";
        public uint FilterIndex => 3;

        public bool Evaluate(IFilterContext context)
        {
            return context.SearchFilter.StartsWith(FilterKeyword, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<QueryableItem> Filter(IFilterContext context)
        {
            LayerMask mask = LayerMask.GetMask(context.FilterWord);
            return context.Objects.Where(o =>
            {
                GameObject go = o.As<GameObject>();
                return go != null && ((mask.value & (1 << go.layer)) != 0);
            });
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
