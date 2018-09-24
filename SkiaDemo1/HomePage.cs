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
            TextCell polyLabelC = new TextCell {
                Text = "Polygon Label"
            };
            polyLabelC.Tapped += async delegate {
                await Navigation.PushAsync(new PolyLabelPage());
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
            TextCell solarSystemC = new TextCell {
                Text = "Solar System"
            };
            solarSystemC.Tapped += async delegate {
                await Navigation.PushAsync(new SolarSystemPage());
            };
            TextCell imageInteractionC = new TextCell {
				Text = "Image Interaction"
			};
			imageInteractionC.Tapped += async delegate {
				await Navigation.PushAsync(new ImageInteractionPage());
			};
            TextCell annotateFaceC = new TextCell {
                Text = "Annotate Face"
            };
            annotateFaceC.Tapped += async delegate {
                await Navigation.PushAsync(new AnnotateFacePage());
            };
            TextCell layerPerformanceC = new TextCell {
                Text = "Layer Performance"
            };
            layerPerformanceC.Tapped += async delegate {
                await Navigation.PushAsync(new LayerPerformancePage());
            };
            TextCell touchManipC = new TextCell {
                Text = "Touch Manipulation"
            };
            touchManipC.Tapped += async delegate {
                await Navigation.PushAsync(new TouchManipulationPage());
            };
			TableView tableV = new TableView {
				Intent = TableIntent.Menu,
				Root = new TableRoot {
					new TableSection {
						solidColorC,
						rectangleC,
						primitivesC,
                        polyLabelC,
                        taskAnimationC,
						imageC,
                        solarSystemC,
                        imageInteractionC,
                        annotateFaceC,
                        layerPerformanceC,
                        touchManipC
					}
				}
			};
			Content = tableV;
		}
	}
}
