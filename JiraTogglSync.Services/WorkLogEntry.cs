﻿using System;
using System.Collections.Generic;

namespace JiraTogglSync.Services
{
	public class WorkLogEntry
	{
		public string IssueKey { get; set; }
		public string Description { get; set; }
		public DateTime Start { get; set; }
		public DateTime Stop { get; set; }

		public TimeSpan RoundedDuration { get; set; }
	    public int? Id { get; set; }
	    public List<string> Tags { get; set; }

	    public override string ToString()
		{
			return string.Format("{0:d} - {1} - {2}", Start.Date, RoundedDuration, Description);
		}

		public void Round(int nbMinutes)
		{
			RoundedDuration = RoundToClosest(Stop - Start, new TimeSpan(0, 0, nbMinutes, 0));
		}

		private static TimeSpan RoundToClosest(TimeSpan input, TimeSpan precision)
		{
			if (input < TimeSpan.Zero)
			{
				return -RoundToClosest(-input, precision);
			}

			return new TimeSpan(((input.Ticks + precision.Ticks/2) / precision.Ticks) * precision.Ticks);
		}
	}
}