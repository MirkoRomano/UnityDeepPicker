using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sparkling.DeepClicker
{
    internal class ComponentFilter : IFilterable
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

    internal class LabelFilter : IFilterable
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

    internal class TagFilter : IFilterable
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

    internal class LayerFilter : IFilterable
    {
        const int MIN_LAYER_INDEX = 0;
        const int MAX_LAYER_INDEX = sizeof(int) * 8;
        public string FilterKeyword => "lay:";
        public uint FilterIndex => 3;

        public bool Evaluate(IFilterContext context)
        {
            return context.SearchFilter.StartsWith(FilterKeyword, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<QueryableItem> Filter(IFilterContext context)
        {
            if(!int.TryParse(context.FilterWord, out int layerIndex))
            {
                layerIndex = LayerMask.NameToLayer(context.FilterWord);
            }

            if(layerIndex < MIN_LAYER_INDEX || layerIndex > MAX_LAYER_INDEX)
            {
                return Enumerable.Empty<QueryableItem>();
            }

            int layerMask = 1 << layerIndex;
            return context.Objects.Where(o => o.HasLayerMask(layerMask));
        }
    }

    internal class NameFilter : IFilterable
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
