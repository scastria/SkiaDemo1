using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class SolidColorPage : ContentPage
	{
		public SolidColorPage()
		{
			Title = "Solid Color";
			SKCanvasView canvasV = new SKCanvasView();
			canvasV.PaintSurface += HandlePaintCanvas;
			Content = canvasV;
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			e.Surface.Canvas.DrawColor(SKColors.Red);
		}
	}
}
