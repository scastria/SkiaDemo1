using System;
using System.Diagnostics;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;

namespace SkiaDemo1
{
	public class ImageInteractionPage : ContentPage
	{
		private SKCanvasView _canvasV = null;
		private SKMatrix _m = SKMatrix.MakeIdentity();
		private SKBitmap _bitmap = null;
        private SKPoint _lastPanPt = SKPoint.Empty;

		public ImageInteractionPage()
		{
            Title = "Image Interaction";
            ToolbarItem resetTBI = new ToolbarItem {
                Text = "Reset"
            };
            resetTBI.Clicked += HandleResetClicked;
            ToolbarItems.Add(resetTBI);
            _canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
			Content = _canvasV;
			//Load assets
			using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().GetTypeInfo().Assembly, "landscape.jpg"))) {
				_bitmap = SKBitmap.Decode(stream);
			}
            //Interaction
            PanGestureRecognizer pgr = new PanGestureRecognizer();
            pgr.PanUpdated += HandlePan;
            _canvasV.GestureRecognizers.Add(pgr);
            PinchGestureRecognizer pngr = new PinchGestureRecognizer();
            pngr.PinchUpdated += HandlePinch;
            _canvasV.GestureRecognizers.Add(pngr);
        }

        private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            SKRect bitmapRect = CalculateBitmapAspectRect(_bitmap);
            canvas.SetMatrix(_m);
            canvas.DrawBitmap(_bitmap, bitmapRect);
        }

        private SKRect CalculateBitmapAspectRect(SKBitmap bitmap)
        {
            if (bitmap == null)
                return (SKRect.Empty);
            SKSize imgSize = new SKSize(bitmap.Width, bitmap.Height);
            return (SKRect.Create(_canvasV.CanvasSize.Width, _canvasV.CanvasSize.Height).AspectFit(imgSize));
        }

        private void HandleResetClicked(object sender, EventArgs e)
        {
            _m = SKMatrix.MakeIdentity();
            _canvasV.InvalidateSurface();
        }

        private void HandlePan(object sender, PanUpdatedEventArgs e)
        {
            SKPoint panPt = ToUntransformedCanvasPt((float)e.TotalX, (float)e.TotalY);
            switch (e.StatusType) {
            case GestureStatus.Started:
                _lastPanPt = panPt;
                break;
            case GestureStatus.Running:
                SKPoint deltaTran = panPt - _lastPanPt;
                _lastPanPt = panPt;
                SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTran.X, deltaTran.Y);
                SKMatrix.PostConcat(ref _m, deltaM);
                _canvasV.InvalidateSurface();
                break;
            }
        }

        private void HandlePinch(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status) {
            case GestureStatus.Running:
                SKPoint pivotPt = ToUntransformedCanvasPt((float)(e.ScaleOrigin.X * _canvasV.Width), (float)(e.ScaleOrigin.Y * _canvasV.Height));
                SKMatrix deltaM = SKMatrix.MakeScale((float)e.Scale, (float)e.Scale, pivotPt.X, pivotPt.Y);
                SKMatrix.PostConcat(ref _m, deltaM);
                _canvasV.InvalidateSurface();
                break;
            }
        }

        private SKPoint ToUntransformedCanvasPt(float x, float y)
        {
            return (new SKPoint(x * _canvasV.CanvasSize.Width / (float)_canvasV.Width, y * _canvasV.CanvasSize.Height / (float)_canvasV.Height));
        }
    }
}
