using System;
using System.Collections.Generic;
using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class MrGesturesSquaresPage : ContentPage
	{
		private const int NUM_ROWS = 18;
		private const int NUM_COLS = 10;

		private SKCanvasView _canvasV = null;
		private List<GridSquare> _squares = null;
		private float _screenScale;
		private bool _isPanZoom = false;
		private Point _totalDistance;
		private SKMatrix _m = SKMatrix.MakeIdentity();
		private SKMatrix _im = SKMatrix.MakeIdentity();
		private SKMatrix _r = SKMatrix.MakeIdentity();
		private SKMatrix _ir = SKMatrix.MakeIdentity();
		private SKMatrix _startM = SKMatrix.MakeIdentity();
		private Point _startAnchorPt;
		private double _totalScale;

		public MrGesturesSquaresPage()
		{
#if __ANDROID__
            _screenScale = ((Android.App.Activity)Forms.Context).Resources.DisplayMetrics.Density;
#else
			_screenScale = (float)UIKit.UIScreen.MainScreen.Scale;
#endif
			Title = "Mr. Gestures Squares";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
			Grid mainG = new Grid();
			mainG.Children.Add(_canvasV, 0, 0);
			MR.Gestures.BoxView gestureV = new MR.Gestures.BoxView();
			mainG.Children.Add(gestureV, 0, 0);
			Content = mainG;
			//Interaction
			//gestureV.Down += HandleDown;
			gestureV.Tapped += HandleTapped;
			//gestureV.DoubleTapped += HandleDoubleTapped;
			gestureV.LongPressing += HandleLongPressed;
			gestureV.Panning += HandlePanning;
			gestureV.Panned += HandlePanned;
			gestureV.Pinching += HandlePinching;
			gestureV.Pinched += HandlePinched;
		}

		private void HandleTapped(object sender, MR.Gestures.TapEventArgs tea)
		{
			if (tea.NumberOfTouches != 1)
				return;
			Debug.WriteLine("Tapped: " + tea.Center);
			float unmappedCanvasX = (float)tea.Center.X * _screenScale;
			float unmappedCanvasY = (float)tea.Center.Y * _screenScale;
			SKPoint unrotatedCanvasPt = _im.MapPoint(unmappedCanvasX, unmappedCanvasY);
			SKPoint canvasPt = _ir.MapPoint(unrotatedCanvasPt);
			//Search for tapped square
			foreach (GridSquare square in _squares) {
				if (square.Contains(canvasPt.X, canvasPt.Y)) {
					square.IsSelected = !square.IsSelected;
					break;
				}
			}
			_canvasV.InvalidateSurface();
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
			//Update inverse
			_m.TryInvert(out _im);
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
			//Update inverse
			_m.TryInvert(out _im);
		}

		private void StartPanZoom(Point viewPt)
		{
			_startM = _m;
			_startAnchorPt = viewPt;
			_totalDistance = Point.Zero;
			_totalScale = 1;
			_isPanZoom = true;
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			_r = SKMatrix.MakeRotationDegrees(90, (float)width * _screenScale / 2, (float)height * _screenScale / 2);
			_r.TryInvert(out _ir);
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

		private void ResetZoom()
		{
			_m = SKMatrix.MakeIdentity();
			_im = SKMatrix.MakeIdentity();
			_canvasV.InvalidateSurface();
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKMatrix outM = SKMatrix.MakeIdentity();
			SKMatrix.Concat(ref outM, ref _m, ref _r);
			e.Surface.Canvas.SetMatrix(outM);
			e.Surface.Canvas.Clear();
			if (_squares == null) {
				float squareWidth = (float)e.Info.Width / NUM_COLS;
				float squareHeight = (float)e.Info.Height / NUM_ROWS;
				_squares = new List<GridSquare>();
				for (int r = 3; r < NUM_ROWS - 3; r++) {
					for (int c = 3; c < NUM_COLS - 3; c++) {
						GridSquare square = new GridSquare();
						square.AddRect(new SKRect(squareWidth * c, squareHeight * r, squareWidth * (c + 1), squareHeight * (r + 1)));
						_squares.Add(square);
					}
				}
			}
			using (SKPaint p = new SKPaint()) {
				p.IsAntialias = true;
				p.StrokeWidth = 3;
				foreach (GridSquare square in _squares) {
					//Draw filling first
					if (square.IsSelected) {
						p.Style = SKPaintStyle.Fill;
						p.Color = SKColors.Blue;
						e.Surface.Canvas.DrawPath(square, p);
					}
					//Draw outline
					p.Style = SKPaintStyle.Stroke;
					p.Color = SKColors.Black;
					e.Surface.Canvas.DrawPath(square, p);
				}
			}
		}
	}

	class GridSquare : SKPath
	{
		public bool IsSelected { get; set; }
	}
}
