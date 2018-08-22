using System;
using System.Diagnostics;
using System.Reflection;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
    public class TouchManipulationPage : ContentPage
    {
        private const string SELECTION_ANIMATION = "SelectionAnimation";
        private const int ANIMATION_DURATION = 1000;

        private SKCanvasView _canvasV = null;
        private SKBitmap _bitmap = null;
        private SKBitmap _layer = null;
        private SKMatrix _m = SKMatrix.MakeIdentity();
        private SKMatrix? _layerM = null;

        public TouchManipulationPage()
        {
            Title = "Touch Manipulation";
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
            //Draw bitmap to fill screen maintaining aspect
            SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
            SKRect aspectRect = SKRect.Create(_canvasV.CanvasSize.Width, _canvasV.CanvasSize.Height).AspectFit(imgSize);
            using (new SKAutoCanvasRestore(canvas)) {
                canvas.SetMatrix(_m);
                canvas.DrawBitmap(_bitmap, aspectRect);
            }
            //Draw layer
            if (_layer == null) {
                _layer = new SKBitmap(info);
                using (SKCanvas layerCanvas = new SKCanvas(_layer)) {
                    layerCanvas.Clear();
                    layerCanvas.SetMatrix(_m);
                    using (SKPaint p = new SKPaint { Color = SKColors.Red, IsAntialias = true, StrokeWidth = 3, StrokeCap = SKStrokeCap.Round, TextAlign = SKTextAlign.Center, TextSize = 40 }) {
                        p.Style = SKPaintStyle.Stroke;
                        layerCanvas.DrawRect(aspectRect.MidX - 50, aspectRect.MidY - 50, 100, 100, p);
                        p.Style = SKPaintStyle.Fill;
                        layerCanvas.DrawText("Hello", aspectRect.MidX, aspectRect.MidY, p);
                    }
                }
                canvas.DrawBitmap(_layer, info.Rect);
            } else {
                using (new SKAutoCanvasRestore(canvas)) {
                    SKMatrix layerM = (_layerM.HasValue) ? _layerM.Value : SKMatrix.MakeIdentity();
                    canvas.SetMatrix(layerM);
                    canvas.DrawBitmap(_layer, info.Rect);
                }
            }
        }

        private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs lpea)
        {
            AnimateReset();
        }

        private void AnimateReset()
        {
            //Animate transition to identity transform
            float startTransX = _m.TransX;
            float startTransY = _m.TransY;
            float startScale = _m.ScaleX;
            float startAngle = GetAngle(_m);
            float endTransX = 0;
            float endTransY = 0;
            float endScale = 1;
            float endAngle = 0;
            float totalTransX = endTransX - startTransX;
            float totalTransY = endTransY - startTransY;
            float totalScale = endScale - startScale;
            float totalAngle = endAngle - startAngle;
            _layerM = SKMatrix.MakeIdentity();
            new Animation(percent => {
                float curTransX = _m.TransX;
                float curTransY = _m.TransY;
                float curScale = _m.ScaleX;
                float curAngle = GetAngle(_m);
                float newTransX = totalTransX * (float)percent + startTransX;
                float newTransY = totalTransY * (float)percent + startTransY;
                float newScale = totalScale * (float)percent + startScale;
                float newAngle = totalAngle * (float)percent + startAngle;
                float deltaTransX = newTransX - curTransX;
                float deltaTransY = newTransY - curTransY;
                float deltaScale = newScale / curScale;
                float deltaAngle = newAngle - curAngle;
                //Animate translation
                SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTransX, deltaTransY);
                SKMatrix.PostConcat(ref _m, deltaM);
                DeltaLayer(deltaM);
                //Animate scale
                deltaM = SKMatrix.MakeScale(deltaScale, deltaScale);
                SKMatrix.PostConcat(ref _m, deltaM);
                DeltaLayer(deltaM);
                //Animate rotation
                deltaM = SKMatrix.MakeRotationDegrees(deltaAngle);
                SKMatrix.PostConcat(ref _m, deltaM);
                DeltaLayer(deltaM);
                _canvasV.InvalidateSurface();
            }).Commit(this, SELECTION_ANIMATION, length: ANIMATION_DURATION, easing: Easing.SinInOut, finished: (percent, isCanceled) => {
                _m = SKMatrix.MakeIdentity();
                InvalidateLayer();
                _canvasV.InvalidateSurface();
            });
        }

        private float GetAngle(SKMatrix matrix)
        {
            SKPoint unitVector = new SKPoint(1, 0);
            SKPoint transformedVector = matrix.MapVector(unitVector.X, unitVector.Y);
            double rad = Math.Atan2(transformedVector.Y, transformedVector.X);
            return ((float)(rad * 180 / Math.PI));
        }

        private void InvalidateLayer()
        {
            if (_layer != null) {
                _layer.Dispose();
                _layer = null;
            }
            _layerM = null;
        }

        private void DeltaLayer(SKMatrix deltaM)
        {
            SKMatrix layerM = _layerM.Value;
            SKMatrix.PostConcat(ref layerM, deltaM);
            _layerM = layerM;
        }

        private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
        {
            HandlePan((float)pea.DeltaDistance.X, (float)pea.DeltaDistance.Y);
        }

        private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
        {
            //HandlePan((float)pea.DeltaDistance.X, (float)pea.DeltaDistance.Y);
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private void HandlePan(float deltaX, float deltaY)
        {
            if (!_layerM.HasValue)
                _layerM = SKMatrix.MakeIdentity();
            SKPoint deltaTran = ToCanvasPt(deltaX, deltaY);
            SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTran.X, deltaTran.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            DeltaLayer(deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
        {
            HandlePinch((float)pea.Center.X, (float)pea.Center.Y, (float)pea.DeltaScale);
        }

        private void HandlePinched(object sender, MR.Gestures.PinchEventArgs pea)
        {
            //HandlePinch((float)pea.Center.X, (float)pea.Center.Y, (float)pea.DeltaScale);
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private void HandlePinch(float pivotX, float pivotY, float deltaScale)
        {
            if (!_layerM.HasValue)
                _layerM = SKMatrix.MakeIdentity();
            SKPoint pivotPt = ToCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeScale(deltaScale, deltaScale, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            DeltaLayer(deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandleRotating(object sender, MR.Gestures.RotateEventArgs rea)
        {
            HandleRotate((float)rea.Center.X, (float)rea.Center.Y, (float)rea.DeltaAngle);
        }

        private void HandleRotated(object sender, MR.Gestures.RotateEventArgs rea)
        {
            //HandleRotate((float)rea.Center.X, (float)rea.Center.Y, (float)rea.DeltaAngle);
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private void HandleRotate(float pivotX, float pivotY, float deltaAngle)
        {
            if (!_layerM.HasValue)
                _layerM = SKMatrix.MakeIdentity();
            SKPoint pivotPt = ToCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeRotationDegrees(deltaAngle, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            DeltaLayer(deltaM);
            _canvasV.InvalidateSurface();
        }

        private SKPoint ToCanvasPt(float x, float y)
        {
            return (new SKPoint(x * _canvasV.CanvasSize.Width / (float)_canvasV.Width, y * _canvasV.CanvasSize.Height / (float)_canvasV.Height));
        }
    }
}
