using System;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class HomePage : ContentPage
	{
		public HomePage()
		{
			Title = "Home";
			TextCell solidColorC = new TextCell {
				Text = "Solid Color"
			};
			solidColorC.Tapped += async delegate {
				await Navigation.PushAsync(new SolidColorPage());
			};
			TextCell rectangleC = new TextCell {
				Text = "Rectangle"
			};
			rectangleC.Tapped += async delegate {
				await Navigation.PushAsync(new RectanglePage());
			};
			TextCell primitivesC = new TextCell {
				Text = "Primitives"
			};
			primitivesC.Tapped += async delegate {
				await Navigation.PushAsync(new PrimitivesPage());
			};
			TextCell taskAnimationC = new TextCell {
				Text = "Task Animation"
			};
			taskAnimationC.Tapped += async delegate {
				await Navigation.PushAsync(new TaskAnimationPage());
			};
			TextCell imageC = new TextCell {
				Text = "Image"
			};
			imageC.Tapped += async delegate {
				await Navigation.PushAsync(new ImagePage());
			};
			TextCell imageInteractionC = new TextCell {
				Text = "Image Interaction"
			};
			imageInteractionC.Tapped += async delegate {
				await Navigation.PushAsync(new ImageInteractionPage());
			};
			TextCell mrGesturesC = new TextCell {
				Text = "Mr. Gestures Squares"
			};
			mrGesturesC.Tapped += async delegate {
				await Navigation.PushAsync(new MrGesturesSquaresPage());
			};
			TextCell annotateFaceC = new TextCell {
				Text = "Annotate Face"
			};
			annotateFaceC.Tapped += async delegate {
				await Navigation.PushAsync(new AnnotateFacePage());
			};
			TextCell polyLabelC = new TextCell {
				Text = "Polygon Label"
			};
			polyLabelC.Tapped += async delegate {
				await Navigation.PushAsync(new PolyLabelPage());
			};
			TextCell layerPerformanceC = new TextCell {
				Text = "Layer Performance"
			};
			layerPerformanceC.Tapped += async delegate {
				await Navigation.PushAsync(new LayerPerformancePage());
			};
			TableView tableV = new TableView {
				Intent = TableIntent.Menu,
				Root = new TableRoot {
					new TableSection {
						solidColorC,
						rectangleC,
						primitivesC,
						taskAnimationC,
						imageC,
						imageInteractionC,
						mrGesturesC,
						annotateFaceC,
						polyLabelC,
						layerPerformanceC
					}
				}
			};
			Content = tableV;
		}
	}
}
