using System;

namespace HolidayCalculator
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	static class HolidayCalculatorDemonstrator
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Console.Write("\nPlease enter a date (mm/dd/yyyy):  ");
			string dateString = Console.ReadLine();
		    try
			{
				DateTime date = DateTime.Parse(dateString);
				Holidays.HolidayCalculator hc = new Holidays.HolidayCalculator(date,@"../../Holidays.xml");
				Console.WriteLine("\nHere are holidays for the 12 months following " + date.ToString("D") + ":");
				foreach (Holidays.HolidayCalculator.Holiday h in hc.OrderedHolidays)
				{
					Console.WriteLine(h.Name + " - " + h.Date.ToString("D"));
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		    
            // wait for user to hit enter
            Console.WriteLine("\n\nPress enter to continue...");
            Console.ReadLine();
		}
	}
}
