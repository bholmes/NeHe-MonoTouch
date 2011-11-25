using System;

using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.iPhoneOS;

using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace NeHeLesson1
{
	[Register ("OpenGLViewController")]
	public partial class OpenGLViewController : UIViewController
	{
		public OpenGLViewController (string nibName, NSBundle bundle) : base (nibName, bundle)
		{
		}
		
		new EAGLView View { get { return (EAGLView)base.View; } }
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			NSNotificationCenter.DefaultCenter.RemoveObserver (this);
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.Portrait ||
				toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
		}		
	}
}
