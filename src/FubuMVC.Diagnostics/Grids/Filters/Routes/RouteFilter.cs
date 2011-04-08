﻿using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Diagnostics.Grids.Columns.Routes;
using FubuMVC.Diagnostics.Models.Grids;

namespace FubuMVC.Diagnostics.Grids.Filters.Routes
{
	public class RouteFilter : GridFilterBase<RouteColumn, BehaviorChain>
    {
    	public RouteFilter(RouteColumn column)
			: base(column)
    	{
    	}

        public override bool Matches(BehaviorChain target, JsonGridFilter filter)
        {
        	return startsWith(target, filter);
        }
    }
}