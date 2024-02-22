using System.Text.Json;
using System.Text;

namespace RsiApiWrapper
{
	public class SpreadsheetConnector
	{
		public struct Player
		{
			public string Name;
			public Guid Id;
			public bool SignedIn;
		}

		//public static async Task<Player[]> Players()
		//{
		//	using var httpClient = new HttpClient();
		//	const string url = "https://script.google.com/macros/s/AKfycbxq8ZQK90D2iHHaWOcZSTERSdI6O0gaJEH8-1Y0w4lCDRQcsgx5zMuf2ZxvRZy9JOnd/exec?endpoint=checkinAttendancesStatus";
		//	var response = await httpClient.GetAsync(url);
		//	if (response.StatusCode != System.Net.HttpStatusCode.OK)
		//		return Array.Empty<Player>();
		//	var payload = await response.Content.ReadAsStringAsync();
		//	return JsonSerializer.Deserialize<Player[]>(payload);
		//}

		private static readonly Task<string> URL = Task.Run(async () =>
		{
			const string initialUrl = "https://script.google.com/macros/s/AKfycbyGCn4r_ObZO0f_UoaGSa6slm8lH8dz2RJTn9bOIo-QaXNjOfWDgebV5JghQ9eyiELd/exec";
			using var httpClient = new HttpClient();
			var response = await httpClient.GetAsync(initialUrl);
			return await response.Content.ReadAsStringAsync();
		});
		

		public static async Task<bool> CheckinPlayers(IEnumerable<Guid> ids)
		{
			var payload = JsonSerializer.Serialize(ids);
			using var httpClient = new HttpClient();
			string url = await URL + "?endpoint=checkin";
			var response = await httpClient.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
			return response.StatusCode == System.Net.HttpStatusCode.OK
				&& (await response.Content.ReadAsStringAsync()) == "1";
		}

		public static async Task<bool> CheckoutPlayers(IEnumerable<Guid> ids)
		{
			var payload = JsonSerializer.Serialize(ids);
			using var httpClient = new HttpClient();
			string url = await URL + "?endpoint=checkout";
			var response = await httpClient.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
			return response.StatusCode == System.Net.HttpStatusCode.OK
				&& (await response.Content.ReadAsStringAsync()) == "1";
		}
	}
}
