using System;
using System.Collections;
using System.Xml;

namespace Holidays
{
	/// <summary>
	/// Summary description for HolidayCalculator.
	/// </summary>
	public class HolidayCalculator
	{
		#region Constructor
		/// <summary>
		/// Returns all of the holidays occuring in the year following the date that is passed in the constructor.  Holidays are defined in an XML file.
		/// </summary>	
		/// <param name="startDate">The starting date for returning holidays.  All holidays for one year after this date are returned.</param>
		/// <param name="xmlPath">The path to the XML file that contains the holiday definitions.</param>
		public HolidayCalculator(DateTime startDate, string xmlPath)
		{
			_startingDate = startDate;
			_orderedHolidays = new ArrayList();
			_xHolidays = new XmlDocument();
			_xHolidays.Load(xmlPath);
			ProcessXML();
		}
		#endregion

		#region Private Properties
		private readonly ArrayList _orderedHolidays;
		private readonly XmlDocument _xHolidays;
		private DateTime _startingDate;
		#endregion

		#region Public Properties

		/// <summary>
		/// The holidays occuring after StartDate listed in chronological order;
		/// </summary>
		public ArrayList OrderedHolidays 
		{
			get { return _orderedHolidays; }
		}
		#endregion

		#region Private Methods


		/// <summary>
		/// Loops through the holidays defined in the XML configuration file, and adds the next occurance into the OrderHolidays collection if it occurs within one year.
		/// </summary>
		private void ProcessXML()
		{
		    var holidaysNodes = _xHolidays.SelectNodes("/Holidays/Holiday");
		    if (holidaysNodes != null)
		        foreach (XmlNode n in holidaysNodes)
		        {
		            Holiday h = ProcessNode(n);
		            if (h.Date.Year > 1)
		                _orderedHolidays.Add(h);
		        }
		    _orderedHolidays.Sort();
		}


	    /// <summary>
		/// Processes a Holiday node from the XML configuration file.
		/// </summary>
		/// <param name="n">The Holdiay node to process.</param>
		/// <returns></returns>
		private Holiday ProcessNode(XmlNode n)
		{
			Holiday h = new Holiday();
			h.Name = n.Attributes["name"].Value;
			ArrayList childNodes = new ArrayList();
			foreach (XmlNode o in n.ChildNodes)
			{
				childNodes.Add(o.Name);
			}
			if (childNodes.Contains("WeekOfMonth"))
			{
				int m = Int32.Parse(n.SelectSingleNode("./Month").InnerXml);
				int w = Int32.Parse(n.SelectSingleNode("./WeekOfMonth").InnerXml);
				int wd = Int32.Parse(n.SelectSingleNode("./DayOfWeek").InnerXml);
				h.Date = GetDateByMonthWeekWeekday(m,w,wd,_startingDate);
			}
			else if (childNodes.Contains("DayOfWeekOnOrAfter"))
			{
				int dow = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrAfter/DayOfWeek").InnerXml);
				if (dow > 6 || dow < 0)
					throw new Exception("DOW is greater than 6");
				int m = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrAfter/Month").InnerXml);
				int d = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrAfter/Day").InnerXml);
				h.Date = GetDateByWeekdayOnOrAfter(dow,m,d, _startingDate);
			}
			else if (childNodes.Contains("WeekdayOnOrAfter"))
			{
				int m = Int32.Parse(n.SelectSingleNode("./WeekdayOnOrAfter/Month").InnerXml);
				int d = Int32.Parse(n.SelectSingleNode("./WeekdayOnOrAfter/Day").InnerXml);
				DateTime dt = new DateTime(_startingDate.Year, m, d);
				if (dt < _startingDate)
					dt = dt.AddYears(1);
				while(dt.DayOfWeek.Equals(DayOfWeek.Saturday) || dt.DayOfWeek.Equals(DayOfWeek.Sunday))
				{
					dt = dt.AddDays(1);
				}
				h.Date =dt;
			}
			else if (childNodes.Contains("LastFullWeekOfMonth"))
			{
				int m = Int32.Parse(n.SelectSingleNode("./LastFullWeekOfMonth/Month").InnerXml);
				int weekday = Int32.Parse(n.SelectSingleNode("./LastFullWeekOfMonth/DayOfWeek").InnerXml);
				DateTime dt = GetDateByMonthWeekWeekday(m,5,weekday, _startingDate);

				if (dt.AddDays(6-weekday).Month == m)
					h.Date = dt;
				else
					h.Date = dt.AddDays(-7);
			}
			else if (childNodes.Contains("DaysAfterHoliday"))
			{
				XmlNode basis = _xHolidays.SelectSingleNode("/Holidays/Holiday[@name='" + n.SelectSingleNode("./DaysAfterHoliday").Attributes["Holiday"].Value + "']");
				Holiday bHoliday = ProcessNode(basis);
				int days = Int32.Parse(n.SelectSingleNode("./DaysAfterHoliday/Days").InnerXml);
				h.Date = bHoliday.Date.AddDays(days);
			}
			else if (childNodes.Contains("Easter"))
			{
				h.Date = Easter();
			}
			else
			{
				if (childNodes.Contains("Month") && childNodes.Contains("Day"))
				{
					int m = Int32.Parse(n.SelectSingleNode("./Month").InnerXml);
					int d = Int32.Parse(n.SelectSingleNode("./Day").InnerXml);
					DateTime dt = new DateTime(_startingDate.Year, m, d);
					if (dt < _startingDate)
					{
						dt = dt.AddYears(1);
					}
					if (childNodes.Contains("EveryXYears"))
					{
						int yearMult = Int32.Parse(n.SelectSingleNode("./EveryXYears").InnerXml);
						int startYear = Int32.Parse(n.SelectSingleNode("./StartYear").InnerXml);
						if (((dt.Year - startYear) % yearMult) == 0)
						{
							h.Date = dt;
						}
					}
					else
					{
						h.Date = dt;
					}
				}
			}
			return h;
		}


		/// <summary>
		/// Determines the next occurance of Easter (western Christian).
		/// </summary>
		/// <returns></returns>
		private DateTime Easter()
		{
			DateTime workDate = getFirstDayOfMonth(_startingDate);
			int y = workDate.Year;
			if (workDate.Month > 4)
				y = y+1;
			return Easter(y);
		}


		/// <summary>
		/// Determines the occurance of Easter in the given year.  If the result comes before StartDate, recalculates for the following year.
		/// </summary>
		/// <param name="y"></param>
		/// <returns></returns>
		private DateTime Easter(int y)
		{
			int a=y%19;
			int b=y/100;
			int c=y%100;
			int d=b/4;
			int e=b%4;
			int f=(b+8)/25;
			int g=(b-f+1)/3;
			int h=(19*a+b-d-g+15)%30;
			int i=c/4;
			int k=c%4;
			int l=(32+2*e+2*i-h-k)%7;
			int m=(a+11*h+22*l)/451;
			int easterMonth =(h+l-7*m+114)/31;
			int  p=(h+l-7*m+114)%31;
			int easterDay=p+1;
			DateTime est = new DateTime(y,easterMonth,easterDay);
			if (est < _startingDate)
				return Easter(y+1);
			else
				return new DateTime(y,easterMonth,easterDay);
		}

	    /// <summary>
	    /// Gets the next occurance of a weekday after a given month and day in the year after StartDate.
	    /// </summary>
	    /// <param name="weekday">The day of the week (0=Sunday).</param>
	    /// <param name="m">The Month</param>
	    /// <param name="d">Day</param>
	    /// <param name="startDate"></param>
	    /// <returns></returns>
	    private DateTime GetDateByWeekdayOnOrAfter(int weekday, int m, int d, DateTime startDate)
		{
			DateTime workDate = getFirstDayOfMonth(startDate);
			while (workDate.Month != m)
			{
				workDate = workDate.AddMonths(1);
			}
			workDate = workDate.AddDays(d-1);

			while (weekday != (int)(workDate.DayOfWeek))
			{
				workDate = workDate.AddDays(1);
			}

			//It's possible the resulting date is before the specified starting date.  If so we'll calculate again for the next year.
			if (workDate < _startingDate)
				return GetDateByWeekdayOnOrAfter(weekday,m,d,startDate.AddYears(1));
			else
				return workDate;				
		}

	    /// <summary>
	    /// Gets the n'th instance of a day-of-week in the given month after StartDate
	    /// </summary>
	    /// <param name="month">The month the Holiday falls on.</param>
	    /// <param name="week">The instance of weekday that the Holiday falls on (5=last instance in the month).</param>
	    /// <param name="weekday">The day of the week that the Holiday falls on.</param>
	    /// <param name="startDate"></param>
	    /// <returns></returns>
	    private DateTime GetDateByMonthWeekWeekday(int month, int week, int weekday, DateTime startDate)
		{
			DateTime workDate = getFirstDayOfMonth(startDate);
			while (workDate.Month != month)
			{
				workDate = workDate.AddMonths(1);
			}
			while ((int)workDate.DayOfWeek != weekday)
			{
				workDate = workDate.AddDays(1);
			}

			DateTime result;
			if (week == 1)
			{
				result =  workDate;
			}
			else
			{
				int addDays = (week*7)-7;
				int day = workDate.Day + addDays;
				if (day > DateTime.DaysInMonth(workDate.Year, workDate.Month))
				{
					day = day-7;
				}
				result = new  DateTime(workDate.Year,workDate.Month,day);
			}

			//It's possible the resulting date is before the specified starting date.  If so we'll calculate again for the next year.
			if (result >= _startingDate)
				return result;
			else
				return GetDateByMonthWeekWeekday(month,week,weekday,startDate.AddYears(1));
		}

		/// <summary>
		/// Returns the first day of the month for the specified date.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private DateTime getFirstDayOfMonth(DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, 1);
		}
		#endregion

		#region Holiday Object
		public class Holiday : IComparable
		{
			public DateTime Date;
			public string Name;

			#region IComparable Members

			public int CompareTo(object obj)
			{
			    var h = obj as Holiday;
			    if (h != null)
			    {
			        return Date.CompareTo(h.Date);
			    }
				throw new ArgumentException("Object is not a Holiday"); 
			}
			#endregion
		}
		#endregion
	}
}
