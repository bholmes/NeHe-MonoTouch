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

namespace NeHeLesson5
{
	[Register ("EAGLView")]
	public class EAGLView : iPhoneOSGameView
	{
		private float rtri = 0f;
		private float rquad = 0f;
		
		
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
			
			GL1.ShadeModel(All1.Smooth);								// Enables Smooth Shading
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.5f);						// Black Background

			// Create a depth renderbuffer
			uint depthRenderBuffer = 0;
			GL.GenRenderbuffers (1,ref depthRenderBuffer);
			GL.BindRenderbuffer (All.Renderbuffer, depthRenderBuffer);
			
			// Allocate storage for the new renderbuffer
			GL.RenderbufferStorage (All.Renderbuffer, All.DepthComponent16, Size.Width, Size.Height);
			
			// Attach the renderbuffer to the framebuffer's depth attachment point
			GL.FramebufferRenderbuffer (All.Framebuffer, All.DepthAttachment, All.Renderbuffer, depthRenderBuffer);
			
			GL1.ClearDepth(1.0f);										// Depth Buffer Setup
			GL1.Enable(All1.DepthTest);									// Enables Depth Testing
			GL1.DepthFunc( All1.Lequal);								// The Type Of Depth Test To Do
			
			GL1.Hint( All1.PerspectiveCorrectionHint , All1.Nicest);	// Really Nice Perspective Calculations	
			
		}

		
		void SetupView ()
		{
			GL.Viewport(0, 0, Size.Width, Size.Height);					// Reset The Current Viewport
			GL1.MatrixMode (All1.Projection);							// Select The Projection Matrix
			
			GL1.LoadIdentity ();										// Reset The Projection Matrix            
			
			// Calculate The Aspect Ratio Of The Window
			Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,  this.Bounds.Size.Width / this.Bounds.Size.Height, 1.0f, 100.0f);
			GL1.MatrixMode (All1.Projection);
			GL1.LoadMatrix(ref projection.Row0.X);
			
			GL1.MatrixMode(All1.Modelview);								// Select The Modelview Matrix
			GL1.LoadIdentity ();										// Reset The Modelview Matrix
		}
		
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			
			MakeCurrent ();
			
			SetupView ();
			
			GL1.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));	// Clear The Screen And The Depth Buffer
    		GL1.LoadIdentity();											// Reset The Current Modelview Matrix
			
			GL1.Translate(-1.5f, 0.0f, -6.0f);			// Move Left And Into The Screen
			GL1.Rotate (rtri, 0.0f, 1.0f, 0.0f);		// Rotate The Pyramid On It's Y Axis
			
			float [] triangleVerticies = {
				0.0f, 1.0f, 0.0f,			// Top Of Triangle (Front)
				-1.0f,-1.0f, 1.0f,			// Left Of Triangle (Front)
				1.0f,-1.0f, 1.0f,			// Right Of Triangle (Front)
				
				0.0f, 1.0f, 0.0f,			// Top Of Triangle (Right)
				1.0f,-1.0f, 1.0f,			// Left Of Triangle (Right)
				1.0f,-1.0f, -1.0f,			// Right Of Triangle (Right)
				
				0.0f, 1.0f, 0.0f,			// Top Of Triangle (Back)
				1.0f,-1.0f, -1.0f,			// Left Of Triangle (Back)
				-1.0f,-1.0f, -1.0f,			// Right Of Triangle (Back)
				
				0.0f, 1.0f, 0.0f,			// Top Of Triangle (Left)
				-1.0f,-1.0f,-1.0f,			// Left Of Triangle (Left)
				-1.0f,-1.0f, 1.0f,			// Right Of Triangle (Left)
			};
			
			float [] triangleColors = {
				1.0f,0.0f,0.0f,1.0f,			// Red
				0.0f,1.0f,0.0f,1.0f,			// Green
				0.0f,0.0f,1.0f,1.0f,			// Blue
				1.0f,0.0f,0.0f,1.0f,			// Red
				0.0f,0.0f,1.0f,1.0f,			// Blue
				0.0f,1.0f,0.0f,1.0f,			// Green
				1.0f,0.0f,0.0f,1.0f,			// Red
				0.0f,1.0f,0.0f,1.0f,			// Green
				0.0f,0.0f,1.0f,1.0f,			// Blue
				1.0f,0.0f,0.0f,1.0f,			// Red
				0.0f,0.0f,1.0f,1.0f,			// Blue
				0.0f,1.0f,0.0f,1.0f,			// Green
			};
			
			GL1.VertexPointer(3, All1.Float, 0, triangleVerticies);
			GL1.EnableClientState(All1.VertexArray);
			
			GL1.ColorPointer (4, All1.Float, 0, triangleColors);
			GL1.EnableClientState (All1.ColorArray);
			
			GL1.DrawArrays (All1.Triangles, 0, 12);
			
			GL1.LoadIdentity ();
			GL1.Translate(1.5f,0.0f,-7.0f);              // Move Right And Into The Screen
			 
			GL1.Rotate(rquad,1.0f,1.0f,1.0f);            // Rotate The Cube On X, Y & Z
			
			float [] cubeVerticies = {
				-1.0f, 1.0f,-1.0f,						// Top Left Of The Quad (Top)
				-1.0f, 1.0f, 1.0f,						// Bottom Left Of The Quad (Top)
				1.0f, 1.0f,-1.0f,						// Top Right Of The Quad (Top)
				1.0f, 1.0f, 1.0f,						// Bottom Right Of The Quad (Top)

				-1.0f,-1.0f, 1.0f,						// Top Left Of The Quad (Bottom)
				-1.0f,-1.0f,-1.0f,						// Bottom Left Of The Quad (Bottom)
				1.0f,-1.0f, 1.0f,						// Top Right Of The Quad (Bottom)
				1.0f,-1.0f,-1.0f,						// Bottom Right Of The Quad (Bottom)
				
				-1.0f, 1.0f, 1.0f,						// Top Left Of The Quad (Front)
				-1.0f,-1.0f, 1.0f,						// Bottom Left Of The Quad (Front)
				1.0f, 1.0f, 1.0f,						// Top Right Of The Quad (Front)
				1.0f,-1.0f, 1.0f,						// Bottom Right Of The Quad (Front)

				1.0f, 1.0f,-1.0f,						// Top Left Of The Quad (Back)
				1.0f,-1.0f,-1.0f,						// Bottom Left Of The Quad (Back)
				-1.0f, 1.0f,-1.0f,						// Top Right Of The Quad (Back)
				-1.0f,-1.0f,-1.0f,						// Bottom Right Of The Quad (Back)
				
				-1.0f, 1.0f,-1.0f,						// Top Left Of The Quad (Left)
				-1.0f,-1.0f,-1.0f,						// Bottom Left Of The Quad (Left)
				-1.0f, 1.0f, 1.0f,						// Top Right Of The Quad (Left)
				-1.0f,-1.0f, 1.0f,						// Bottom Right Of The Quad (Left)
				
		        1.0f, 1.0f, 1.0f,						// Top Left Of The Quad (Right)
		        1.0f,-1.0f, 1.0f,						// Bottom Left Of The Quad (Right)
				1.0f, 1.0f,-1.0f,						// Top Right Of The Quad (Right)
		        1.0f,-1.0f,-1.0f,						// Bottom Right Of The Quad (Right)
			};
			
			GL1.VertexPointer(3, All1.Float, 0, cubeVerticies);
			GL1.EnableClientState(All1.VertexArray);
			
			GL1.DisableClientState (All1.ColorArray);
			
			GL1.Color4 (0.0f, 1.0f, 0.0f, 1.0f);			// Set The Color To Green
			GL1.DrawArrays (All1.TriangleStrip, 0, 4);
			
			GL1.Color4 (1.0f, 0.5f, 0.0f, 1.0f);			// Set The Color To Orange
			GL1.DrawArrays (All1.TriangleStrip, 4, 4);
			
			GL1.Color4 (1.0f, 0.0f, 0.0f, 1.0f);			// Set The Color To Red
			GL1.DrawArrays (All1.TriangleStrip, 8, 4);
			
			GL1.Color4 (1.0f, 1.0f, 0.0f, 1.0f);			// Set The Color To Yellow
			GL1.DrawArrays (All1.TriangleStrip, 12, 4);
			
			GL1.Color4 (0.0f, 0.0f, 1.0f, 1.0f);			// Set The Color To Blue
			GL1.DrawArrays (All1.TriangleStrip, 16, 4);
			
			GL1.Color4 (1.0f, 0.0f, 1.0f, 1.0f);			// Set The Color To Violet
			GL1.DrawArrays (All1.TriangleStrip, 20, 4);
			
			SwapBuffers ();
			
			rtri+=2f;									// Increase The Rotation Variable For The Triangle
			rquad-=1.5f;								// Decrease The Rotation Variable For The Quad 
		}
		

	}
}
