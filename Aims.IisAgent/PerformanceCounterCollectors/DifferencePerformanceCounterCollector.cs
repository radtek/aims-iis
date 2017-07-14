﻿using System;
using System.Collections.Generic;
using System.Linq;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public class DifferencePerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly IBasePerformanceCounterCollector _basePerformanceCounterCollector;
		private Dictionary<NodeRef, StatPoint> _prewValues;

		public DifferencePerformanceCounterCollector(IBasePerformanceCounterCollector basePerformanceCounterCollector)
		{
			if (basePerformanceCounterCollector == null)
				throw new ArgumentNullException(nameof(basePerformanceCounterCollector));
			_basePerformanceCounterCollector = basePerformanceCounterCollector;
			_prewValues = new Dictionary<NodeRef, StatPoint>();
		}

		public StatPoint[] Collect()
		{
			Dictionary<NodeRef, StatPoint> newValues = CollectFromBase();
			var answer = MakeDifference(newValues, _prewValues);
			_prewValues = newValues;
			return answer
				.ToArray();
		}

		private static IEnumerable<StatPoint> MakeDifference(IDictionary<NodeRef, StatPoint> newValues, IDictionary<NodeRef, StatPoint> prewValues)
		{
			List<StatPoint> answer = new List<StatPoint>(newValues.Count);
			foreach (var keyValuePair in newValues)
			{
				StatPoint statPoint = new StatPoint
				{
					NodeRef = keyValuePair.Key,
					StatType = keyValuePair.Value.StatType,
					Time = keyValuePair.Value.Time,
					Value = keyValuePair.Value.Value,
				};
				StatPoint oldStatPoint;
				if (prewValues.TryGetValue(keyValuePair.Key, out oldStatPoint))
				{
					if (oldStatPoint.Value <= statPoint.Value)
						statPoint.Value -= oldStatPoint.Value;
				}
				else
				{
					statPoint.Value = 0;
				}

				answer.Add(statPoint);
			}
			return answer;
		}

		private Dictionary<NodeRef, StatPoint> CollectFromBase()
		{
			Dictionary<NodeRef, StatPoint> collectedValues = new Dictionary<NodeRef, StatPoint>();
			foreach (var statPoint in _basePerformanceCounterCollector.Collect())
			{
				if (statPoint.Value < 0.0)
					throw new ArgumentOutOfRangeException(statPoint.NodeRef.NodeType, statPoint.Value, string.Empty);
				try
				{
					collectedValues.Add(statPoint.NodeRef, statPoint);
				}
				catch (ArgumentException)
				{
					//fix multi instance
					collectedValues[statPoint.NodeRef].Value += statPoint.Value;
				}
			}
			return collectedValues;
		}
	}
}