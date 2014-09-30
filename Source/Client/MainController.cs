using System;
using System.Net;
using System.IO;

namespace Budgeteer
{
	public class MainController
	{
		public MainController ()
		{
		}

		/// <summary>
		/// Called from each platform specific UI when a picture of a receipt has been taken
		/// </summary>
		/// <param name="imageStream">Stream containing the picture of the receipt</param>
		public void OnPictureTaken(Stream imageStream)
		{
			Ocr.BeginGetOcrRequest (imageStream);
		}


	}
}

