using System;
using System.Windows.Forms;

namespace CypressLauncher;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new LaunchWindow());
	}
}
