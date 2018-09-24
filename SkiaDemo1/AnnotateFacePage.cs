using System;
using System.Reflection;
using System.Windows.Input;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

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
		private bool _isDrawing = false;
		private SKMatrix _m = SKMatrix.MakeIdentity();
		private SKMatrix _im = SKMatrix.MakeIdentity();
		private SKBitmap _bitmap = null;
        private SKPoint _lastPanPt = SKPoint.Empty;
        private SKPath _sketchPath = new SKPath();

        public AnnotateFacePage()
		{
			BindingContext = this;
            Title = "Annotate Face";
            ToolbarItem drawTBI = new ToolbarItem();
			ToolbarItems.Add(drawTBI);
			ToolbarItem clearTBI = new ToolbarItem {
				Text = "Clear"
			};
			ToolbarItems.Add(clearTBI);
			_canvasV = new SKCanvasView();
			_canvasV.PaintSurface += HandlePaintCanvas;
            Grid mainG = new Grid();
            mainG.Children.Add(_canvasV, 0, 0);
            MR.Gestures.BoxView gestureV = new MR.Gestures.BoxView();
            mainG.Children.Add(gestureV, 0, 0);
            Content = mainG;
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
            //Bindings
            drawTBI.SetBinding(ToolbarItem.TextProperty, nameof(IsDrawMode), converter: new BoolDrawModeValueConverter());
            drawTBI.SetBinding(ToolbarItem.CommandProperty, nameof(DrawCommand));
            clearTBI.SetBinding(ToolbarItem.CommandProperty, nameof(ClearCommand));
        }

        private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            SKRect bitmapRect = CalculateBitmapAspectRect(_bitmap);
            canvas.SetMatrix(_m);
            canvas.DrawBitmap(_bitmap, bitmapRect);
            using (SKPaint p = new SKPaint { IsAntialias = true, StrokeWidth = 5, StrokeCap = SKStrokeCap.Round, Color = SKColors.Red, Style = SKPaintStyle.Stroke }) {
                e.Surface.Canvas.DrawPath(_sketchPath, p);
            }
        }

        private SKRect CalculateBitmapAspectRect(SKBitmap bitmap)
        {
            if (bitmap == null)
                return (SKRect.Empty);
            SKSize imgSize = new SKSize(bitmap.Width, bitmap.Height);
            return (SKRect.Create(_canvasV.CanvasSize.Width, _canvasV.CanvasSize.Height).AspectFit(imgSize));
        }

        private void HandleLongPressed(object sender, MR.Gestures.LongPressEventArgs e)
        {
            _m = SKMatrix.MakeIdentity();
            _im = SKMatrix.MakeIdentity();
            _canvasV.InvalidateSurface();
        }

        private void HandlePanning(object sender, MR.Gestures.PanEventArgs pea)
        {
            if(IsDrawMode) {
                float drawX = (float)pea.Center.X;
                float drawY = (float)pea.Center.Y;
                SKPoint drawPt = ToUntransformedCanvasPt(drawX, drawY);
                SKPoint canvasPt = _im.MapPoint(drawPt);
                if (!_isDrawing) {
                    _sketchPath.MoveTo(canvasPt);
                    _isDrawing = true;
                } else {
                    _sketchPath.LineTo(canvasPt);
                }
            } else {
                float deltaX = (float)pea.DeltaDistance.X;
                float deltaY = (float)pea.DeltaDistance.Y;
                SKPoint deltaTran = ToUntransformedCanvasPt(deltaX, deltaY);
                SKMatrix deltaM = SKMatrix.MakeTranslation(deltaTran.X, deltaTran.Y);
                SKMatrix.PostConcat(ref _m, deltaM);
            }
            _canvasV.InvalidateSurface();
        }

        private void HandlePanned(object sender, MR.Gestures.PanEventArgs pea)
        {
            if(IsDrawMode)
                _isDrawing = false;
            else
                _m.TryInvert(out _im);
        }

        private void HandlePinching(object sender, MR.Gestures.PinchEventArgs pea)
        {
            float pivotX = (float)pea.Center.X;
            float pivotY = (float)pea.Center.Y;
            float deltaScale = (float)pea.DeltaScale;
            SKPoint pivotPt = ToUntransformedCanvasPt(pivotX, pivotY);
            SKMatrix deltaM = SKMatrix.MakeScale(deltaScale, deltaScale, pivotPt.X, pivotPt.Y);
            SKMatrix.PostConcat(ref _m, deltaM);
            _canvasV.InvalidateSurface();
        }

        private void HandlePinched(object sender, MR.Gestures.PinchEventArgs pea)
        {
            _m.TryInvert(out _im);
        }

        private SKPoint ToUntransformedCanvasPt(float x, float y)
        {
            return (new SKPoint(x * _canvasV.CanvasSize.Width / (float)_canvasV.Width, y * _canvasV.CanvasSize.Height / (float)_canvasV.Height));
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
    }
}
