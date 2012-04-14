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
        SpriteFont lucidaFont,arialFont;
        Vector2 cursorPosition,prevCursorPos,font_pos, scalpal_pos, suction_pos,syringe_pos,hand_pos;
        Vector2 currentToolPos,uHandPos,uScalPos,uSyrPos,uSucPos;
        Vector2 xMarkPos,heartCutPos,cutStartPos, cutEndPos;
        Texture2D cursor,rgb_bg, textBuffer_bg,selectedTool, consoleBg,accBg;
        Texture2D tool_scalpal, tool_scalpal_over;
        Texture2D tool_suction, tool_suction_over;
        Texture2D tool_syringe, tool_syringe_over;
        Texture2D tool_hand, tool_hand_over;
        Texture2D currentTool,uhand,uScalpal,uSuction,uSyringe,brush,heart,xMark,heartCut;
        Texture2D cutStart, cutEnd;
        Rectangle scalpalRect, suctionRect, syringeRect, handRect,cursorRect;
        Rectangle xMarkRect,heartCutRect,cutStartRect,cutEndRect;
        CollisionDetection collision;
        SpeechRec speech;
        Color[] cursor_data,scalpal_data,suction_data,syringe_data,hand_data;
        Color[] xMark_data,heartCutData,cutStartData,cutEndData;
        LoadKinect loadKinect;
        cursorState currentState = cursorState.Hand;
        
        int currentX, currentY, prevPosX, prevPosY;
        public List<Vector2> coordinates { get; set; }
        public List<Vector2> coorTemp { get; set; }

        bool drawline = false;

        string collisionMessage = "Collision: false";
        string currentStateMsg = "";
        string mainMessage = "Welcome to Surgery 101, We must first administer a drug. \nTell the kinect to change to a syringe by saying 'Nurse Syringe'\nGuide the syringe over the area marked X and say 'Inject' ";

        double score = 100;

        // Collision detection boolean
        bool scalpalHit = false;
        bool suctionHit = false;
        bool syringeHit = false;
        bool handHit = false;

        bool xMarkHit = false;
        bool cutting = false;
        bool consoleVisible = true;

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

            xMarkPos = new Vector2(290, 190);
            cutStartPos = new Vector2(490, 270);
            cutEndPos = new Vector2(260, 260);
            heartCutPos = new Vector2(250, 70);

            nui = Runtime.Kinects[0];
            loadKinect = new LoadKinect(graphics);            
            loadKinect.loadKinectNui(nui);          
            loadKinect.smoothKinectNui(nui);

            speech = new SpeechRec();
            speech.initSpeech();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // New SpriteBatch Object for Texture's and fonts.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load the font.
            lucidaFont = Content.Load<SpriteFont>("Font/lucidaConsole");
            arialFont = Content.Load<SpriteFont>("Font/arial");
            // load the texture object to hold the Kinect Video.
            loadKinect.loadKinectVideo(GraphicsDevice, 640, 480);
            // load the reddot image for the cursor.
            cursor = Content.Load<Texture2D>("Images/cursor");
            consoleBg = Content.Load<Texture2D>("Images/consoleBg");
            accBg = Content.Load<Texture2D>("Images/accuracyBg");

            xMark = Content.Load<Texture2D>("images/xMark");
            heartCut = Content.Load<Texture2D>("images/heart_cut");
            cutStart = Content.Load<Texture2D>("images/heartCutStart");
            cutEnd = Content.Load<Texture2D>("images/heartCutEnd");

            currentTool = Content.Load<Texture2D>("Images/useable_hand");

            uhand = Content.Load<Texture2D>("Images/useable_hand");
            uSuction = Content.Load<Texture2D>("Images/useable_suction");
            uSyringe = Content.Load<Texture2D>("Images/useable_syringe");
            uScalpal = Content.Load<Texture2D>("Images/useable_scalpal");

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

            xMark_data = new Color[xMark.Width * xMark.Height];
            heartCutData = new Color[heartCut.Width * heartCut.Height];
            cutEndData = new Color[cutEnd.Width * cutEnd.Height];
            cutStartData = new Color[cutStart.Width * cutStart.Height];
            // pass the cursor object the color data (in this case red pixels and transparent pixels)

            cursor.GetData(cursor_data);
            tool_suction.GetData(suction_data);
            tool_scalpal.GetData(scalpal_data);
            tool_syringe.GetData(syringe_data);
            tool_hand.GetData(hand_data);

            xMark.GetData(xMark_data);
            heartCut.GetData(heartCutData);
            cutEnd.GetData(cutEndData);
            cutStart.GetData(cutStartData);

            // Used for the draw function
            brush = Content.Load<Texture2D>("images/brush");
            heart = Content.Load<Texture2D>("images/heart");
            
            coordinates = new List<Vector2>();
        }

        protected override void UnloadContent()
        {
            nui.Uninitialize();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Update the position of Position by getting the updated value from kinectInit.cs.
            UpdateCursor();

            if (speech.returnMsg() == ": begin incision")
            {
                if (cutting)
                {
                    coordinates.Add(new Vector2(prevPosX, prevPosY));
                }
            }

            if (speech.returnMsg() == ": show console")
            {
                consoleVisible = true;
            }
            else if (speech.returnMsg() == ": hide console")
            {
                consoleVisible = false;
            }

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
           
            spriteBatch.Draw(tool_scalpal, scalpal_pos, Color.White);
            spriteBatch.Draw(tool_suction, suction_pos, Color.White);
            spriteBatch.Draw(tool_syringe, syringe_pos, Color.White);
            spriteBatch.Draw(tool_hand, hand_pos, Color.White);
            spriteBatch.DrawString(arialFont, currentStateMsg, new Vector2(565, 465), Color.AliceBlue);
            spriteBatch.DrawString(arialFont, mainMessage, new Vector2(25, 525), Color.AliceBlue);
            spriteBatch.Draw(accBg, new Vector2(320, 22), Color.White);
            spriteBatch.DrawString(arialFont, "Accuracy: "+score.ToString("0.00"), new Vector2(340, 28), Color.AliceBlue); 
            spriteBatch.Draw(heart, new Vector2(250, 70), Color.White);

            if (!xMarkHit)
            {
                spriteBatch.Draw(xMark, xMarkPos, Color.White);
            }
            else if (xMarkHit)
            {
                spriteBatch.Draw(heartCut, heartCutPos, Color.White);
                spriteBatch.Draw(cutStart, cutStartPos, Color.White);
                spriteBatch.Draw(cutEnd, cutEndPos, Color.White);
            }
            
            spriteBatch.Draw(cursor, cursorPosition, Color.White);
            spriteBatch.Draw(currentTool, currentToolPos, Color.White);

            if (consoleVisible == true)
            {                
                spriteBatch.Draw(consoleBg, new Vector2(22,22), Color.White);
                spriteBatch.DrawString(lucidaFont, collisionMessage, font_pos = new Vector2(27, 28), Color.White);
                spriteBatch.DrawString(lucidaFont, "Speech Recognition" + speech.returnMsg(), font_pos = new Vector2(27, 42), Color.White);
            }
            foreach (Vector2 vect in coordinates)
            {
                // Vector2's values are that of type float, (int) is used to cast them as integers
                if (vect.X != 0 && vect.Y != 0)
                {
                    spriteBatch.Draw(brush, new Rectangle((int)vect.X, (int)vect.Y, 10, 10), Color.White);
                }
            }
     
            if (currentX != 0 && currentY != 0)
            {
                spriteBatch.Draw(cursor, new Rectangle(prevPosX, prevPosY, 4, 4), Color.White);
            }
            spriteBatch.Draw(selectedTool, new Vector2(560, 460), Color.White);
            toolCollisionUpdateImage();
            nurseSelectTool();

            switch (currentState)
            {
                case cursorState.Hand:
                    currentStateMsg = "Hand";
                    currentTool = uhand;
                    currentToolPos = uHandPos;
                    break;
                case cursorState.Syringe:
                    currentStateMsg = "Syringe";
                    currentTool = uSyringe;
                    currentToolPos = uSyrPos;
                    break;
                case cursorState.Suction:
                    currentStateMsg = "Suction";
                    currentTool = uSuction;
                    currentToolPos = uSucPos;
                    break;
                case cursorState.Scalpal:
                    currentStateMsg = "Scalpal";
                    currentTool = uScalpal;
                    currentToolPos = uScalPos;
                    break;
            };
            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected void UpdateCursor()
        {
            prevCursorPos = cursorPosition;

            cursorPosition = loadKinect.Pos;

            // useable Hand position is set to the center of the hand image which is 68*90 pixels
            uHandPos.X = cursorPosition.X - 34;
            uHandPos.Y = cursorPosition.Y - 45;

            uScalPos.X = cursorPosition.X;
            uScalPos.Y = cursorPosition.Y - 60;

            uSyrPos.X = cursorPosition.X;
            uSyrPos.Y = cursorPosition.Y - 79;

            uSucPos.X = cursorPosition.X;
            uSucPos.Y = cursorPosition.Y - 36;

            currentX = (int)cursorPosition.X;
            currentY = (int)cursorPosition.Y;

            prevPosX = (int)prevCursorPos.X;
            prevPosY = (int)prevCursorPos.Y;
        }

        private void nurseSelectTool()
        {
            if (speech.returnMsg() == ": nurse scalpal")
            {
                currentState = cursorState.Scalpal;
            }
            else if (speech.returnMsg() == ": nurse suction")
            {
                currentState = cursorState.Suction;
            }
            else if (speech.returnMsg() == ": nurse syringe")
            {
                currentState = cursorState.Syringe;
            }
            else if (speech.returnMsg() == ": nurse hand")
            {
                currentState = cursorState.Hand;
            }
        }
       
        private void toolCollisionUpdateImage()
        {
            //If the cursor is over the Image for the Scalpal activate the "over" highlighted image
            if (scalpalHit == true)
            {
                collisionMessage = "Collision: true";
                tool_scalpal = tool_scalpal_over;
            }
            else
            {
                collisionMessage = "Collision: false";
                tool_scalpal = Content.Load<Texture2D>("Images/tool_scalpal");
            }
            //If the cursor is over the Image for the Suction activate the "over" highlighted image
            if (suctionHit == true)
            {
                collisionMessage = "Collision: true";
                tool_suction = tool_suction_over;
            }
            else
            {
                collisionMessage = "Collision: false";
                tool_suction = Content.Load<Texture2D>("Images/tool_suction");
            }
            //If the cursor is over the Image for the Syringe activate the "over" highlighted image
            if (syringeHit == true)
            {
                collisionMessage = "Collision: true";
                tool_syringe = tool_syringe_over;
            }
            else
            {
                collisionMessage = "Collision: false";
                tool_syringe = Content.Load<Texture2D>("Images/tool_syringe");
            }
            //If the cursor is over the Image for the hand activate the "over" highlighted image
            if (handHit == true)
            {
                collisionMessage = "Collision: true";
                tool_hand = tool_hand_over;
            }
            else
            {
                collisionMessage = "Collision: false";
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

            xMarkRect = new Rectangle((int)xMarkPos.X, (int)xMarkPos.Y, xMark.Width, xMark.Height);
            heartCutRect = new Rectangle((int)heartCutPos.X, (int)heartCutPos.Y, heartCut.Width, heartCut.Height);
            cutStartRect = new Rectangle((int)cutStartPos.X, (int)cutStartPos.Y, cutStart.Width, cutStart.Height);
            cutEndRect = new Rectangle((int)cutEndPos.X, (int)cutEndPos.Y, cutEnd.Width, cutEnd.Height);
                 
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
            ///
            // Collision with the Xmarks the spot image
            ///
            if (collision.IntersectPixel(cursorRect, cursor_data, xMarkRect, xMark_data))
            {
                if (currentState == cursorState.Syringe)
                {
                    if (speech.returnMsg() == ": inject")
                    {
                        mainMessage = "The Patient is now Sedated. Now select the Scalpal and cut along the \nwhite marker from Green to blue. Say 'begin Incision' when \nin green, and when at blue say 'end incision'";
                        xMarkHit = true;
                        currentState = cursorState.Hand;
                    }
                }
                else if (currentState != cursorState.Syringe)
                {
                    if (speech.returnMsg() == ": inject")
                    {
                        mainMessage = "Please Select the Syringe";
                        if (xMarkHit)
                        {
                            mainMessage = "The Patient is now Sedated. Now select the Scalpal and cut along the \nwhite marker from Green to blue. Say 'begin Incision' when \nin green, and when at blue say 'end incision'";
                        }
                    }
                }
            }
            else
            {
                if (currentState == cursorState.Syringe)
                {
                    if (speech.returnMsg() == ": inject")
                    {
                        mainMessage = "You injected in the wrong area (-10)";
                        score -= 10;
                        xMarkHit = true;
                        currentState = cursorState.Hand;
                    }
                }
            }

            ///
            // Collision with white broken line
            ///
            if (!collision.IntersectPixel(cursorRect, cursor_data, heartCutRect, heartCutData))
            {
                if (cutting)
                {
                    score -= 0.1;
                    
                }
            }
            if (collision.IntersectPixel(cursorRect, cursor_data, heartCutRect, heartCutData))
            {
                if (cutting)
                {
                    score = score;
                }
            }

            ///
            // Collision with green marker
            ///
            if (collision.IntersectPixel(cursorRect, cursor_data, cutStartRect, cutStartData))
            {
                if (xMarkHit)
                {
                    if (currentState == cursorState.Scalpal)
                    {
                        if (speech.returnMsg() == ": begin incision")
                        {
                            mainMessage = "cutting";
                            cutting = true;
                        }
                    }
                    else if (currentState != cursorState.Scalpal)
                    {
                        if (speech.returnMsg() == ": begin incision")
                        {
                            mainMessage = "Please Select the Scalpal";
                        }                      
                    }
                }
            }

            ///
            // Collision with blue marker
            ///
            if (collision.IntersectPixel(cursorRect, cursor_data, heartCutRect, heartCutData))
            {
                if (cutting)
                {
                    if (speech.returnMsg() == ": end incision")
                    {
                        mainMessage = "finished cutting";
                        cutting = false;                      
                    }
                }

            }

        }

        
    }
}