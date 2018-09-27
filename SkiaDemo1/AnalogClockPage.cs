using System;
using System.Diagnostics;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;

namespace SkiaDemo1
{
	public class AnalogClockPage : ContentPage
	{
		private SKCanvasView _canvasV = null;
        private bool _pageIsActive = false;

        public AnalogClockPage()
		{
            Title = "Analog Clock";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
            Content = _canvasV;
		}

        private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            using (SKPaint sp = new SKPaint { Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round }) {
                using (SKPaint fp = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Gray }) {
                    // Transform for 100-radius circle centered at origin
                    canvas.Translate(info.Width / 2f, info.Height / 2f);
                    canvas.Scale(Math.Min(info.Width / 200f, info.Height / 200f));

                    // Hour and minute marks
                    for (int angle = 0; angle < 360; angle += 6) {
                        canvas.DrawCircle(0, -90, angle % 30 == 0 ? 4 : 2, fp);
                        canvas.RotateDegrees(6);
                    }

                    DateTime nowDT = DateTime.Now;

                    // Hour hand
                    sp.StrokeWidth = 15;
                    sp.Color = SKColors.Black;
                    canvas.Save();
                    canvas.RotateDegrees(30 * nowDT.Hour + nowDT.Minute / 2f);
                    canvas.DrawLine(0, 0, 0, -50, sp);
                    canvas.Restore();

                    // Minute hand
                    sp.StrokeWidth = 5;
                    sp.Color = SKColors.Black;
                    using (new SKAutoCanvasRestore(canvas)) {
                        canvas.RotateDegrees(6 * nowDT.Minute + nowDT.Second / 10f);
                        canvas.DrawLine(0, 0, 0, -70, sp);
                    }

                    // Second hand
                    sp.StrokeWidth = 2;
                    sp.Color = SKColors.Red;
                    using (new SKAutoCanvasRestore(canvas)) {
                        canvas.RotateDegrees(6 * (nowDT.Second + nowDT.Millisecond / 1000f));
                        canvas.DrawLine(0, 10, 0, -80, sp);
                    }
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _pageIsActive = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(20), () => {
                _canvasV.InvalidateSurface();
                return(_pageIsActive);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _pageIsActive = false;
        }
	}
}
