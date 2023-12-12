using BarcodeScanning;
using System.Text;
using System.Text.RegularExpressions;

namespace rsiCheckIn
{
	public partial class MainPage : ContentPage
	{
		HashSet<Guid> guids = [
			Guid.Parse("f7cdf6e5-90a2-443d-971e-2725ebede9ec")
		];

		public MainPage()
		{
			InitializeComponent();
		}
		public (Guid guid, string name)? parseQR(BarcodeResult result)
		{
			string text = result.DisplayValue;
			var split = text.IndexOf('\n');
			if (split == -1)
				return null;
			if (text.Length == split + 1)
				return null;
			if (!Guid.TryParse(text.AsSpan(0, split), out Guid guid))
				return null;
			if (!guids.Contains(guid))
				return null;
			return (guid, text.Substring(split + 1));
		}

		protected override async void OnAppearing()
		{
			await Methods.AskForRequiredPermissionAsync();
			base.OnAppearing();

			Barcode.CameraEnabled = true;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Barcode.CameraEnabled = false;
		}

		bool screenWait = false;
		object lockObj = new object();
		private async void Barcode_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
		{
			if (screenWait)
				return;
			lock (lockObj)
			{
				if (screenWait)
					return;
				screenWait = true;
			}

			List<Guid> guids = new List<Guid>();

			foreach (var (guid, name) in e.BarcodeResults.Select(parseQR).Where(x => x.HasValue).Select(x => x.Value))
			{
				var popup = await DisplayAlert("Checking in", $"Sign in: {name}", "Yes", "No");
				if (popup)
				{
					guids.Add(guid);
				}
			}

			lock (lockObj)
			{
				screenWait = false;
			}

			if (guids.Count > 0)
			{
				await SpreadsheetInteraction.SpreadsheetConnector.SigninPlayers(guids);
			}
		}
	}
}
