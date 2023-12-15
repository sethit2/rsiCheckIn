using BarcodeScanning;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using CommunityToolkit.Maui.Alerts;
using System.Text.Json;
using CommunityToolkit.Maui.Core;

namespace rsiCheckIn
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}
		public Guid ParseQR(BarcodeResult result)
		{
			if (Guid.TryParse(result.DisplayValue, out Guid guid))
				return guid;
			else
				return Guid.Empty;
		}

		protected override async void OnAppearing()
		{

			await Methods.AskForRequiredPermissionAsync();
			base.OnAppearing();

			Barcode.CameraEnabled = true;

			await Snackbar.Make("Hello world!", duration: TimeSpan.FromSeconds(3), anchor: Barcode).Show();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Barcode.CameraEnabled = false;
		}

		bool screenWait = false;
		readonly object lockObj = new();
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

			//List<Guid> guids = e.BarcodeResults.Select(ParseQR).Where(x => x != Guid.Empty).ToList();
			var snackbarOptions = new SnackbarOptions
			{
				BackgroundColor = new Color(0x5c, 0x85, 0x5c),
			};
			if (e.BarcodeResults.Count > 0)
			{
				Vibration.Vibrate();
				await Snackbar.Make(JsonSerializer.Serialize(e.BarcodeResults.Select(x => x.DisplayValue)), duration: TimeSpan.FromSeconds(3), visualOptions: snackbarOptions).Show();
			}

			lock (lockObj)
			{
				screenWait = false;
			}

			//if (guids.Count > 0)
			//{
			//	await SpreadsheetInteraction.SpreadsheetConnector.SigninPlayers(guids);
			//}
		}
	}
}
