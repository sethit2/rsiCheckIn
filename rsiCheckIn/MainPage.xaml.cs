using BarcodeScanning;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using CommunityToolkit.Maui.Alerts;
using System.Text.Json;
using CommunityToolkit.Maui.Core;
using rsiCheckIn.SpreadsheetInteraction;

namespace rsiCheckIn
{
	public partial class MainPage : ContentPage
	{
		//readonly object playersLock = new();
		//Dictionary<Guid, SpreadsheetConnector.Player> players;

		public MainPage()
		{
			InitializeComponent();
			//UpdatePlayerInfo();
			//Dispatcher.StartTimer(TimeSpan.FromSeconds(30), () =>
			//{
			//	UpdatePlayerInfo();
			//	return true;
			//});
		}

		//async void UpdatePlayerInfo()
		//{
		//	var p = await SpreadsheetConnector.Players();
		//	var p1 = p.ToDictionary(x => x.Id);
		//	lock (playersLock)
		//		players = p1;
		//}

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
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Barcode.CameraEnabled = false;
		}

		static readonly SnackbarOptions snackbarOptions = new()
		{
			BackgroundColor = new Color(0x5c, 0x85, 0x5c),
		};
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

			//var guids = e.BarcodeResults.Select(ParseQR).Where(x => x != Guid.Empty).Where(x => players.ContainsKey(x)).Select(x => players[x]).Where(x => !x.SignedIn);

			var guids = e.BarcodeResults.Select(ParseQR).Where(x => x != Guid.Empty);

			if (guids.Any())
			{
				Vibration.Vibrate();
			}

			lock (lockObj)
			{
				screenWait = false;
			}

			if (guids.Any())
			{
				await SpreadsheetConnector.SigninPlayers(guids);
			}
		}
	}
}
