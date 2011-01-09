using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoogleDataApiExample
{
	class Program
	{
		static void Main(string[] args)
		{
			//Get an authorisation token
			var authToken = "";
			const string authenticationUrl = "https://www.google.com/accounts/ClientLogin";

			var postData = "Email=" + HttpUtility.UrlEncode() + "&";
			postData += "Passwd=" + HttpUtility.UrlEncode() + "&";
			postData += "source=GoogleDataApiExample-Console-App&";
			postData += "service=analytics&";
			postData += "accountType=HOSTED_OR_GOOGLE";

			var encoding = new UTF8Encoding();
			var postByteArray = encoding.GetBytes(postData);

			var webRequest = WebRequest.Create(authenticationUrl);
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Method = "POST";
			webRequest.ContentLength = postData.Length;

			var requestStream = webRequest.GetRequestStream();
			requestStream.Write(postByteArray, 0, postData.Length);
			requestStream.Close();

			try
			{
				using (var authResponse = webRequest.GetResponse())
				{
					using (var authResponseStream = new StreamReader(authResponse.GetResponseStream(), encoding))
					{
						var responseText = authResponseStream.ReadToEnd();
						var lines = responseText.Split(new[] { '\n' }); //get each line as an array element
						var authLine = lines.First(l => l.StartsWith("Auth")); //find the line with the auth token
						authToken = authLine.Remove(0, 5); //remove the key and equals sign "key="
						Console.WriteLine("auth token is " + authToken);
					}
				}
			}
			catch (WebException e)
			{
				Console.WriteLine("Error:" + e.Message);
				Console.WriteLine("Note: You must have at least 1 Google Website Optimiser experiment.");
				Console.ReadLine();
				return;
			}

			//Get JSON
			var url = "https://www.google.com/analytics/feeds/websiteoptimizer/experiments?alt=json";

			var request = (HttpWebRequest)WebRequest.Create(url);

			request.Headers.Add("GData-Version: 2.0");
			request.Headers.Add("Authorization: GoogleLogin auth=" + authToken);	//auth token required here

			using (var authResponse = request.GetResponse())
			{
				using (var responseStream = new StreamReader(authResponse.GetResponseStream(), Encoding.UTF8))
				{
					var responseText = responseStream.ReadToEnd();

					var jsonObject = JObject.Parse(responseText);
					var jsonEntrySection = jsonObject["feed"]["entry"];	//only want the entry part of the JSON
					var experiments = JsonConvert.DeserializeObject<ExperimentDto[]>(jsonEntrySection.ToString());

					//Write some info to teh console
					Console.WriteLine("Numbe of experiments:" + experiments.Length);
					foreach (var experiment in experiments)
					{
						Console.WriteLine(experiment.Id.Text + " - " + experiment.Title.Text);
					}
				}
			}
			Console.WriteLine("Done. Hit return to exit.");
			Console.ReadLine();
		}

		public class ExperimentDto
		{
			[JsonProperty(PropertyName = "gwo$experimentId")]	//Json property name is illegal C# property name so changed to Id
			public GoogleText Id { get; set; }

			public GoogleText Title { get; set; }

			public class GoogleText
			{
				[JsonProperty(PropertyName = "$t")]
				public string Text { get; set; }
			}
		}
	}
}
