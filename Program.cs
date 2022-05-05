using System;

namespace VeinEngine
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{
			using (var game = new GameManager())
				game.Run();
		}
	}
}
