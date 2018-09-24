using System;
using System.Diagnostics;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;

namespace SkiaDemo1
{
	public class SolarSystemPage : ContentPage
	{
		private SKCanvasView _canvasV = null;
		private SKBitmap _bitmap = null;
		private int _rotationAngle = 0;
        private float _pulsateScale = 1f;
        private int _revolutionAngle = 0;

        public SolarSystemPage()
		{
            Title = "Solar System";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
            Content = _canvasV;
			//Load assets
			using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().GetTypeInfo().Assembly, "earth.png"))) {
				_bitmap = SKBitmap.Decode(stream);
			}
		}

        private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            using (SKPaint p = new SKPaint { Color = SKColors.Yellow, Style = SKPaintStyle.Fill }) {
                canvas.DrawCircle(info.Width / 2, info.Height / 2, 50, p);
            }
            using(SKPaint p = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 2 }) {
                canvas.Translate(info.Width / 2, info.Height / 2);
                canvas.RotateDegrees(_revolutionAngle);
                canvas.Translate(0, -info.Width / 3);
                canvas.Scale(_pulsateScale);
                canvas.RotateDegrees(_rotationAngle);
                canvas.Translate(-75, -75);
                SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
                SKRect aspectRect = SKRect.Create(150, 150).AspectFit(imgSize);
                canvas.DrawBitmap(_bitmap, aspectRect);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            new Animation((value) => _revolutionAngle = (int)(value * 360)).Commit(this, "revolveAnimation", length: 10000, repeat: () => true);
            new Animation((value) => _pulsateScale = 0.5f * (float)Math.Sin(value) + 1, 0, Math.PI * 2).Commit(this, "pulsateAnimation", length: 1000, repeat: () => true);
            new Animation((value) => {
                _rotationAngle = (int)(value * 360);
                _canvasV.InvalidateSurface();
            }).Commit(this, "rotateAnimation", length: 1000, repeat: () => true);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.AbortAnimation("revolveAnimation");
            this.AbortAnimation("pulsateAnimation");
            this.AbortAnimation("rotateAnimation");
        }
	}
}
