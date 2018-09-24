using System;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;
using System.Diagnostics;

namespace SkiaDemo1
{
	public class LayerPerformancePage : ContentPage
	{
        private const bool USE_LAYER = false;
        private const int CELL_DIM = 8;

		private SKCanvasView _canvasV = null;
		private SKMatrix _m = SKMatrix.MakeIdentity();
        private SKMatrix _textLayerM = SKMatrix.MakeIdentity();
        private SKBitmap _bitmap = null;
		private SKBitmap _textLayer = null;
        private bool _isPanZoom = false;

        public LayerPerformancePage()
		{
            Title = "Layer Performance";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
            Grid mainG = new Grid();
            mainG.Children.Add(_canvasV, 0, 0);
            MR.Gestures.BoxView gestureV = new MR.Gestures.BoxView();
            mainG.Children.Add(gestureV, 0, 0);
            Content = mainG;
            //Load assets
            using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().GetTypeInfo().Assembly, "landscape.jpg"))) {
				_bitmap = SKBitmap.Decode(stream);
			}
            //Interaction
            gestureV.LongPressing += HandleLongPressed;
            gestureV.Panning += HandlePanning;
            gestureV.Panned += HandlePanned;
            gestureV.Pinching += HandlePinching;
            gestureV.Pinched += HandlePinched;
            gestureV.Rotating += HandleRotating;
            gestureV.Rotated += HandleRotated;
        }

        private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            //Background layer
            SKRect bitmapRect = CalculateBitmapAspectRect(_bitmap);
            using (new SKAutoCanvasRestore(canvas)) {
                canvas.SetMatrix(_m);
                canvas.DrawBitmap(_bitmap, bitmapRect);
            }
            //Text layer
            if (!USE_LAYER) {
                using (new SKAutoCanvasRestore(canvas)) {
                    canvas.SetMatrix(_m);
                    using (SKPaint p = new SKPaint { TextSize = 5, Color = SKColors.Red, IsAntialias = true, Style = SKPaintStyle.Fill, TextAlign = SKTextAlign.Center }) {
                        for (int offset = 0; offset < 10; offset++) {
                            float curX = offset;
                            float curY = offset;
                            while (curX < info.Width) {
                                while (curY < info.Height) {
                                    SKRect cell = new SKRect(curX, curY, curX + CELL_DIM, curY + CELL_DIM);
                                    canvas.DrawText("Hi", cell.MidX, cell.MidY, p);
                                    curY += CELL_DIM;
                                }
                                curY = offset;
                                curX += CELL_DIM;
                            }
                        }
                    }
                }
            } else {
                if (_textLayer == null) {
                    _textLayer = new SKBitmap(info);
                    using (SKCanvas layerCanvas = new SKCanvas(_textLayer)) {
                        layerCanvas.Clear();
                        layerCanvas.SetMatrix(_m);
                        using (SKPaint p = new SKPaint { TextSize = 5, Color = SKColors.Red, IsAntialias = true, Style = SKPaintStyle.Fill, TextAlign = SKTextAlign.Center }) {
                            for (int offset = 0; offset < 10; offset++) {
                                float curX = offset;
                                float curY = offset;
                                while (curX < info.Width) {
                                    while (curY < info.Height) {
                                        SKRect cell = new SKRect(curX, curY, curX + CELL_DIM, curY + CELL_DIM);
                                        layerCanvas.DrawText("Hi", cell.MidX, cell.MidY, p);
                                        curY += CELL_DIM;
                                    }
                                    curY = offset;
                                    curX += CELL_DIM;
                                }
                            }
                        }
                    }
                    canvas.DrawBitmap(_textLayer, info.Rect);
                } else {
                    using (new SKAutoCanvasRestore(canvas)) {
                        canvas.SetMatrix(_textLayerM);
                        canvas.DrawBitmap(_textLayer, info.Rect);
                    }
                }
            }
        }

        private SKRect CalculateBitmapAspectRect(SKBitmap bitmap)
        {
            if (bitmap == null)
                return (SKRect.Empty);
            SKSize imgSize = new SKSize(bitmap.Width, bitmap.Height);
            return (SKRect.Create(_canvasV.CanvasSize.Width, _canvasV.CanvasSize.Height).AspectFit(imgSize));
        }

        private void InvalidateLayers()
        {
            if (_textLayer != null) {
                _textLayer.Dispose();
                _textLayer = null;
            }
            _textLayerM = SKMatrix.MakeIdentity();
        }

        private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs e)
        {
            _m = SKMatrix.MakeIdentity();
            InvalidateLayers();
            _canvasV.InvalidateSurface();
        }

        private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
        {
            float deltaX = (float)pea.DeltaDistance.X;
            float deltaY = (float)pea.DeltaDistance.Y;
            if (!_isPanZoom) {
                _isPanZoom = true;
                _textLayerM = SKMatrix.MakeIdentity();
            }
            SKPoint deltaTran = ToUntransformedCanvasPt(deltaX, deltaY);
            SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTran.X, deltaTran.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            SKMatrix.PostConcat(ref _textLayerM, deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
        {
            _isPanZoom = false;
            InvalidateLayers();
            _canvasV.InvalidateSurface();
        }

        private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
        {
            float pivotX = (float)pea.Center.X;
            float pivotY = (float)pea.Center.Y;
            float deltaScale = (float)pea.DeltaScale;
            if (!_isPanZoom) {
                _isPanZoom = true;
                _textLayerM = SKMatrix.MakeIdentity();
            }
            SKPoint pivotPt = ToUntransformedCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeScale(deltaScale, deltaScale, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            SKMatrix.PostConcat(ref _textLayerM, deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePinched(object sender, MR.Gestures.PinchEventArgs pea)
        {
            _isPanZoom = false;
            InvalidateLayers();
            _canvasV.InvalidateSurface();
        }

        private void HandleRotating(object sender, MR.Gestures.RotateEventArgs rea)
        {
            float pivotX = (float)rea.Center.X;
            float pivotY = (float)rea.Center.Y;
            float deltaAngle = (float)rea.DeltaAngle;
            if (!_isPanZoom) {
                _isPanZoom = true;
                _textLayerM = SKMatrix.MakeIdentity();
            }
            SKPoint pivotPt = ToUntransformedCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeRotationDegrees(deltaAngle, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            SKMatrix.PostConcat(ref _textLayerM, deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandleRotated(object sender, MR.Gestures.RotateEventArgs rea)
        {
            _isPanZoom = false;
            InvalidateLayers();
            _canvasV.InvalidateSurface();
        }

        private SKPoint ToUntransformedCanvasPt(float x, float y)
        {
            return (new SKPoint(x * _canvasV.CanvasSize.Width / (float)_canvasV.Width, y * _canvasV.CanvasSize.Height / (float)_canvasV.Height));
        }
    }
}
