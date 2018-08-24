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
        private const int ANNOTATION_CIRCLE_RADIUS = 20;

        private SKCanvasView _canvasV = null;
        private SKBitmap _bitmap = null;
        private SKBitmap _layer = null;
        private SKMatrix _m = SKMatrix.MakeIdentity();
        private SKMatrix _im = SKMatrix.MakeIdentity();
        private SKMatrix? _layerM = null;
        private int _rotationAngle = 0;
        private int _selectedCircle = -1;

        public TouchManipulationPage()
        {
            Title = "Touch Manipulation";
            ToolbarItem rotateTBI = new ToolbarItem {
                Text = "Rotate"
            };
            rotateTBI.Clicked += HandleRotateClicked;
            ToolbarItems.Add(rotateTBI);
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
            gestureV.Tapped += HandleTapped;
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
            SKRect bitmapRect = CalculateBitmapAspectRect();
            using (new SKAutoCanvasRestore(canvas)) {
                canvas.SetMatrix(_m);
                canvas.DrawBitmap(_bitmap, bitmapRect);
            }
            //Draw layer
            if (_layer == null) {
                _layer = new SKBitmap(info);
                using (SKCanvas layerCanvas = new SKCanvas(_layer)) {
                    layerCanvas.Clear();
                    layerCanvas.SetMatrix(_m);
                    float angle = GetAngle(_m);
                    using (SKPaint p = new SKPaint { Color = SKColors.Red, IsAntialias = true, StrokeWidth = 3, StrokeCap = SKStrokeCap.Round, TextAlign = SKTextAlign.Center, TextSize = 40 }) {
                        p.Style = SKPaintStyle.Stroke;
                        SKRect boxR = new SKRect(bitmapRect.MidX - 50, bitmapRect.MidY - 50, bitmapRect.MidX + 50, bitmapRect.MidY + 50);
                        layerCanvas.DrawRect(boxR, p);
                        p.Style = SKPaintStyle.Fill;
                        p.Color = (_selectedCircle == 0) ? SKColors.Yellow : SKColors.Red;
                        layerCanvas.DrawCircle(bitmapRect.Left, bitmapRect.Top, ANNOTATION_CIRCLE_RADIUS, p);
                        p.Color = (_selectedCircle == 1) ? SKColors.Yellow : SKColors.Red;
                        layerCanvas.DrawCircle(bitmapRect.Right, bitmapRect.Top, ANNOTATION_CIRCLE_RADIUS, p);
                        p.Color = (_selectedCircle == 2) ? SKColors.Yellow : SKColors.Red;
                        layerCanvas.DrawCircle(bitmapRect.Right, bitmapRect.Bottom, ANNOTATION_CIRCLE_RADIUS, p);
                        p.Color = (_selectedCircle == 3) ? SKColors.Yellow : SKColors.Red;
                        layerCanvas.DrawCircle(bitmapRect.Left, bitmapRect.Bottom, ANNOTATION_CIRCLE_RADIUS, p);
                        p.Color = SKColors.Green;
                        layerCanvas.Save();
                        layerCanvas.RotateDegrees(-angle, boxR.Left, boxR.Top);
                        layerCanvas.DrawText("Hello", boxR.Left, boxR.Top, p);
                        layerCanvas.Restore();
                        p.Color = SKColors.Blue;
                        layerCanvas.Save();
                        layerCanvas.RotateDegrees(-angle, boxR.Right, boxR.Top);
                        layerCanvas.DrawText("There", boxR.Right, boxR.Top, p);
                        layerCanvas.Restore();
                        p.Color = SKColors.Orange;
                        layerCanvas.Save();
                        layerCanvas.RotateDegrees(-angle, boxR.Left, boxR.Bottom);
                        layerCanvas.DrawText("Bye", boxR.Left, boxR.Bottom, p);
                        layerCanvas.Restore();
                        p.Color = SKColors.Purple;
                        layerCanvas.Save();
                        layerCanvas.RotateDegrees(-angle, boxR.Right, boxR.Bottom);
                        layerCanvas.DrawText("Now", boxR.Right, boxR.Bottom, p);
                        layerCanvas.Restore();
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

        private SKRect CalculateBitmapAspectRect()
        {
            if (_bitmap == null)
                return (SKRect.Empty);
            SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
            return (SKRect.Create(_canvasV.CanvasSize.Width, _canvasV.CanvasSize.Height).AspectFit(imgSize));
        }

        private void HandleRotateClicked(object sender, EventArgs e)
        {
            _rotationAngle += 90;
            if (_rotationAngle == 360)
                _rotationAngle = 0;
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private void HandleTapped(object sender, MR.Gestures.TapEventArgs tea)
        {
            SKPoint canvasPt = ToCanvasPt((float)tea.Center.X, (float)tea.Center.Y);
            SKRect bitmapRect = CalculateBitmapAspectRect();
            //Check UL
            SKRect ulRect = new SKRect(bitmapRect.Left, bitmapRect.Top, bitmapRect.Left, bitmapRect.Top);
            ulRect.Inflate(ANNOTATION_CIRCLE_RADIUS, ANNOTATION_CIRCLE_RADIUS);
            if(ulRect.Contains(canvasPt)) {
                _selectedCircle = 0;
                InvalidateLayer();
                _canvasV.InvalidateSurface();
                return;
            }
            //Check UR
            SKRect urRect = new SKRect(bitmapRect.Right, bitmapRect.Top, bitmapRect.Right, bitmapRect.Top);
            urRect.Inflate(ANNOTATION_CIRCLE_RADIUS, ANNOTATION_CIRCLE_RADIUS);
            if (urRect.Contains(canvasPt)) {
                _selectedCircle = 1;
                InvalidateLayer();
                _canvasV.InvalidateSurface();
                return;
            }
            //Check LR
            SKRect lrRect = new SKRect(bitmapRect.Right, bitmapRect.Bottom, bitmapRect.Right, bitmapRect.Bottom);
            lrRect.Inflate(ANNOTATION_CIRCLE_RADIUS, ANNOTATION_CIRCLE_RADIUS);
            if (lrRect.Contains(canvasPt)) {
                _selectedCircle = 2;
                InvalidateLayer();
                _canvasV.InvalidateSurface();
                return;
            }
            //Check LL
            SKRect llRect = new SKRect(bitmapRect.Left, bitmapRect.Bottom, bitmapRect.Left, bitmapRect.Bottom);
            llRect.Inflate(ANNOTATION_CIRCLE_RADIUS, ANNOTATION_CIRCLE_RADIUS);
            if (llRect.Contains(canvasPt)) {
                _selectedCircle = 3;
                InvalidateLayer();
                _canvasV.InvalidateSurface();
                return;
            }
            _selectedCircle = -1;
            InvalidateLayer();
            _canvasV.InvalidateSurface();
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
                _m.TryInvert(out _im);
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
            float deltaX = (float)pea.DeltaDistance.X;
            float deltaY = (float)pea.DeltaDistance.Y;
            if (!_layerM.HasValue)
                _layerM = SKMatrix.MakeIdentity();
            SKPoint deltaTran = ToUntransformedCanvasPt(deltaX, deltaY);
            SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTran.X, deltaTran.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            DeltaLayer(deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
        {
            _m.TryInvert(out _im);
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
        {
            float pivotX = (float)pea.Center.X;
            float pivotY = (float)pea.Center.Y;
            float deltaScale = (float)pea.DeltaScale;
            if (!_layerM.HasValue)
                _layerM = SKMatrix.MakeIdentity();
            SKPoint pivotPt = ToUntransformedCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeScale(deltaScale, deltaScale, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            DeltaLayer(deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePinched(object sender, MR.Gestures.PinchEventArgs pea)
        {
            _m.TryInvert(out _im);
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private void HandleRotating(object sender, MR.Gestures.RotateEventArgs rea)
        {
            float pivotX = (float)rea.Center.X;
            float pivotY = (float)rea.Center.Y;
            float deltaAngle = (float)rea.DeltaAngle;
            if (!_layerM.HasValue)
                _layerM = SKMatrix.MakeIdentity();
            SKPoint pivotPt = ToUntransformedCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeRotationDegrees(deltaAngle, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            DeltaLayer(deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandleRotated(object sender, MR.Gestures.RotateEventArgs rea)
        {
            _m.TryInvert(out _im);
            InvalidateLayer();
            _canvasV.InvalidateSurface();
        }

        private SKPoint ToUntransformedCanvasPt(float x, float y)
        {
            return (new SKPoint(x * _canvasV.CanvasSize.Width / (float)_canvasV.Width, y * _canvasV.CanvasSize.Height / (float)_canvasV.Height));
        }

        private SKPoint ToCanvasPt(float x,float y)
        {
            SKPoint untransformedPt = ToUntransformedCanvasPt(x, y);
            return (_im.MapPoint(untransformedPt));
        }
    }
}
