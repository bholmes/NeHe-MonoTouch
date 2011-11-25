using System;

using OpenTK;
using OpenTK.Graphics.ES20;
using GL1 = OpenTK.Graphics.ES11.GL;
using All1 = OpenTK.Graphics.ES11.All;
using OpenTK.Platform.iPhoneOS;

using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace NeHeLesson1
{
	[Register ("EAGLView")]
	public class EAGLView : iPhoneOSGameView
	{
		[Export("initWithCoder:")]
		public EAGLView (NSCoder coder) : base (coder)
		{
			LayerRetainsBacking = false;
			LayerColorFormat    = EAGLColorFormat.RGBA8;
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
		}
		
		[Export ("layerClass")]
		public static new Class GetLayerClass ()
		{
			return iPhoneOSGameView.GetLayerClass ();
		}
		
		protected override void ConfigureLayer (CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
		}
		
		#region DisplayLink support
		
		[Export ("drawFrame")]
		void DrawFrame ()
		{
			OnRenderFrame (new FrameEventArgs ());
		}
		
		#endregion
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			
				
			GL1.ShadeModel(All1.Smooth);                        // Enables Smooth Shading
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);                   // Black Background

			GL.ClearDepth(1.0f);                         // Depth Buffer Setup
			GL.Enable(All.DepthTest);                        // Enables Depth Testing
			GL.DepthFunc( All.Lequal);                         // The Type Of Depth Test To Do
						
							
			GL1.Hint( All1.PerspectiveCorrectionHint , All1.Nicest);          // Really Nice Perspective Calculations

		}
		
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			
			GL.Viewport(0, 0, Size.Width, Size.Height);                    // Reset The Current Viewport
			GL1.MatrixMode (All1.Projection);                        // Select The Projection Matrix
	
    		GL1.LoadIdentity ();                           // Reset The Projection Matrix            

            // Calculate The Aspect Ratio Of The Window
			Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Size.Width / (float)Size.Height, 1.0f, 64.0f);
            GL1.MatrixMode (All1.Projection);
            GL1.LoadMatrix(new float [] {projection.M11,projection.M12,projection.M13,projection.M14,
										projection.M21,projection.M22,projection.M23,projection.M24,
										projection.M31,projection.M32,projection.M33,projection.M34,
										projection.M41,projection.M42,projection.M34,projection.M44});
			
			GL1.MatrixMode(All1.Modelview);                     // Select The Modelview Matrix
			GL1.LoadIdentity ();                           // Reset The Modelview Matrix
		}
		

		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			
			MakeCurrent ();
			
			GL1.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));         // Clear The Screen And The Depth Buffer
    		GL1.LoadIdentity();                           // Reset The Current Modelview Matrix
			
			
			SwapBuffers ();
		}
		

	}
}