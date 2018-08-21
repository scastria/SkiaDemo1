using System;
using System.Reflection;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
    public class TouchManipulationPage : ContentPage
    {
        private SKCanvasView _canvasV = null;
        private SKBitmap _bitmap = null;
        private SKMatrix _m = SKMatrix.MakeIdentity();

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
            canvas.SetMatrix(_m);
            //Draw bitmap to fill screen maintaining aspect
            SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
            SKRect aspectRect = SKRect.Create(_canvasV.CanvasSize.Width, _canvasV.CanvasSize.Height).AspectFit(imgSize);
            canvas.DrawBitmap(_bitmap, aspectRect);
        }

        private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs lpea)
        {
            _m = SKMatrix.MakeIdentity();
            _canvasV.InvalidateSurface();
        }

        private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
        {
            HandlePan((float)pea.DeltaDistance.X, (float)pea.DeltaDistance.Y);
        }

        private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
        {
            HandlePan((float)pea.DeltaDistance.X, (float)pea.DeltaDistance.Y);
        }

        private void HandlePan(float deltaX, float deltaY)
        {
            SKPoint deltaTran = ToCanvasPt(deltaX, deltaY);
            SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTran.X, deltaTran.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
        {
            HandlePinch((float)pea.Center.X, (float)pea.Center.Y, (float)pea.DeltaScale);
        }

        private void HandlePinched(object sender, MR.Gestures.PinchEventArgs pea)
        {
            HandlePinch((float)pea.Center.X, (float)pea.Center.Y, (float)pea.DeltaScale);
        }

        private void HandlePinch(float pivotX, float pivotY, float deltaScale)
        {
            SKPoint pivotPt = ToCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeScale(deltaScale, deltaScale, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandleRotating(object sender, MR.Gestures.RotateEventArgs rea)
        {
            HandleRotate((float)rea.Center.X, (float)rea.Center.Y, (float)rea.DeltaAngle);
        }

        private void HandleRotated(object sender, MR.Gestures.RotateEventArgs rea)
        {
            HandleRotate((float)rea.Center.X, (float)rea.Center.Y, (float)rea.DeltaAngle);
        }

        private void HandleRotate(float pivotX, float pivotY, float deltaAngle)
        {
            SKPoint pivotPt = ToCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeRotationDegrees(deltaAngle, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            _canvasV.InvalidateSurface();
        }

        private SKPoint ToCanvasPt(float x, float y)
        {
            return (new SKPoint(x * _canvasV.CanvasSize.Width / (float)_canvasV.Width, y * _canvasV.CanvasSize.Height / (float)_canvasV.Height));
        }
    }
}
