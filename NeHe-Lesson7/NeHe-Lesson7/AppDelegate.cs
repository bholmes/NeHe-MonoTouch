using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace NeHeLesson7
{
	/// <summary>
	/// The UIApplicationDelegate for the application. This class is responsible for launching the 
	/// User Interface of the application, as well as listening (and optionally responding) to 
	/// application events from iOS.
	/// </summary>
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		OpenGLViewController viewController;
		
		// This method is invoked when the application has loaded its UI and is ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			app.SetStatusBarHidden (true, false);
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			// load the appropriate UI, depending on whether the app is running on an iPhone or iPad
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) {
				viewController = new OpenGLViewController ("OpenGLViewController_iPhone", null);
			} else {
				viewController = new OpenGLViewController ("OpenGLViewController_iPad", null);
			}
			window.RootViewController = viewController;
			
			((EAGLView)(viewController.View)).Run (60.0);

			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
		
		public override void OnResignActivation (UIApplication application)
		{
			((EAGLView)(viewController.View)).Stop ();
			((EAGLView)(viewController.View)).Run(5.0);

		}
		
		public override void OnActivated (UIApplication application)
		{
			((EAGLView)(viewController.View)).Stop ();
			((EAGLView)(viewController.View)).Run(60.0);
		}
	}
}
