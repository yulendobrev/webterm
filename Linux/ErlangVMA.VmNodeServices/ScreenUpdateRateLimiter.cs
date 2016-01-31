using System;
using System.Linq;
using System.Threading;
using ErlangVMA.TerminalEmulation;

namespace ErlangVMA.VmNodeServices
{
	public class ScreenUpdateRateLimiter
	{
		private const int sendDelay = 40;

		private readonly Action<ScreenUpdate> onScreenUpdate;
		private readonly object locker = new object();

		private ScreenUpdate pendingScreenUpdate;
		private Timer sendTimer;
		private bool isUpdateReceivedSinceLastSend;
		private bool isIdle;

		public ScreenUpdateRateLimiter(Action<ScreenUpdate> onScreenUpdate)
		{
			this.onScreenUpdate = onScreenUpdate ?? new Action<ScreenUpdate>(u => { });
			this.pendingScreenUpdate = new ScreenUpdate();
			this.sendTimer = new Timer(SendPendingUpdates);
			this.isIdle = true;
		}

		public void AddScreenUpdate(ScreenData screenData)
		{
			lock (locker)
			{
				pendingScreenUpdate.CursorPosition = screenData.CursorPosition;

				if (isIdle)
				{
					pendingScreenUpdate.DisplayUpdates.Add(screenData);
					SendUpdates();
				}
				else
				{
					if (screenData.Width == 80 && screenData.Height == 24)
					{
						pendingScreenUpdate.DisplayUpdates.Clear();
					}
					if (!TryAggregateWithLastUpdate(screenData))
					{
						pendingScreenUpdate.DisplayUpdates.Add(screenData);
					}

					isUpdateReceivedSinceLastSend = true;
				}
			}
		}
			
		private void SendPendingUpdates(object ignored)
		{
			lock (locker)
			{
				if (isUpdateReceivedSinceLastSend)
				{
					SendUpdates();
				}
				else
				{
					isIdle = true;
				}
			}
		}

		private void SendUpdates()
		{
			onScreenUpdate(pendingScreenUpdate);

			isUpdateReceivedSinceLastSend = false;
			isIdle = false;

			pendingScreenUpdate = new ScreenUpdate();

			sendTimer.Change(sendDelay, -1);
		}

		private bool TryAggregateWithLastUpdate(ScreenData screenData)
		{
			var lastUpdate = pendingScreenUpdate.DisplayUpdates.LastOrDefault();
			if (lastUpdate == null)
			{
				return false;
			}

			if (screenData.Y == lastUpdate.Y && screenData.Height == lastUpdate.Height)
			{
				if (screenData.X == lastUpdate.X + lastUpdate.Width)
				{
					lastUpdate.Width += screenData.Width;

					var originalData = lastUpdate.Data;

					lastUpdate.Data = new TerminalScreenCharacter[lastUpdate.Width * lastUpdate.Height];
					for (int i = 0; i < lastUpdate.Height; ++i)
					{
						Array.Copy(originalData, i * (lastUpdate.Width - screenData.Width), lastUpdate.Data, i * lastUpdate.Width, lastUpdate.Width - screenData.Width);
						Array.Copy(screenData.Data, i * screenData.Width, lastUpdate.Data, (i + 1) * lastUpdate.Width - screenData.Width, screenData.Width);
					}
					return true;
				}
				if (lastUpdate.X == screenData.X + screenData.Width)
				{
//					lastUpdate.X -= screenData.Width;
//					for (int i = 0; i < lastUpdate.Height; ++i)
//					{
//						lastUpdate.Data[i] = screenData.Data[i].Concat(lastUpdate.Data[i]).ToArray();
//					}
//					return true;
				}
			}

			return false;
		}
	}
}
