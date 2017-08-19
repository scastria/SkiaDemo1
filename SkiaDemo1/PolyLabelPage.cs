using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.EmbeddedResource;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class PolyLabelPage : ContentPage
	{
		private float[][][] _waterPoints = null;

		public PolyLabelPage()
		{
			Title = "Poly Label";
			SKCanvasView canvasV = new SKCanvasView();
			canvasV.PaintSurface += HandlePaintCanvas;
			Content = canvasV;
			string waterJson = ResourceLoader.GetEmbeddedResourceString(this.GetType().Assembly, "water1.json");
			_waterPoints = JsonConvert.DeserializeObject<float[][][]>(waterJson);
		}

		private void HandlePaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			SKRect canvasRect = SKRect.Create(e.Info.Size);
			e.Surface.Canvas.DrawColor(SKColors.White);
			using (SKPaint p = new SKPaint()) {
				p.Color = SKColors.Black;
				p.IsAntialias = true;
				p.Style = SKPaintStyle.Stroke;
				p.TextSize = 20;
				p.TextAlign = SKTextAlign.Center;
				using (SKPath path = new SKPath()) {
					//for (int i = 0; i < _waterPoints[0].Length; i++) {
					//	if (i == (_waterPoints[0].Length - 1))
					//		path.Close();
					//	else {
					//		SKPoint pt = new SKPoint(_waterPoints[0][i][0], _waterPoints[0][i][1]);
					//		if (i == 0)
					//			path.MoveTo(pt);
					//		else
					//			path.LineTo(pt);
					//	}
					//}
					//path.MoveTo(100, 100);
					//path.LineTo(500, 100);
					//path.LineTo(500, 200);
					//path.LineTo(200, 200);
					//path.LineTo(200, 500);
					//path.LineTo(100, 500);
					path.MoveTo(174, 709);
					path.LineTo(174, 933);
					path.LineTo(449, 931);
					path.LineTo(451, 711);
					path.Close();
					////I need to find the size of the path
					//SKRect pathRect = path.TightBounds;
					////I want to find the largest rectangle that can fit on my canvas maintaining the path's aspect ratio
					////SkiaSharp added a builtin method for this based on code from me
					//SKRect drawPathRect = canvasRect.AspectFit(pathRect.Size);
					////Now I need to transform the path to draw within the drawPathRect
					////First translate original path to its own origin
					//SKMatrix firstTranslateM = SKMatrix.MakeTranslation(-pathRect.Left, -pathRect.Top);
					////Next handle scaling.  Since I maintained aspect ratio, I should be able to use either
					////width or height to figure out scaling factor
					//float scalingFactor = drawPathRect.Width / pathRect.Width;
					//SKMatrix scaleM = SKMatrix.MakeScale(scalingFactor, scalingFactor);
					////Last I need to handle translation so path is centered on canvas
					//SKMatrix secondTranslateM = SKMatrix.MakeTranslation(drawPathRect.Left, drawPathRect.Top);
					////Now combine the translation, scaling, and translation into a single matrix by matrix multiplication/concatentation
					//SKMatrix transformM = SKMatrix.MakeIdentity();
					//SKMatrix.PostConcat(ref transformM, firstTranslateM);
					//SKMatrix.PostConcat(ref transformM, scaleM);
					//SKMatrix.PostConcat(ref transformM, secondTranslateM);
					////Now apply the transform to the path
					//path.Transform(transformM);
					e.Surface.Canvas.DrawPath(path, p);
					////Calculate poly label
					float[][] ring = new float[path.PointCount][];
					for (int i = 0; i < path.PointCount; i++) {
						SKPoint pt;
						if (i == path.PointCount)
							pt = path.Points[0];
						else
							pt = path.Points[i];
						ring[i] = new float[] { pt.X, pt.Y };
					}
					float[][][] polygon = new float[][][] { ring };
					float[] labelPos = PolyLabel.GetPolyLabel(polygon);
					Console.WriteLine("(" + labelPos[0] + "," + labelPos[1] + ")");
					p.Color = SKColors.Red;
					p.Style = SKPaintStyle.Fill;
					e.Surface.Canvas.DrawCircle(labelPos[0], labelPos[1], 3, p);
					e.Surface.Canvas.DrawText("Poly Label", labelPos[0], labelPos[1], p);
				}
			}
		}
	}
}
