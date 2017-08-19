using System;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;

namespace SkiaDemo1
{
	public class ImagePage : ContentPage
	{
		public ImagePage()
		{
			Title = "Image";
			SKCanvasView canvasV = new SKCanvasView();
			canvasV.PaintSurface += HandlePaintCanvas;
			Content = canvasV;
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			e.Surface.Canvas.Clear();
			e.Surface.Canvas.SetMatrix(SKMatrix.MakeRotationDegrees(90, e.Info.Width / 2, e.Info.Height / 2));
			using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().GetTypeInfo().Assembly, "landscape.jpg")))
			using (var bitmap = SKBitmap.Decode(stream))
			using (var paint = new SKPaint()) {
				SKSize imgSize = new SKSize(bitmap.Width, bitmap.Height);
				SKRect aspectRect = SKRect.Create(e.Info.Width, e.Info.Height).AspectFit(imgSize);
				e.Surface.Canvas.DrawBitmap(bitmap, aspectRect, paint);
			}
		}
	}
}
