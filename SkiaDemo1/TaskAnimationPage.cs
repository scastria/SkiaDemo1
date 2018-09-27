using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class TaskAnimationPage : ContentPage
	{
		private SKCanvasView _canvasV = null;
		private Stopwatch _stopwatch = new Stopwatch();
		private bool _pageIsActive = false;
		private float _scale = 0.5f;

		public TaskAnimationPage()
		{
			Title = "Task Animation";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
			Content = _canvasV;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_pageIsActive = true;
			AnimationLoop();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_pageIsActive = false;
		}

		private async Task AnimationLoop()
		{
			_stopwatch.Start();

			while (_pageIsActive) {
				double cycleTime = 3;
				double t = _stopwatch.Elapsed.TotalSeconds % cycleTime / cycleTime;
				_scale = (1 + (float)Math.Sin(2 * Math.PI * t)) / 2;
				_canvasV.InvalidateSurface();
				await Task.Delay(TimeSpan.FromSeconds(1.0 / 30));
			}

			_stopwatch.Stop();
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKImageInfo info = e.Info;
			SKSurface surface = e.Surface;
			SKCanvas canvas = surface.Canvas;

			canvas.Clear();

			float maxRadius = 0.75f * Math.Min(info.Width, info.Height) / 2;
			float minRadius = 0.25f * maxRadius;

			float xRadius = minRadius * _scale + maxRadius * (1 - _scale);
			float yRadius = maxRadius * _scale + minRadius * (1 - _scale);

            using (SKPaint p = new SKPaint { StrokeWidth = 30 }) {
				p.Style = SKPaintStyle.Fill;
				p.Color = SKColors.SkyBlue;
				canvas.DrawOval(info.Width / 2, info.Height / 2, xRadius, yRadius, p);

                p.Style = SKPaintStyle.Stroke;
                p.Color = SKColors.Blue;
                canvas.DrawOval(info.Width / 2, info.Height / 2, xRadius, yRadius, p);
            }
        }
	}
}
