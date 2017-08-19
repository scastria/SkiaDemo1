using System;
using System.Diagnostics;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class ImageInteractionPage : ContentPage
	{
		private SKCanvasView _canvasV = null;
		private SKMatrix _m = SKMatrix.MakeIdentity();
		private SKMatrix _r = SKMatrix.MakeIdentity();
		private SKMatrix _startM = SKMatrix.MakeIdentity();
		private bool _isPanZoom = false;
		private Point _totalDistance;
		private Point _startAnchorPt;
		private double _totalScale;
		private float _screenScale;
		private SKBitmap _bitmap = null;
		private SKRect _aspectRect;
		private int _rotationAngle = 0;

		public ImageInteractionPage()
		{
#if __ANDROID__
            _screenScale = ((Android.App.Activity)Forms.Context).Resources.DisplayMetrics.Density;
#else
			_screenScale = (float)UIKit.UIScreen.MainScreen.Scale;
#endif
			Title = "Image Interaction";
			ToolbarItem rotateTBI = new ToolbarItem {
				Text = "Rotate"
			};
			rotateTBI.Clicked += HandleRotate;
			ToolbarItems.Add(rotateTBI);
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
			Grid mainG = new Grid();
			mainG.Children.Add(_canvasV, 0, 0);
			MR.Gestures.BoxView gestureV = new MR.Gestures.BoxView();
			mainG.Children.Add(gestureV, 0, 0);
			Content = mainG;
			//Load assets
			using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().Assembly, "landscape.jpg"))) {
				_bitmap = SKBitmap.Decode(stream);
			}
			//Interaction
			//gestureV.Down += HandleDown;
			//gestureV.Tapped += HandleTapped;
			//gestureV.DoubleTapped += HandleDoubleTapped;
			gestureV.LongPressing += HandleLongPressed;
			gestureV.Panning += HandlePanning;
			gestureV.Panned += HandlePanned;
			gestureV.Pinching += HandlePinching;
			gestureV.Pinched += HandlePinched;
		}

		private void HandleRotate(object sender, EventArgs e)
		{
			_rotationAngle += 90;
			if (_rotationAngle == 360)
				_rotationAngle = 0;
			CalculateRotation();
            CalculateBitmapAspect();
			ResetZoom();
		}

		private void CalculateRotation()
		{
			_r = SKMatrix.MakeRotationDegrees(_rotationAngle);
			switch (_rotationAngle) {
			case 90:
				_r.TransX = (float)Width * _screenScale;
				break;
			case 180:
				_r.TransX = (float)Width * _screenScale;
				_r.TransY = (float)Height * _screenScale;
				break;
			case 270:
				_r.TransY = (float)Height * _screenScale;
				break;
			}
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
            CalculateRotation();
			CalculateBitmapAspect();
		}

		private void ResetZoom()
		{
			_m = SKMatrix.MakeIdentity();
			_canvasV.InvalidateSurface();
		}

		private void CalculateBitmapAspect()
		{
			SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
			switch (_rotationAngle) {
			case 0:
			case 180:
				_aspectRect = SKRect.Create((float)Width * _screenScale, (float)Height * _screenScale).AspectFit(imgSize);
				break;
			case 90:
			case 270:
				_aspectRect = SKRect.Create((float)Height * _screenScale, (float)Width * _screenScale).AspectFit(imgSize);
				break;
			}
		}

		private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs lpea)
		{
			ResetZoom();
		}

		private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
		{
			Debug.WriteLine("Panning: " + pea.Center);
			if (!_isPanZoom) {
				StartPanZoom(pea.Center);
			}
			if (_isPanZoom) {
				//PanZoom
				_totalDistance = pea.TotalDistance;
				DoPanZoom(_startM, _startAnchorPt, _totalDistance, _totalScale);
			}
			_canvasV.InvalidateSurface();
		}

		private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
		{
			Debug.WriteLine("Finish Pan");
			_isPanZoom = false;
		}

		private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
		{
			if (!_isPanZoom)
				StartPanZoom(pea.Center);
			_totalScale = pea.TotalScale;
			DoPanZoom(_startM, _startAnchorPt, _totalDistance, _totalScale);
		}

		private void HandlePinched(object sender, MR.Gestures.PinchEventArgs pea)
		{
			_isPanZoom = false;
		}

		private void StartPanZoom(Point viewPt)
		{
			_startM = _m;
			_startAnchorPt = viewPt;
			_totalDistance = Point.Zero;
			_totalScale = 1;
			_isPanZoom = true;
		}

		private void DoPanZoom(SKMatrix startM, Point anchorPt, Point totalTranslation, double totalScale)
		{
			Point canvasAnchorPt = new Point(anchorPt.X * _screenScale, anchorPt.Y * _screenScale);
			Point totalCanvasTranslation = new Point(totalTranslation.X * _screenScale, totalTranslation.Y * _screenScale);
			SKMatrix canvasTranslation = SKMatrix.MakeTranslation((float)totalCanvasTranslation.X, (float)totalCanvasTranslation.Y);
			SKMatrix canvasScaling = SKMatrix.MakeScale((float)totalScale, (float)totalScale, (float)canvasAnchorPt.X, (float)canvasAnchorPt.Y);
			SKMatrix canvasCombined = SKMatrix.MakeIdentity();
			SKMatrix.Concat(ref canvasCombined, ref canvasTranslation, ref canvasScaling);
			SKMatrix.Concat(ref _m, ref canvasCombined, ref startM);
			//Debug.WriteLine("Trans: (" + _m.TransX + "," + _m.TransY + ")");
			//Debug.WriteLine("Scale: (" + _m.ScaleX + "," + _m.ScaleY + ")");
			_canvasV.InvalidateSurface();
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKMatrix outM = SKMatrix.MakeIdentity();
			SKMatrix.Concat(ref outM, ref _m, ref _r);
			e.Surface.Canvas.SetMatrix(outM);
			e.Surface.Canvas.Clear();
			e.Surface.Canvas.DrawBitmap(_bitmap, _aspectRect);
		}
	}
}
