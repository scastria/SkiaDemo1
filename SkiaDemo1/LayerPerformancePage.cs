﻿using System;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class LayerPerformancePage : ContentPage
	{
		private const int CELL_DIM = 15;

		private SKCanvasView _canvasV = null;
		private SKMatrix _m = SKMatrix.MakeIdentity();
		private SKMatrix _currentTransformM = SKMatrix.MakeIdentity();
		private SKMatrix _startPanM = SKMatrix.MakeIdentity();
		private SKMatrix _startPinchM = SKMatrix.MakeIdentity();
		private Point _startPinchAnchor = Point.Zero;
		private float _totalPinchScale = 1f;
		private float _screenScale;
		private SKBitmap _bitmap = null;
		private SKBitmap _textLayer = null;

		public LayerPerformancePage()
		{
#if __ANDROID__
            _screenScale = ((Android.App.Activity)Forms.Context).Resources.DisplayMetrics.Density;
#else
			_screenScale = (float)UIKit.UIScreen.MainScreen.Scale;
#endif
			Title = "Layer Performance";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
			Content = _canvasV;
			//Load assets
			using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().Assembly, "landscape.jpg"))) {
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

		private void HandlePan(object sender, PanUpdatedEventArgs puea)
		{
			Console.WriteLine(puea.StatusType + " (" + puea.TotalX + "," + puea.TotalY + ")");
			switch (puea.StatusType) {
			case GestureStatus.Started:
				_startPanM = _m;
				break;
			case GestureStatus.Running:
				float canvasTotalX = (float)puea.TotalX * _screenScale;
				float canvasTotalY = (float)puea.TotalY * _screenScale;
				SKMatrix canvasTranslation = SKMatrix.MakeTranslation(canvasTotalX, canvasTotalY);
				SKMatrix.Concat(ref _m, ref canvasTranslation, ref _startPanM);
				_currentTransformM = canvasTranslation;
				_canvasV.InvalidateSurface();
				break;
			default:
				_startPanM = SKMatrix.MakeIdentity();
				//Force textLayer to regenerate
				_textLayer?.Dispose();
				_textLayer = null;
				_canvasV.InvalidateSurface();
				break;
			}
		}

		private void HandlePinch(object sender, PinchGestureUpdatedEventArgs puea)
		{
			Console.WriteLine(puea.Status + " (" + puea.ScaleOrigin.X + "," + puea.ScaleOrigin.Y + ") " + puea.Scale);
			Point canvasAnchor = new Point(puea.ScaleOrigin.X * _canvasV.Width * _screenScale,
										   puea.ScaleOrigin.Y * _canvasV.Height * _screenScale);
			switch (puea.Status) {
			case GestureStatus.Started:
				_startPinchM = _m;
				_startPinchAnchor = canvasAnchor;
				_totalPinchScale = 1f;
				break;
			case GestureStatus.Running:
				_totalPinchScale *= (float)puea.Scale;
				SKMatrix canvasScaling = SKMatrix.MakeScale(_totalPinchScale, _totalPinchScale, (float)_startPinchAnchor.X, (float)_startPinchAnchor.Y);
				SKMatrix.Concat(ref _m, ref canvasScaling, ref _startPinchM);
				_currentTransformM = canvasScaling;
				_canvasV.InvalidateSurface();
				break;
			default:
				_startPinchM = SKMatrix.MakeIdentity();
				_startPinchAnchor = Point.Zero;
				_totalPinchScale = 1f;
				//Force textLayer to regenerate
				_textLayer?.Dispose();
				_textLayer = null;
				_canvasV.InvalidateSurface();
				break;
			}
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;
			SKImageInfo info = e.Info;
			canvas.Clear();
			//Background layer
			using (new SKAutoCanvasRestore(canvas)) {
				canvas.SetMatrix(_m);
				SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
				SKRect aspectRect = SKRect.Create(info.Width, info.Height).AspectFit(imgSize);
				canvas.DrawBitmap(_bitmap, aspectRect);
			}
			//Text layer
			if (_textLayer == null) {
				_textLayer = new SKBitmap(info);
				using (SKCanvas layerCanvas = new SKCanvas(_textLayer)) {
					layerCanvas.Clear();
					layerCanvas.SetMatrix(_m);
					using (var paint = new SKPaint()) {
						paint.TextSize = 10;
						paint.Color = SKColors.Red;
						paint.IsAntialias = true;
						paint.Style = SKPaintStyle.Fill;
						paint.TextAlign = SKTextAlign.Center;
						float curX = 0;
						float curY = 0;
						while (curX < info.Width) {
							while (curY < info.Height) {
								SKRect cell = new SKRect(curX, curY, curX + CELL_DIM, curY + CELL_DIM);
								layerCanvas.DrawText("Hi", cell.MidX, cell.MidY, paint);
								curY += CELL_DIM;
							}
							curY = 0;
							curX += CELL_DIM;
						}
					}
				}
				canvas.DrawBitmap(_textLayer, info.Rect);
			} else {
				using (new SKAutoCanvasRestore(canvas)) {
					canvas.SetMatrix(_currentTransformM);
					canvas.DrawBitmap(_textLayer, info.Rect);
				}
			}
		}
	}
}