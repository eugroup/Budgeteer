using System;
using System.Net;
using System.IO;

namespace Budgeteer
{
	/// <summary>
	/// Handles all communication with the server
	/// </summary>
	public class Ocr
	{
		/// <summary>
		/// Event that fires when data has been received from the server
		/// </summary>
		public static event EventHandler<string> ReceivedOcrData;

		/// <summary>
		/// Begins getting the request stream for the server. This is used for uploading the image
		/// </summary>
		/// <param name="imageStream">Image stream.</param>
		public static void BeginGetOcrRequest(Stream imageStream)
		{
			//TODO: make central data store for urls like this one
			Uri uri = new Uri ("http://api.budgeteer.devbase.biz/", UriKind.Absolute);
			HttpWebRequest request = HttpWebRequest.CreateHttp (uri);
			request.Method = "POST";

			request.BeginGetRequestStream (new AsyncCallback(EndGetOcrRequest), new Tuple<HttpWebRequest, Stream>(request, imageStream));
		}

		/// <summary>
		/// Ends getting the request stream for the server and uploads the image
		/// 
		/// This function is only called by BeginGetRequestStream
		/// </summary>
		/// <param name="result">Result.</param>
		static async void EndGetOcrRequest(IAsyncResult result)
		{
			Stream imageStream = ((Tuple<HttpWebRequest, Stream>)result.AsyncState).Item2;
			HttpWebRequest request = ((HttpWebRequest)((Tuple<HttpWebRequest, Stream>)result.AsyncState).Item1);
			Stream requestStream = request.EndGetRequestStream (result);
			await imageStream.CopyToAsync (requestStream);

			request.BeginGetResponse (new AsyncCallback (EndGetOcrResponse), request);
		}

		/// <summary>
		/// Gets the response from the server
		/// 
		/// This function is only called by BeginGetRequestStream
		/// </summary>
		/// <param name="result">Result.</param>
		static async void EndGetOcrResponse(IAsyncResult result)
		{
			HttpWebRequest request = (HttpWebRequest)result.AsyncState;
			HttpWebResponse response = (HttpWebResponse)request.EndGetResponse (result);

			Stream responseStream = response.GetResponseStream ();
			string ocrText = await new StreamReader (responseStream).ReadToEndAsync ();

			//Fire event that notifies the rest of the app that data has been received from the server
			if (ReceivedOcrData != null)
				ReceivedOcrData (null, ocrText);
		}
	}
}

