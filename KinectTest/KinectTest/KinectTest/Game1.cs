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
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;


namespace KinectTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        
        KinectAudioSource kinectSource;
        Runtime kinectSensor;
        SkeletonData skeleton;
        SpeechRecognitionEngine speechEngine;
        Stream stream;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;              
        Vector2 position,font_pos,resolution,hazard_pos;
        Texture2D kinectRGBVideo,controller, hazard;
        Color[] controller_data,hazard_data;        

        string speechMsg;
        string RecognizerId = "SR_MS_en-US_Kinect_10.0";      
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
            resolution = new Vector2(640, 480);
            hazard_pos = new Vector2(400, 150);

            kinectSensor = Runtime.Kinects[0];            
            kinectSensor.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            kinectSensor.NuiCamera.ElevationAngle = 5;
            kinectSensor.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            kinectSensor.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinectSensor_VideoFrameReady);

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

            kinectSource = new KinectAudioSource();
            kinectSource.FeatureMode = true;
            kinectSource.AutomaticGainControl = false;
            kinectSource.SystemMode = SystemMode.OptibeamArrayOnly;

            var rec = (from r in SpeechRecognitionEngine.InstalledRecognizers() where r.Id == RecognizerId select r).FirstOrDefault();

            speechEngine = new SpeechRecognitionEngine(rec.Id);

            var choices = new Choices();
            choices.Add("scalpal");
            choices.Add("syringe");
            choices.Add("suction");
            GrammarBuilder gb = new GrammarBuilder();
            gb.Culture = rec.Culture;
            gb.Append(choices);

            var g = new Grammar(gb);

            speechEngine.LoadGrammar(g);
            speechEngine.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sre_SpeechHypothesized);
            speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            speechEngine.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

            Console.WriteLine("Recognizing Speech");

            stream = kinectSource.Start();

            speechEngine.SetInputToAudioStream(stream,
                          new SpeechAudioFormatInfo(
                              EncodingFormat.Pcm, 16000, 16, 1,
                              32000, 2, null));


            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
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

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            foreach (SkeletonData s in e.SkeletonFrame.Skeletons)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    skeleton = s;
                    Joint rightHandJoint = skeleton.Joints[JointID.HandRight];
                    position = new Vector2((((0.5f * rightHandJoint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * rightHandJoint.Position.Y) + 0.5f) * (resolution.Y)));
                }
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

        void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.Write("\rSpeech Rejected: \t{0}", e.Result.Text);
            //speechNotRecognized = true;
        }

        void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //speechNotRecognized = false;
            if (e.Result.Text == "scalpal")
            {
                speechMsg = ": Scalpal Selected!";
            }
            else if (e.Result.Text == "syringe")
            {
                speechMsg = ": Syringe Selected!";
            }
            else if (e.Result.Text == "suction")
            {
                speechMsg = ": Suction Selected!";
            }
            Console.Write("\rSpeech Recognized: \t{0} \n", e.Result.Text);
        }

        

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            kinectSensor.Uninitialize();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {            

            Rectangle controllerRectangle = new Rectangle((int)position.X, (int)position.Y, controller.Width, controller.Height);
            Rectangle hazardRectangle = new Rectangle((int)hazard_pos.X, (int)hazard_pos.Y, hazard.Width, hazard.Height);
            CollisionDetection collision = new CollisionDetection();
            if (collision.IntersectPixel(controllerRectangle, controller_data, hazardRectangle, hazard_data))
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
            spriteBatch.Begin();

                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
                spriteBatch.DrawString(font, message, font_pos = new Vector2(0, 430), Color.White);
                spriteBatch.DrawString(font, "Speech Recognition"+speechMsg, font_pos = new Vector2(0, 450), Color.White);
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

            spriteBatch.End();
            base.Draw(gameTime);
        }      
    }
}
