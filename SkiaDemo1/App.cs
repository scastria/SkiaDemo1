using System;

using Xamarin.Forms;

namespace SkiaDemo1
{
	public class App : Application
	{
		public App()
		{
			MainPage = new NavigationPage(new HomePage());
		}
	}
}
