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

namespace NeHeLesson6
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
			
			LoadGLTextures ();
			
			GL.Enable (All.Texture2D);
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
		
		int [] texture = new int[1];
		
		unsafe private bool LoadGLTextures ()
		{
			fixed (int* texture_ptr = texture)
			{
				GL.GenTextures (1, &texture_ptr[0]);
				GL.BindTexture (All.Texture2D, texture[0]);
				
				GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Linear);
				GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			}
			
			bool loadUsingPng = true;
			if (loadUsingPng)
				LoadTextureFromPng ();
			else
				LoadTextureFromPvrtc ();

			return true;	
		}

		void LoadTextureFromPvrtc ()
		{
			using (NSData ns = NSData.FromFile ("NeHeImage.pvrtc"))
			{
				GL.CompressedTexImage2D (All.Texture2D, 0, All.CompressedRgbPvrtc4Bppv1Img, 256, 256, 0, (int)ns.Length, ns.Bytes);
			}
		}

		void LoadTextureFromPng ()
		{
			using (UIImage img = UIImage.FromFile ("NeHe.png"))
			{
				int width = img.CGImage.Width;
				int height = img.CGImage.Height;
				using (MonoTouch.CoreGraphics.CGColorSpace colorSpaceRef =  MonoTouch.CoreGraphics.CGColorSpace.CreateDeviceRGB ())
				{
					IntPtr imageData = System.Runtime.InteropServices.Marshal.AllocCoTaskMem (height * width * 4);
					try
					{						
						using (MonoTouch.CoreGraphics.CGBitmapContext context = new MonoTouch.CoreGraphics.CGBitmapContext (
							imageData, width, height, 8, 4 * width, colorSpaceRef, MonoTouch.CoreGraphics.CGBitmapFlags.ByteOrder32Big | 
							MonoTouch.CoreGraphics.CGBitmapFlags.PremultipliedLast))
						{
							colorSpaceRef.Dispose ();
							context.ClearRect (new System.Drawing.RectangleF (0.0f, 0.0f, (float)width, (float)height));
							context.TranslateCTM (0, 0);
							context.DrawImage (new System.Drawing.RectangleF (0.0f, 0.0f, (float)width, (float)height), img.CGImage);
							
							GL1.TexImage2D (All1.Texture2D, 0, (int)All1.Rgba, width, height, 0, All1.Rgba, All1.UnsignedByte, imageData);
						}
					}
					finally
					{
						System.Runtime.InteropServices.Marshal.FreeCoTaskMem (imageData);
					}
				}
			}
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
		
		float xrot = 0.0f;
		float yrot = 0.0f;
		float zrot = 0.0f;
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			
			MakeCurrent ();
			
			SetupView ();
			
			GL1.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));	// Clear The Screen And The Depth Buffer
    		GL1.LoadIdentity();											// Reset The Current Modelview Matrix
			
			GL1.LoadIdentity ();
			GL1.Translate(0.0f,0.0f,-5.0f);              // Move Right And Into The Screen
			
			GL1.Rotate (xrot, 1.0f, 0.0f, 0.0f);		// Rotate on the X Axis
			GL1.Rotate (yrot, 0.0f, 1.0f, 0.0f);		// Rotate on the Y Axis
			GL1.Rotate (zrot, 0.0f, 0.0f, 1.0f);		// Rotate on the Z Axis
			
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
			
			float [] cubeTexs = {
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f,1.0f,								// Bottom Right Of The Quad (Front)	
				
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f,1.0f,								// Bottom Right Of The Quad (Front)	
				
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f,1.0f,								// Bottom Right Of The Quad (Front)	
				
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f,1.0f,								// Bottom Right Of The Quad (Front)	
				
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f,1.0f,								// Bottom Right Of The Quad (Front)	
				
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f,1.0f,								// Bottom Right Of The Quad (Front)	
			};
			
			GL1.VertexPointer(3, All1.Float, 0, cubeVerticies);
			GL1.EnableClientState(All1.VertexArray);
			
			GL1.TexCoordPointer (2, All1.Float, 0, cubeTexs);
			GL1.EnableClientState (All1.TextureCoordArray);
		
			GL1.DrawArrays (All1.TriangleStrip, 0, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 4, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 8, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 12, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 16, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 20, 4);
			
			SwapBuffers ();
			
			xrot += 0.75f;
			yrot += 0.5f;
			zrot += 1.0f;
			
			if (xrot > 360f) xrot -= 360f;
			if (yrot > 360f) yrot -= 360f;
			if (zrot > 360f) zrot -= 360f;
		}
		

	}
}
