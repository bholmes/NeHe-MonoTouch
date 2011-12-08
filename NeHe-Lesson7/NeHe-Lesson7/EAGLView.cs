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

namespace NeHeLesson7
{
	[Register ("EAGLView")]
	public class EAGLView : iPhoneOSGameView
	{
		bool light = false;  				// Lighting On/Off
		bool lp = false;					// L Pressed
		bool fp = false;					// F Pressed
		
		float xrot = 0f;					// X Rotation
		float yrot = 0f;					// Y Rotation
		float xspeed = 0f;					// X Rotation Speed
		float yspeed = 0f;					// Y Rotation Speed
		float z=-5.0f;						// Depth Into The Screen
		
		float [] LightAmbient = { 0.5f, 0.5f, 0.5f, 1.0f };		// Ambient Light Values ( NEW )
		float [] LightDiffuse = { 1.0f, 1.0f, 1.0f, 1.0f };		// Diffuse Light Values ( NEW )
		float [] LightPosition = { 0.0f, 0.0f, 2.0f, 1.0f };	// Light Position ( NEW )
		
		uint filter = 0;						// Which Filter To Use
		uint [] texture = new uint[3];			// Storage for 3 textures

		
		[Export("initWithCoder:")]
		public EAGLView (NSCoder coder) : base (coder)
		{
			LayerRetainsBacking = false;
			LayerColorFormat    = EAGLColorFormat.RGBA8;
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
			
			SetupButtons ();
		}
		
		UIButton upButton, downButton, leftButton, rightButton, lightButton, filterButton, plusButton, minusButton;
		
		private void SetupButtons()
		{
			float ypos = this.Bounds.Width - 40f;
			float xpos = this.Bounds.Height - 40f;
			
			this.AddButton (ref lightButton, 10, ypos, "Light");
			this.AddButton (ref filterButton, xpos, ypos, "Filter");
			ypos-=40;
			this.AddButton (ref upButton, 10, ypos, "Up");
			this.AddButton (ref downButton, xpos, ypos, "Down");
			ypos-=40;
			this.AddButton (ref leftButton, 10, ypos, "Left");
			this.AddButton (ref rightButton, xpos, ypos, "Right");
			ypos-=40;
			this.AddButton (ref plusButton, 10, ypos, "+");
			this.AddButton (ref minusButton, xpos, ypos, "-");
		}
		
		private void AddButton (ref UIButton button, float x, float y, string title)
		{
			button = UIButton.FromType (UIButtonType.RoundedRect);
			button.Frame = new System.Drawing.RectangleF(x, y, 50, 30);
			button.SetTitle (title, UIControlState.Normal);
			this.AddSubview (button);	
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
			
			GL1.Light(All1.Light0, All1.Ambient, LightAmbient);			// Setup The Ambient Light	
			GL1.Light(All1.Light0, All1.Diffuse, LightDiffuse);			// Setup The Diffuse Light
			GL1.Light(All1.Light0, All1.Position,LightPosition);		// Position The Light
			GL1.Enable (All1.Light0);
		}
		
		unsafe private bool LoadGLTextures ()
		{
			fixed (uint* texture_ptr = texture)
			{
				GL.GenTextures (3, &texture_ptr[0]);
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
			//texturetool -e PVRTC -o Crate.pvrtc Crate.png
			using (NSData ns = NSData.FromFile ("Crate.pvrtc"))
			{
				GL.BindTexture (All.Texture2D, texture[0]);
				GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Nearest);
				GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
				
				GL.CompressedTexImage2D (All.Texture2D, 0, All.CompressedRgbPvrtc4Bppv1Img, 256, 256, 0, (int)ns.Length, ns.Bytes);
				
				GL.BindTexture (All.Texture2D, texture[1]);
				GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
				GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Linear);
				
				GL.CompressedTexImage2D (All.Texture2D, 0, All.CompressedRgbPvrtc4Bppv1Img, 256, 256, 0, (int)ns.Length, ns.Bytes);
				
				GL.BindTexture (All.Texture2D, texture[2]);
				GL1.TexParameter (All1.Texture2D, All1.GenerateMipmap, 1);
				GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
				GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.LinearMipmapNearest);
				
				GL.CompressedTexImage2D (All.Texture2D, 0, All.CompressedRgbPvrtc4Bppv1Img, 256, 256, 0, (int)ns.Length, ns.Bytes);
				
			}
		}

		void LoadTextureFromPng ()
		{
			using (UIImage img = UIImage.FromFile ("Crate.png"))
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
							
							GL.BindTexture (All.Texture2D, texture[0]);
							GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Nearest);
							GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
							
							GL1.TexImage2D (All1.Texture2D, 0, (int)All1.Rgba, width, height, 0, All1.Rgba, All1.UnsignedByte, imageData);
							
							GL.BindTexture (All.Texture2D, texture[1]);
							GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
							GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Linear);
							
							GL1.TexImage2D (All1.Texture2D, 0, (int)All1.Rgba, width, height, 0, All1.Rgba, All1.UnsignedByte, imageData);
							
							GL.BindTexture (All.Texture2D, texture[2]);
							GL1.TexParameter (All1.Texture2D, All1.GenerateMipmap, 1);
							GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
							GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.LinearMipmapNearest);
							
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
				
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			
			MakeCurrent ();
			
			SetupView ();
			
			GL1.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));	// Clear The Screen And The Depth Buffer
    		GL1.LoadIdentity();											// Reset The Current Modelview Matrix
			
			GL1.Translate(0.0f,0.0f,z);					// Move Right And Into The Screen
			GL1.Rotate(xrot,1.0f,0.0f,0.0f);			// Rotate On The X Axis By xrot
			GL1.Rotate(yrot,0.0f,1.0f,0.0f);			// Rotate On The Y Axis By yrot
			
			GL1.BindTexture(All1.Texture2D, texture[filter]);			// Select A Texture Based On filter
 
			
			float [] cubeVerticies = {
				-1.0f, 1.0f, 1.0f,					// Top Left Of The Quad (Front)
				-1.0f,-1.0f, 1.0f,					// Bottom Left Of The Quad (Front)
				 1.0f, 1.0f, 1.0f,					// Top Right Of The Quad (Front)
				 1.0f,-1.0f, 1.0f,					// Bottom Right Of The Quad (Front)

				-1.0f,-1.0f,-1.0f,					// Bottom Left Of The Quad (Back)
				-1.0f, 1.0f,-1.0f,					// Top Left Of The Quad (Back)
				 1.0f,-1.0f,-1.0f,					// Bottom Right Of The Quad (Back)
				 1.0f, 1.0f,-1.0f,					// Top Right Of The Quad (Back)
				
				-1.0f, 1.0f,-1.0f,					// Top Left Of The Quad (Top)
				-1.0f, 1.0f, 1.0f,					// Bottom Left Of The Quad (Top)
				 1.0f, 1.0f,-1.0f,					// Top Right Of The Quad (Top)
				 1.0f, 1.0f, 1.0f,					// Bottom Right Of The Quad (Top)

				-1.0f,-1.0f,-1.0f,					// Top Left Of The Quad (Bottom)
				 1.0f,-1.0f,-1.0f,					// Top Right Of The Quad (Bottom)
				-1.0f,-1.0f, 1.0f,					// Bottom Left Of The Quad (Bottom)
				 1.0f,-1.0f, 1.0f,					// Bottom Right Of The Quad (Bottom)
				
				 1.0f,-1.0f,-1.0f,					// Bottom Right Of The Quad (Right)
				 1.0f, 1.0f,-1.0f,					// Top Right Of The Quad (Right)
				 1.0f,-1.0f, 1.0f,					// Bottom Left Of The Quad (Right)
				 1.0f, 1.0f, 1.0f,					// Top Left Of The Quad (Right)
				
		        -1.0f,-1.0f,-1.0f,					// Bottom Right Of The Quad (Left)
		        -1.0f,-1.0f, 1.0f,					// Bottom Left Of The Quad (Left)
				-1.0f, 1.0f,-1.0f,					// Top Right Of The Quad (Left)
		        -1.0f, 1.0f, 1.0f,					// Top Left Of The Quad (Left)
			};
			
			float [] cubeTexs = {
				0.0f, 0.0f,								// Top Left Of The Quad (Front)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Front)
				1.0f, 0.0f,								// Top Right Of The Quad (Front)
				1.0f, 1.0f,								// Bottom Right Of The Quad (Front)	
				
				1.0f, 1.0f,								// Bottom Left Of The Quad (Back)
				1.0f, 0.0f,								// Top Left Of The Quad (Back)
				0.0f, 1.0f,								// Bottom Right Of The Quad (Back)
				0.0f, 0.0f,								// Top Right Of The Quad (Back)	
				
				0.0f, 0.0f,								// Top Left Of The Quad (Top)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Top)
				1.0f, 0.0f,								// Top Right Of The Quad (Top)
				1.0f, 1.0f,								// Bottom Right Of The Quad (Top)	
				
				1.0f, 0.0f,								// Top Left Of The Quad (Bottom)
				0.0f, 0.0f,								// Top Right Of The Quad (Bottom)
				1.0f, 1.0f,								// Bottom Left Of The Quad (Bottom)
				0.0f, 1.0f,								// Bottom Right Of The Quad (Bottom)	
				
				1.0f, 1.0f,								// Bottom Right Of The Quad (Right)
				1.0f, 0.0f,								// Top Right Of The Quad (Right)
				0.0f, 1.0f,								// Bottom Left Of The Quad (Right)
				0.0f, 0.0f,								// Top Left Of The Quad (Right)	
				
				0.0f, 1.0f,								// Bottom Right Of The Quad (Left)
				1.0f, 1.0f,								// Bottom Left Of The Quad (Left)
				0.0f, 0.0f,								// Top Right Of The Quad (Left)
				1.0f, 0.0f,								// Top Left Of The Quad (Left)	
			};
			
			float [] cubeNormals = {
				 0.0f, 0.0f, 1.0f,						// Top Left Of The Quad (Front)
				 0.0f, 0.0f, 1.0f,						// Bottom Left Of The Quad (Front)
				 0.0f, 0.0f, 1.0f,						// Top Right Of The Quad (Front)
				 0.0f, 0.0f, 1.0f,						// Bottom Right Of The Quad (Front)

				 0.0f, 0.0f,-1.0f,						// Bottom Left Of The Quad (Back)
				 0.0f, 0.0f,-1.0f,						// Top Left Of The Quad (Back)
				 0.0f, 0.0f,-1.0f,						// Bottom Right Of The Quad (Back)
				 0.0f, 0.0f,-1.0f,						// Top Right Of The Quad (Back)
				
				 0.0f, 1.0f, 0.0f,						// Top Left Of The Quad (Top)
				 0.0f, 1.0f, 0.0f,						// Bottom Left Of The Quad (Top)
				 0.0f, 1.0f, 0.0f,						// Top Right Of The Quad (Top)
				 0.0f, 1.0f, 0.0f,						// Bottom Right Of The Quad (Top)
				
				 0.0f,-1.0f, 0.0f,						// Top Left Of The Quad (Bottom)
				 0.0f,-1.0f, 0.0f,						// Top Right Of The Quad (Bottom)
				 0.0f,-1.0f, 0.0f,						// Bottom Left Of The Quad (Bottom)
				 0.0f,-1.0f, 0.0f,						// Bottom Right Of The Quad (Bottom)
				
				 1.0f, 0.0f, 0.0f,						// Bottom Right Of The Quad (Right)
				 1.0f, 0.0f, 0.0f,						// Top Right Of The Quad (Right)
				 1.0f, 0.0f, 0.0f,						// Bottom Left Of The Quad (Right)
				 1.0f, 0.0f, 0.0f,						// Top Left Of The Quad (Right)
				
				-1.0f, 0.0f, 0.0f,						// Bottom Right Of The Quad (Left)
				-1.0f, 0.0f, 0.0f,						// Bottom Left Of The Quad (Left)
				-1.0f, 0.0f, 0.0f,						// Top Right Of The Quad (Left)
				-1.0f, 0.0f, 0.0f,						// Top Left Of The Quad (Left)
			};
			
			GL1.VertexPointer(3, All1.Float, 0, cubeVerticies);
			GL1.EnableClientState(All1.VertexArray);
			
			GL1.TexCoordPointer (2, All1.Float, 0, cubeTexs);
			GL1.EnableClientState (All1.TextureCoordArray);
			
			GL1.NormalPointer(All1.Float, 0, cubeNormals);
			GL1.EnableClientState(All1.NormalArray);
		
			GL1.DrawArrays (All1.TriangleStrip, 0, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 4, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 8, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 12, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 16, 4);
			
			GL1.DrawArrays (All1.TriangleStrip, 20, 4);
			
			SwapBuffers ();
			
			xrot+=xspeed;				// Add xspeed To xrot
    		yrot+=yspeed;				// Add yspeed To yrot
			
			handleButtons ();
		}
		
		private void handleButtons ()
		{
			if (lightButton.TouchInside && !lp)		// L Key Being Pressed Not Held?
			{	
				lp=true;							// lp Becomes TRUE
				light=!light;						// Toggle Light TRUE/FALSE
				if (!light)							// If Not Light
				{
					GL1.Disable(All1.Lighting);		// Disable Lighting
				}
				else								// Otherwise
				{
					GL1.Enable(All1.Lighting);		// Enable Lighting
				}		
			}
			if (!lightButton.TouchInside)			// Has L Key Been Released?
			{
				lp=false;							// If So, lp Becomes FALSE
			}
			if (filterButton.TouchInside && !fp)	// Is F Key Being Pressed?
			{
				fp=true;							// fp Becomes TRUE
				filter+=1;							// filter Value Increases By One
				if (filter>2)						// Is Value Greater Than 2?
				{
					filter=0;						// If So, Set filter To 0
				}
			}
			if (!filterButton.TouchInside)			// Has F Key Been Released?
			{
				fp=false;							// If So, fp Becomes FALSE
			}
			if (minusButton.TouchInside)			// Is Page Up Being Pressed?
			{	
				z-=0.02f;							// If So, Move Into The Screen
			}
			if (plusButton.TouchInside)				// Is Page Down Being Pressed?
			{
				z+=0.02f;							// If So, Move Towards The Viewer
			}
			if (upButton.TouchInside)				// Is Up Arrow Being Pressed?
			{
				xspeed-=0.01f;						// If So, Decrease xspeed
			}
			if (downButton.TouchInside)				// Is Down Arrow Being Pressed?
			{
				xspeed+=0.01f;						// If So, Increase xspeed
			}
			if (rightButton.TouchInside)			// Is Right Arrow Being Pressed?
			{
				yspeed+=0.01f;						// If So, Increase yspeed
			}
			if (leftButton.TouchInside)				// Is Left Arrow Being Pressed?
			{
				yspeed-=0.01f;						// If So, Decrease yspeed
			}
		}
	}
}
