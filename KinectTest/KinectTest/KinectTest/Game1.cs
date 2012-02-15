using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Research.Kinect.Nui;


namespace KinectTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Runtime kinectSensor;
        Texture2D kinectRGBVideo;
        Vector2 position,font_pos,resolution,hazard_pos;
        Texture2D controller,hazard;
        Color[] controller_data,hazard_data;


        string message = "Collision: false";
        bool controllerHit = false;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Set up kinect and initialize it to use colour
            kinectSensor = Runtime.Kinects[0];
            kinectSensor.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            resolution = new Vector2(640,480);
            hazard_pos = new Vector2(400, 150);
            //rect = new Rectangle(0,0, 10, 10);
            kinectSensor.SkeletonEngine.TransformSmooth = true;
            TransformSmoothParameters p = new TransformSmoothParameters
            {
                Smoothing = 0.75f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            kinectSensor.SkeletonEngine.SmoothParameters = p;


            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
            kinectSensor.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            kinectSensor.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinectSensor_VideoFrameReady);
            //TiltKinectUp(5);
            base.Initialize();
            
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");

            kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);

            controller = Content.Load<Texture2D>("reddot");
            hazard = Content.Load<Texture2D>("hazard");

            controller_data = new Color[controller.Width * controller.Height];
            controller.GetData(controller_data);

            hazard_data = new Color[hazard.Width * hazard.Height];
            hazard.GetData(hazard_data);
            
        }

        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                SkeletonFrame allSkeletons = e.SkeletonFrame;
                SkeletonData playerSkeleton = (from s in allSkeletons.Skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                Joint rightHandJoint = playerSkeleton.Joints[JointID.HandRight];
                position = new Vector2((((0.5f * rightHandJoint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * rightHandJoint.Position.Y) + 0.5f) * (resolution.Y)));
            }
            catch
            {
                //Console.WriteLine("Holy Gawd i broke it!!!");
            }

        }
        //pass a value to tilt up by that much
        public void TiltKinectUp(int angle)
        {
            if (kinectSensor.NuiCamera.ElevationAngle < Camera.ElevationMaximum)
            {
                try
                {                   
                    kinectSensor.NuiCamera.ElevationAngle += angle;   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + " - no up tilt");
                }
            }
            else
            {
                angle = 0;
            }
            
        }

        public void TiltKinectDown(int angle)
        {
            if (kinectSensor.NuiCamera.ElevationAngle > Camera.ElevationMinimum)
            {
                try
                {                   
                    kinectSensor.NuiCamera.ElevationAngle -= angle;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + " - no down tilt");
                }
            }
            else
            {
                angle = 0;
            }
        }

        void kinectSensor_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage p = e.ImageFrame.Image;

            Color[] color = new Color[p.Height * p.Width];
            kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, p.Width, p.Height);

            int index = 0;
            for (int y = 0; y < p.Height; y++)
            {
                for (int x = 0; x < p.Width; x++, index += 4)
                {
                    color[y * p.Width + x] =
                    new Color(p.Bits[index + 2], p.Bits[index + 1], p.Bits[index + 0]);
                }
            }

            kinectRGBVideo.SetData(color);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            kinectSensor.Uninitialize();
        }

        protected override void Update(GameTime gameTime)
        {            
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            Console.WriteLine(controller.Bounds);

            Rectangle controllerRectangle = new Rectangle((int)position.X, (int)position.Y, controller.Width, controller.Height);
            Rectangle hazardRectangle = new Rectangle((int)hazard_pos.X, (int)hazard_pos.Y, hazard.Width, hazard.Height);

            if (IntersectPixels(controllerRectangle, controller_data, hazardRectangle, hazard_data))
            {
                controllerHit = true;
            }
            else
            {
                controllerHit = false;
            }

            base.Update(gameTime);
        }    

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
            spriteBatch.DrawString(font, message, font_pos = new Vector2(0, 0), Color.White);
            
           // controller.Draw(spriteBatch,position);
           // hazard.Draw(spriteBatch);
            spriteBatch.Draw(controller, position, Color.White);
            spriteBatch.Draw(hazard, hazard_pos, Color.White);

            if (controllerHit == true)
            {
                message = "Collision: true";
                hazard = Content.Load<Texture2D>("hazard_hit");
            }
            else
            {
                message = "Collision: false";
                hazard = Content.Load<Texture2D>("hazard");
            }
           // Console.WriteLine(position);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        static bool IntersectPixels(Rectangle rectangleA, Color[] dataA, Rectangle rectangleB, Color[] dataB)
        {

            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) + (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) + (y - rectangleB.Top) * rectangleB.Width];
                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }
            // No intersection found
            return false;
        }
    }
}
