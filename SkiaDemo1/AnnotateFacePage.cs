using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;

namespace SkiaDemo1
{
	public class AnnotateFacePage : ContentPage
	{
		private bool _isDrawMode;
		public bool IsDrawMode {
			get { return (_isDrawMode); }
			set {
				_isDrawMode = value;
				OnPropertyChanged();
			}
		}
		private Command _drawCommand = null;
		public ICommand DrawCommand {
			get {
				_drawCommand = _drawCommand ?? new Command(DoDrawCommand);
				return (_drawCommand);
			}
		}
		private Command _clearCommand = null;
		public ICommand ClearCommand {
			get {
				_clearCommand = _clearCommand ?? new Command(DoClearCommand);
				return (_clearCommand);
			}
		}

		private SKCanvasView _canvasV = null;
		private float _screenScale;
		private bool _isPanZoom = false;
		private bool _isDrawing = false;
		private Point _totalDistance;
		private SKMatrix _m = SKMatrix.MakeIdentity();
		private SKMatrix _im = SKMatrix.MakeIdentity();
		private SKMatrix _startM = SKMatrix.MakeIdentity();
		private Point _startAnchorPt;
		private double _totalScale;
		private SKBitmap _bitmap = null;
		private SKPath _sketchPath = new SKPath();

		public AnnotateFacePage()
		{
#if __ANDROID__
			_screenScale = ((Android.App.Activity)Forms.Context).Resources.DisplayMetrics.Density;
#elif __IOS__
            _screenScale = (float)UIKit.UIScreen.MainScreen.Scale;
#else
            _screenScale = 1;
#endif
			BindingContext = this;
			ToolbarItem drawTBI = new ToolbarItem();
			ToolbarItems.Add(drawTBI);
			ToolbarItem clearTBI = new ToolbarItem {
				Text = "Clear"
			};
			ToolbarItems.Add(clearTBI);
			Title = "Annotate Face";
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
			Grid mainG = new Grid();
			mainG.Children.Add(_canvasV, 0, 0);
			MR.Gestures.BoxView gestureV = new MR.Gestures.BoxView();
			mainG.Children.Add(gestureV, 0, 0);
			Content = mainG;
			//Bindings
			drawTBI.SetBinding<AnnotateFacePage>(ToolbarItem.TextProperty, vm => vm.IsDrawMode, converter: new BoolDrawModeValueConverter());
			drawTBI.SetBinding<AnnotateFacePage>(ToolbarItem.CommandProperty, vm => vm.DrawCommand);
			clearTBI.SetBinding<AnnotateFacePage>(ToolbarItem.CommandProperty, vm => vm.ClearCommand);
			//Load assets
			using (var stream = new SKManagedStream(ResourceLoader.GetEmbeddedResourceStream(this.GetType().GetTypeInfo().Assembly, "face.jpg"))) {
				_bitmap = SKBitmap.Decode(stream);
			}
			//Interaction
			gestureV.LongPressing += HandleLongPressed;
			gestureV.Panning += HandlePanning;
			gestureV.Panned += HandlePanned;
			gestureV.Pinching += HandlePinching;
			gestureV.Pinched += HandlePinched;
		}

		private void DoDrawCommand()
		{
			IsDrawMode = !IsDrawMode;
		}

		private void DoClearCommand()
		{
			_sketchPath.Reset();
			_canvasV.InvalidateSurface();
		}

		private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs lpea)
		{
			ResetZoom();
		}

		private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
		{
			Debug.WriteLine("Panning: " + pea.Center);
			if (IsDrawMode) {
				float unmappedCanvasX = (float)pea.Center.X * _screenScale;
				float unmappedCanvasY = (float)pea.Center.Y * _screenScale;
				SKPoint canvasPt = _im.MapPoint(unmappedCanvasX, unmappedCanvasY);
				if (!_isDrawing) {
					_sketchPath.MoveTo(canvasPt);
					_isDrawing = true;
				} else {
					_sketchPath.LineTo(canvasPt);
				}
			} else {
				if (!_isPanZoom) {
					StartPanZoom(pea.Center);
				}
				if (_isPanZoom) {
					//PanZoom
					_totalDistance = pea.TotalDistance;
					DoPanZoom(_startM, _startAnchorPt, _totalDistance, _totalScale);
				}
			}
			_canvasV.InvalidateSurface();
		}

		private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
		{
			Debug.WriteLine("Finish Pan");
			if (IsDrawMode) {
				_isDrawing = false;
			} else {
				_isPanZoom = false;
				//Update inverse
				_m.TryInvert(out _im);
			}
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
			e.Surface.Canvas.SetMatrix(_m);
			e.Surface.Canvas.Clear();
			using (SKPaint p = new SKPaint()) {
				p.IsAntialias = true;
				p.StrokeWidth = 5;
				p.StrokeCap = SKStrokeCap.Round;
				p.Color = SKColors.Red;
				p.Style = SKPaintStyle.Stroke;
				SKSize imgSize = new SKSize(_bitmap.Width, _bitmap.Height);
				SKRect aspectRect = SKRect.Create(e.Info.Width, e.Info.Height).AspectFit(imgSize);
				e.Surface.Canvas.DrawBitmap(_bitmap, aspectRect, p);
				e.Surface.Canvas.DrawPath(_sketchPath, p);
			}
		}
	}
}
