using System;
using System.Threading;

namespace Tools.Extensions
{
	public static class CancellationTokenSourceExtensions
	{
		public static void CancelAndDispose(this CancellationTokenSource cts)
		{
			if (cts == null) return;

			try
			{
				cts.Cancel();
				cts.Dispose();
			}
			catch (ObjectDisposedException) { }
		}
	}
}