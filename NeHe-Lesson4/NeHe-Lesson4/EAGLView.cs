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

namespace NeHeLesson4
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
			
				
			GL1.ShadeModel(All1.Smooth);                        // Enables Smooth Shading
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);                   // Black Background

			GL.ClearDepth(1.0f);                         // Depth Buffer Setup
			GL.Enable(All.DepthTest);                        // Enables Depth Testing
			GL.DepthFunc( All.Lequal);                         // The Type Of Depth Test To Do
			
			GL1.Hint( All1.PerspectiveCorrectionHint , All1.Nicest);          // Really Nice Perspective Calculations

		}

		
		void SetupView ()
		{
			GL.Viewport(0, 0, Size.Width, Size.Height);                    // Reset The Current Viewport
			GL1.MatrixMode (All1.Projection);                        // Select The Projection Matrix
			
			GL1.LoadIdentity ();                           // Reset The Projection Matrix            
			
			// Calculate The Aspect Ratio Of The Window
			Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,  this.Bounds.Size.Width / this.Bounds.Size.Height, 1.0f, 64.0f);
			GL1.MatrixMode (All1.Projection);
			GL1.LoadMatrix(ref projection.Row0.X);
			
			GL1.MatrixMode(All1.Modelview);                     // Select The Modelview Matrix
			GL1.LoadIdentity ();                           // Reset The Modelview Matrix
		}
		
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			
			MakeCurrent ();
			
			SetupView ();
			
			GL1.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));         // Clear The Screen And The Depth Buffer
    		GL1.LoadIdentity();                           // Reset The Current Modelview Matrix
			
			GL1.Translate(-1.5f,0.0f,-6.0f);                 // Move Left 1.5 Units And Into The Screen 6.0
			
			GL1.Rotate (rtri,0.0f,1.0f,0.0f);             // Rotate The Triangle On The Y axis ( NEW )

			
			float [] triangleVertices = {	0.0f, 1.0f, 0.0f,              // Top
    										-1.0f,-1.0f, 0.0f,              // Bottom Left
    		 								1.0f,-1.0f, 0.0f};              // Bottom Right
			
			float [] triangleColors = {	1.0f,0.0f,0.0f,1.0f,          // Set The Color To Red
										0.0f,1.0f,0.0f,1.0f,          // Set The Color To Green
										0.0f,0.0f,1.0f,1.0f};         // Set The Color To Blue
			
			GL1.VertexPointer (3, All1.Float, 0, triangleVertices);
			GL1.EnableClientState (All1.VertexArray);
			
			GL1.ColorPointer (4, All1.Float, 0, triangleColors);
			GL1.EnableClientState (All1.ColorArray);
			
			GL.DrawArrays (All.Triangles, 0, 3);
			
			GL1.LoadIdentity();                   // Reset The Current Modelview Matrix
			GL1.Translate(1.5f,0.0f,-6.0f);              // Move Right 1.5 Units And Into The Screen 6.0
			GL1.Rotate(rquad,1.0f,0.0f,0.0f);            // Rotate The Quad On The X axis ( NEW )
			
			float []  squareVerticies = {	-1.0f, 1.0f, 0.0f,             // Top Left
											-1.0f,-1.0f, 0.0f,             // Bottom Left
	         						  		1.0f, 1.0f, 0.0f,              // Top Right
	         						  		1.0f,-1.0f, 0.0f};              // Bottom Right
	        						  
			GL1.DisableClientState(All1.ColorArray);
			GL1.Color4(0.5f,0.5f,1.0f,1.0f);              // Set The Color To Blue One Time Only
			GL1.VertexPointer ( 3, All1.Float, 0, squareVerticies);
			GL1.EnableClientState (All1.VertexArray);
			
			GL.DrawArrays (All.TriangleStrip, 0, 4);
			
			SwapBuffers ();
			
			rtri+=2f;                     // Increase The Rotation Variable For The Triangle ( NEW )
    		rquad-=1.5f;                       // Decrease The Rotation Variable For The Quad     ( NEW )
		}
		

	}
}
