using System;

namespace JayMuntzCom
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class HolidayCalculatorDemonstrator
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.Write("\nPlease enter a date (mm/dd/yyyy):  ");
			string dateString = Console.ReadLine();
			DateTime date;
			try
			{
				date = DateTime.Parse(dateString);
				HolidayCalculator hc = new HolidayCalculator(date,@"../../Holidays.xml");
				Console.WriteLine("\nHere are holidays for the 12 months following " + date.ToString("D") + ":");
				foreach (HolidayCalculator.Holiday h in hc.OrderedHolidays)
				{
					Console.WriteLine(h.Name + " - " + h.Date.ToString("D"));
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
