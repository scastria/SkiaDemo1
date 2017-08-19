using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class PrimitivesPage : ContentPage
	{
		public PrimitivesPage()
		{
			Title = "Primitives";
			SKCanvasView canvasV = new SKCanvasView();
			canvasV.PaintSurface += HandlePaintCanvas;
			Content = canvasV;
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKSizeI canvasSize = e.Info.Size;
			e.Surface.Canvas.DrawColor(SKColors.Yellow);
			using (SKPaint p = new SKPaint()) {
				p.Color = SKColors.Blue;
				p.IsAntialias = true;
				p.Style = SKPaintStyle.Fill;
				e.Surface.Canvas.DrawCircle(100, 100, 75, p);
				p.Color = SKColors.Brown;
				using (SKPath path = new SKPath()) {
					path.MoveTo(200, 200);
					path.RLineTo(50, -25);
					path.RLineTo(0, -50);
					path.RLineTo(25, -25);
					path.RLineTo(-50, 25);
					path.Close();
					e.Surface.Canvas.DrawPath(path, p);
				}
				p.Color = SKColors.Purple;
				p.TextSize = 60;
				e.Surface.Canvas.DrawText("Hello Text", 300, 300, p);
				p.Color = SKColors.Green;
				p.TextSize = 30;
				using (SKPath path = new SKPath()) {
					path.MoveTo(300, 500);
					path.CubicTo(366, 500, 400, 533, 400, 600);
					path.CubicTo(400, 666, 433, 700, 500, 700);
					p.Style = SKPaintStyle.Stroke;
					p.StrokeWidth = 2;
					e.Surface.Canvas.DrawPath(path, p);
					p.Style = SKPaintStyle.Fill;
					e.Surface.Canvas.DrawTextOnPath("Follow Path Along Curve", path, 0, 0, p);
				}
			}
		}
	}
}
