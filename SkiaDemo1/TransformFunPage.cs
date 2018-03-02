using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
    public class TransformFunPage : ContentPage
	{
        private enum TouchActionType { Down, Moved, Up, Cancelled }

        private const string TEXT = "Hello World";

		private float _screenScale;
		private SKMatrix _m = SKMatrix.MakeIdentity();
        private SKCanvasView _canvasV = null;
		private Dictionary<int, TouchManipulationInfo> _touchDictionary = new Dictionary<int, TouchManipulationInfo>();
		private List<int> _touchIds = new List<int>();

		public TransformFunPage()
		{
#if __ANDROID__
            _screenScale = ((Android.App.Activity)Forms.Context).Resources.DisplayMetrics.Density;
#elif __IOS__
			_screenScale = (float)UIKit.UIScreen.MainScreen.Scale;
#else
            _screenScale = 1;
#endif
			Title = "Transform Fun";
            _canvasV = new SKCanvasView {
                BackgroundColor = Color.White
            };
			_canvasV.PaintSurface += HandlePaintCanvas;
            MR.Gestures.BoxView gestureV = new MR.Gestures.BoxView();
            Grid mainG = new Grid();
            mainG.Children.Add(_canvasV, 0, 0);
			mainG.Children.Add(gestureV, 0, 0);
			Content = mainG;
			//Interaction
			gestureV.Down += HandleDown;
            gestureV.Up += HandleUp;
			gestureV.LongPressing += HandleLongPressed;
			gestureV.Panning += HandlePanning;
			gestureV.Pinching += HandlePinching;
            gestureV.Rotating += HandleRotating;
		}

        private void HandleUp(object sender, MR.Gestures.DownUpEventArgs due)
        {
            System.Diagnostics.Debug.WriteLine("Up: " + string.Join(" ", due.TriggeringTouches));
			//foreach (int finger in due.TriggeringTouches)
                //HandleTouch(finger, TouchActionType.Up, due.Touches[finger]);
		}

        private void HandleDown(object sender, MR.Gestures.DownUpEventArgs due)
        {
            System.Diagnostics.Debug.WriteLine("Down: " + string.Join(" ", due.TriggeringTouches));
			//foreach (int finger in due.TriggeringTouches)
			//HandleTouch(finger, TouchActionType.Down, due.Touches[finger]);
		}

        private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs lpea)
        {
            _m = SKMatrix.MakeIdentity();
			_canvasV.InvalidateSurface();
		}

        private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
        {
            for (int finger = 0; finger < pea.Touches.Length; finger++)
                HandleTouch(finger, TouchActionType.Moved, pea.Touches[finger]);
		}

        private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
		{
			for (int finger = 0; finger < pea.Touches.Length; finger++)
				HandleTouch(finger, TouchActionType.Moved, pea.Touches[finger]);
		}

        private void HandleRotating(object sender, MR.Gestures.RotateEventArgs rea)
        {
			for (int finger = 0; finger < rea.Touches.Length; finger++)
				HandleTouch(finger, TouchActionType.Moved, rea.Touches[finger]);
		}

        private void HandleTouch(int finger, TouchActionType type, Point pt)
        {
            // Convert Xamarin.Forms point to pixels
            SKPoint point = new SKPoint((float)pt.X * _screenScale, (float)pt.Y * _screenScale);

			switch (type) {
            case TouchActionType.Down:
				_touchIds.Add(finger);
				ProcessTouchEvent(finger, type, point);
				break;
			case TouchActionType.Moved:
                if (!_touchIds.Contains(finger))
                    break;
				ProcessTouchEvent(finger, type, point);
				_canvasV.InvalidateSurface();
				break;
            case TouchActionType.Up:
			case TouchActionType.Cancelled:
				if (!_touchIds.Contains(finger))
					break;
				ProcessTouchEvent(finger, type, point);
				_touchIds.Remove(finger);
				_canvasV.InvalidateSurface();
				break;
			}
		}

        private void ProcessTouchEvent(int finger, TouchActionType type, SKPoint location)
		{
			switch (type) {
			case TouchActionType.Down:
				_touchDictionary.Add(finger, new TouchManipulationInfo {
					PreviousPoint = location,
					NewPoint = location
				});
				break;

			case TouchActionType.Moved:
				TouchManipulationInfo info = _touchDictionary[finger];
				info.NewPoint = location;
				Manipulate();
				info.PreviousPoint = info.NewPoint;
				break;

            case TouchActionType.Up:
				_touchDictionary[finger].NewPoint = location;
				Manipulate();
				_touchDictionary.Remove(finger);
				break;

			case TouchActionType.Cancelled:
				_touchDictionary.Remove(finger);
				break;
			}
		}

		private void Manipulate()
		{
			TouchManipulationInfo[] infos = new TouchManipulationInfo[_touchDictionary.Count];
			_touchDictionary.Values.CopyTo(infos, 0);
			SKMatrix touchMatrix = SKMatrix.MakeIdentity();

			if (infos.Length == 1) {
				SKPoint prevPoint = infos[0].PreviousPoint;
				SKPoint newPoint = infos[0].NewPoint;
                SKPoint pivotPoint = _m.MapPoint((float)_canvasV.Width * _screenScale / 2, (float)_canvasV.Height * _screenScale / 2);

				touchMatrix = OneFingerManipulate(prevPoint, newPoint, pivotPoint);
			} else if (infos.Length >= 2) {
				int pivotIndex = infos[0].NewPoint == infos[0].PreviousPoint ? 0 : 1;
				SKPoint pivotPoint = infos[pivotIndex].NewPoint;
				SKPoint newPoint = infos[1 - pivotIndex].NewPoint;
				SKPoint prevPoint = infos[1 - pivotIndex].PreviousPoint;

				touchMatrix = TwoFingerManipulate(prevPoint, newPoint, pivotPoint);
			}

			SKMatrix matrix = _m;
			SKMatrix.PostConcat(ref matrix, touchMatrix);
			_m = matrix;
		}

		private SKMatrix OneFingerManipulate(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
		{
			SKMatrix touchMatrix = SKMatrix.MakeIdentity();
			SKPoint delta = newPoint - prevPoint;
			SKMatrix.PostConcat(ref touchMatrix, SKMatrix.MakeTranslation(delta.X, delta.Y));
			return touchMatrix;
		}

		private SKMatrix TwoFingerManipulate(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
		{
			SKMatrix touchMatrix = SKMatrix.MakeIdentity();
			SKPoint oldVector = prevPoint - pivotPoint;
			SKPoint newVector = newPoint - pivotPoint;

			// Find angles from pivot point to touch points
			float oldAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
			float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

			// Calculate rotation matrix
			float angle = newAngle - oldAngle;
			touchMatrix = SKMatrix.MakeRotation(angle, pivotPoint.X, pivotPoint.Y);

			// Effectively rotate the old vector
			float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
			oldVector.X = magnitudeRatio * newVector.X;
			oldVector.Y = magnitudeRatio * newVector.Y;

			float scaleX = 1;
			float scaleY = 1;

			scaleX = scaleY = Magnitude(newVector) / Magnitude(oldVector);

			if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX) &&
				!float.IsNaN(scaleY) && !float.IsInfinity(scaleY)) {
				SKMatrix.PostConcat(ref touchMatrix,
					SKMatrix.MakeScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y));
			}

			return touchMatrix;
		}

		private float Magnitude(SKPoint point)
		{
			return (float)Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
		}
		
        private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKSizeI canvasSize = e.Info.Size;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            canvas.SetMatrix(_m);
			using (SKPaint p = new SKPaint()) {
				p.Color = SKColors.Blue;
				p.IsAntialias = true;
				p.Style = SKPaintStyle.Fill;
                p.TextSize = 100;
                p.TextAlign = SKTextAlign.Center;
                SKRect textBounds = SKRect.Empty;
                p.MeasureText(TEXT, ref textBounds);
                canvas.DrawText(TEXT, canvasSize.Width / 2, canvasSize.Height / 2 + textBounds.Height / 2, p);
                SKRect borderRect = new SKRect(canvasSize.Width / 2, canvasSize.Height / 2, canvasSize.Width / 2, canvasSize.Height / 2);
                borderRect.Inflate(textBounds.Width / 2, textBounds.Height / 2);
                p.Color = SKColors.Red;
                p.Style = SKPaintStyle.Stroke;
                canvas.DrawRect(borderRect, p);
			}
		}
    }

	class TouchManipulationInfo
	{
		public SKPoint PreviousPoint { set; get; }
		public SKPoint NewPoint { set; get; }
	}
}
