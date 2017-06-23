using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Environment = System.Environment;

namespace Aims.IISAgent
{
	class NoInstancePerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly string _counterName;
		private readonly string _statType;
		private readonly PerformanceCounterCategory _category;
		private readonly INodeRefCreator _nodeRefCreator;

		public NoInstancePerformanceCounterCollector(string categogyName, string counterName, string statType,
			INodeRefCreator nodeRefCreator)
		{
			if(nodeRefCreator == null)
				throw new ArgumentNullException();
			_nodeRefCreator = nodeRefCreator;
			_counterName = counterName;
			_statType = statType;
			_category = PerformanceCounterCategory
				.GetCategories()
				.SingleOrDefault(category => category.CategoryName.Equals(categogyName,
					StringComparison.InvariantCultureIgnoreCase));
			if (_category == null)
			{
				throw new MyExceptions.CategoryNotFoundException(categogyName);
			}
		}

		public StatPoint[] Collect()
		{
			if (_category == null)
				return new StatPoint[0];
			using (var counter = new PerformanceCounter(_category.CategoryName, _counterName))
			{
				return new StatPoint[]
				{
					new StatPoint
					{
						NodeRef = _nodeRefCreator.CreateFromInstanceName(null),
						StatType = _statType,
						Time = DateTimeOffset.UtcNow,
						Value = counter.NextValue(),
					}
				};
			}
		}
	}
}