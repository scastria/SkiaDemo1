using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class RectanglePage : ContentPage
	{
		public RectanglePage()
		{
			Title = "Rectangle";
			SKCanvasView canvasV = new SKCanvasView();
			canvasV.PaintSurface += HandlePaintCanvas;
			Content = canvasV;
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKSizeI canvasSize = e.Info.Size;
			e.Surface.Canvas.DrawColor(SKColors.LightGreen);
			using (SKPaint p = new SKPaint()) {
				p.Color = SKColors.Blue;
				p.IsAntialias = true;
				p.Style = SKPaintStyle.Fill;
				e.Surface.Canvas.DrawRect(new SKRect(canvasSize.Width / 3, canvasSize.Height / 3, canvasSize.Width * 2 / 3, canvasSize.Height * 2 / 3),p);
			}
		}
	}
}
