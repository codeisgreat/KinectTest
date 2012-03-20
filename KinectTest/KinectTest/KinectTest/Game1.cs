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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum cursorState {Hand,Scalpal,Syringe,Suction};

        Runtime nui;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Vector2 cursorPosition,font_pos, scalpal_pos, suction_pos,syringe_pos,hand_pos;
        Texture2D cursor, rgb_bg, textBuffer_bg,selectedTool;
        Texture2D tool_scalpal, tool_scalpal_over;
        Texture2D tool_suction, tool_suction_over;
        Texture2D tool_syringe, tool_syringe_over;
        Texture2D tool_hand, tool_hand_over;
        Rectangle scalpalRect, suctionRect, syringeRect, handRect,cursorRect;
        CollisionDetection collision;
        SpeechRec speech;
        Color[] cursor_data,scalpal_data,suction_data,syringe_data,hand_data;
        LoadKinect loadKinect;
        cursorState currentState = cursorState.Hand;

        string message = "Collision: false";
        string currentStateMsg = "";

        // global string to hold the string that the Kinect Audio has recognised
        string speechIn;

        // Collision detection boolean
        bool scalpalHit = false;
        bool suctionHit = false;
        bool syringeHit = false;
        bool handHit = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);           
            Content.RootDirectory = "Content";   
        }

        protected override void Initialize()
        {
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 680;
            graphics.PreferredBackBufferHeight = 640;
            graphics.ApplyChanges();

            scalpal_pos = new Vector2(560, 35);
            suction_pos = new Vector2(560, 140);
            syringe_pos = new Vector2(560, 245);
            hand_pos = new Vector2(560, 350);


            nui = Runtime.Kinects[0];
            loadKinect = new LoadKinect(graphics);            
            loadKinect.loadKinectNui(nui);          
            loadKinect.smoothKinectNui(nui);

            speech = new SpeechRec();
            speech.initSpeech();
            speechIn = speech.returnMsg();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // New SpriteBatch Object for Texture's and fonts.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load the font.
            font = Content.Load<SpriteFont>("Font/Arial");
            // load the texture object to hold the Kinect Video.
            loadKinect.loadKinectVideo(GraphicsDevice, 640, 480);
            // load the reddot image for the cursor.
            cursor = Content.Load<Texture2D>("Images/reddot");
            rgb_bg = Content.Load<Texture2D>("Images/rgb_bg");
            selectedTool = Content.Load<Texture2D>("Images/selectedTool");
            textBuffer_bg = Content.Load<Texture2D>("Images/textBuffer_bg");

            tool_scalpal = Content.Load<Texture2D>("Images/tool_scalpal");
            tool_scalpal_over = Content.Load<Texture2D>("Images/tool_scalpal_over");

            tool_suction = Content.Load<Texture2D>("Images/tool_suction");
            tool_suction_over = Content.Load<Texture2D>("Images/tool_suction_over");

            tool_syringe = Content.Load<Texture2D>("Images/tool_syringe");
            tool_syringe_over = Content.Load<Texture2D>("Images/tool_syringe_over");

            tool_hand = Content.Load<Texture2D>("Images/tool_hand");
            tool_hand_over = Content.Load<Texture2D>("Images/tool_hand_over");

            // create a Color Array to the size of the Cursors height and width in pixels
            cursor_data = new Color[cursor.Width * cursor.Height];
            scalpal_data = new Color[tool_scalpal.Width * tool_scalpal.Height];
            suction_data = new Color[tool_suction.Width * tool_suction.Height];
            syringe_data = new Color[tool_syringe.Width * tool_syringe.Height];
            hand_data = new Color[tool_hand.Width * tool_hand.Height];
            // pass the cursor object the color data (in this case red pixels and transparent pixels)

            cursor.GetData(cursor_data);
            tool_suction.GetData(suction_data);
            tool_scalpal.GetData(scalpal_data);
            tool_syringe.GetData(syringe_data);
            tool_hand.GetData(hand_data);
        }

        

        protected override void UnloadContent()
        {
            nui.Uninitialize();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Update the position of Position by getting the updated value from kinectInit.cs.
            cursorPosition = loadKinect.Pos;
            toolCollisionOccured();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            spriteBatch.Draw(rgb_bg, new Vector2(15, 15), Color.White);
            spriteBatch.Draw(textBuffer_bg, new Vector2(15, 510), Color.White);
            spriteBatch.Draw(loadKinect.kinectRGBVideo, new Rectangle(20, 20, 640, 480), Color.White);
            spriteBatch.Draw(selectedTool, new Vector2(560,460), Color.White);
            spriteBatch.Draw(tool_scalpal, scalpal_pos, Color.White);
            spriteBatch.Draw(tool_suction, suction_pos, Color.White);
            spriteBatch.Draw(tool_syringe, syringe_pos, Color.White);
            spriteBatch.Draw(tool_hand, hand_pos, Color.White);
            spriteBatch.DrawString(font, currentStateMsg, new Vector2(565, 465), Color.Black);
            spriteBatch.DrawString(font, message, font_pos = new Vector2(25, 520), Color.Green);
            spriteBatch.DrawString(font, "Speech Recognition" + speech.returnMsg(), font_pos = new Vector2(25,540), Color.Green);            
            spriteBatch.Draw(cursor, cursorPosition, Color.White);
            toolCollisionUpdateImage();

            switch (currentState)
            {
                case cursorState.Hand:
                    currentStateMsg = "Hand";
                    break;
                case cursorState.Syringe:
                    currentStateMsg = "Syringe";
                    break;
                case cursorState.Suction:
                    currentStateMsg = "Suction";
                    break;
                case cursorState.Scalpal:
                    currentStateMsg = "Scalpal";
                    break;
            };
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void toolCollisionUpdateImage()
        {
            //If the cursor is over the Image for the Scalpal activate the "over" highlighted image
            if (scalpalHit == true)
            {
                message = "Collision: true";
                tool_scalpal = tool_scalpal_over;
            }
            else
            {
                message = "Collision: false";
                tool_scalpal = Content.Load<Texture2D>("Images/tool_scalpal");
            }
            //If the cursor is over the Image for the Suction activate the "over" highlighted image
            if (suctionHit == true)
            {
                message = "Collision: true";
                tool_suction = tool_suction_over;
            }
            else
            {
                message = "Collision: false";
                tool_suction = Content.Load<Texture2D>("Images/tool_suction");
            }
            //If the cursor is over the Image for the Syringe activate the "over" highlighted image
            if (syringeHit == true)
            {
                message = "Collision: true";
                tool_syringe = tool_syringe_over;
            }
            else
            {
                message = "Collision: false";
                tool_syringe = Content.Load<Texture2D>("Images/tool_syringe");
            }
            //If the cursor is over the Image for the hand activate the "over" highlighted image
            if (handHit == true)
            {
                message = "Collision: true";
                tool_hand = tool_hand_over;
            }
            else
            {
                message = "Collision: false";
                tool_hand = Content.Load<Texture2D>("Images/tool_hand");
            }

        }

        private void toolCollisionOccured()
        {
            // Create a rectangle for the red Cursor and update the position relative to the position of the Users Right hand.
            cursorRect = new Rectangle((int)cursorPosition.X, (int)cursorPosition.Y, cursor.Width, cursor.Height);

            // Create a rectangle and set is position.
            scalpalRect = new Rectangle((int)scalpal_pos.X, (int)scalpal_pos.Y, tool_scalpal.Width, tool_scalpal.Height);
            suctionRect = new Rectangle((int)suction_pos.X, (int)suction_pos.Y, tool_suction.Width, tool_suction.Height);
            syringeRect = new Rectangle((int)syringe_pos.X, (int)syringe_pos.Y, tool_syringe.Width, tool_syringe.Height);
            handRect = new Rectangle((int)hand_pos.X, (int)hand_pos.Y, tool_hand.Width, tool_hand.Height);

            // Collision Detection.
            collision = new CollisionDetection();
            //Check for collision between the Cursor and the Scalpal
            if (collision.IntersectPixel(cursorRect, cursor_data, scalpalRect, scalpal_data))
            {
                scalpalHit = true;
                if (speech.returnMsg() == ": Scalpal Recognized!")
                {
                    currentState = cursorState.Scalpal;
                }                  
            }
            else
            {
                scalpalHit = false;
            }
            //Check for collision between the Cursor and the Suction
            if (collision.IntersectPixel(cursorRect, cursor_data, suctionRect, suction_data))
            {
                suctionHit = true;
                if (speech.returnMsg() == ": Suction Recognized!")
                {
                    currentState = cursorState.Suction;
                }
            }
            else
            {
                suctionHit = false;
            }
            //Check for collision between the Cursor and the syringe
            if (collision.IntersectPixel(cursorRect, cursor_data, syringeRect, syringe_data))
            {
                syringeHit = true;
                if (speech.returnMsg() == ": Syringe Recognized!")
                {
                    currentState = cursorState.Syringe;
                }
            }
            else
            {
                syringeHit = false;
            }
            //Check for collision between the Cursor and the hand
            if (collision.IntersectPixel(cursorRect, cursor_data, handRect, hand_data))
            {
                handHit = true;
                if (speech.returnMsg() == ": Hand Recognized!")
                {
                    currentState = cursorState.Hand;
                }
            }
            else
            {
                handHit = false;
            }
        }
    }
}