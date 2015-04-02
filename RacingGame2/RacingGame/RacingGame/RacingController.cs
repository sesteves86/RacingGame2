using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RacingGame
{
    class RacingController
    {
        //Constants for racing calculus
        //Forward
        const float MAX_SPEED = 1000f;
        const float SPEED_INCREASE = 0.3f;//2
        //Backward
        const float BRAKE_SPEED = 0.1f;
        const float MIN_SPEED = -2f;
        const float SPEED_REDUCTION = 1.01f;
        //Turning
        float MAX_TURNING = 0.5f;
        const float TURNING_REDUCTION = 1.2f;
        const float TURNING_CONSTANT = 0.3f;
        const float TURN_SPEED = 0.1f;
        //Overall
        const float ON_GRASS_MODIFIER = 100;//20

        //Image related variables
        Viewport viewport;
        SpriteFont font;
        Camera camera;
        Texture2D yellowPixelTexture;
        Texture2D trackTexture;
        Color[] pixelColour = new Color[1];
        GraphicsDevice graphicsDevice;
        Color[,] color2d = new Color[2000, 2000];

        //Controller specific variables
        int nCars = 0;
        int cumulativeTime = 0;
        int timer = 0;
        int TIMER_RESET=100;

        List<CarVariables> carVariablesList = new List<CarVariables>();
        List<ICar> carList;

        public RacingController(  SpriteFont newFont,  Texture2D newYellowPixelTexture) //Texture2D newTexture, , 
        {
            yellowPixelTexture = newYellowPixelTexture;

            carList = new List<ICar>();

            //Initializing other variables
            font = newFont;
        }

        public void AddTrack(GraphicsDevice newGraphicsDevice, Texture2D newTrackSprite)
        {
            //Trying to pass circuitTexture to Color[,] and then compare car position with color2D to do the onWall() and onGrass() checks
            trackTexture = newTrackSprite;

            graphicsDevice = newGraphicsDevice;
            viewport = graphicsDevice.Viewport;
            Color[] colorBackground = new Color[2000 * 2000];
            camera = new Camera(newGraphicsDevice.Viewport);
            trackTexture.GetData<Color>(colorBackground);
            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < 2000; j++)
                {
                    color2d[i, j] = colorBackground[i + j * 2000];
                }
            }
        }

        public void AddCar(Texture2D newTexture, ICar car)
        {
            nCars += 1;

            //Create new CarVariables variable and fill it up with default values
            CarVariables cv = new CarVariables();

            cv.id = nCars;
            cv.texture = newTexture;
            
            //movement
            cv.angle = 0;
            cv.turning = 0;
            cv.orientation = (float)(Math.PI / 2);
            cv.normalSpeed = 0;
            cv.speedX = 0;
            cv.speedY = 0;
            cv.positionX = 1400 + 50 * (nCars % 2);
            cv.positionY = 800 - 50 * (int)(nCars / 2);

            //lap
            cv.lapTime = 0;
            cv.bestLap = 50000;
            cv.countingLap = 1;
            cv.currentLap = 1;

            //other
            cv.crossingFinishLine = false;
            cv.onRoad = true;
            cv.onWall = false;

            carVariablesList.Add(cv);
            carList.Add(car);

            car.GetTrackMap(color2d);
        }

        public void Update(GameTime gameTime)
        {
            //Actualizar lap Time para cada carro
            foreach (var car in carVariablesList)
            {
                car.lapTime += gameTime.ElapsedGameTime.Milliseconds;
            }

            // Movement Logic
            // Ensure that the game updates constantly at the same rate (About 100 FPS)
            if (gameTime.ElapsedGameTime.Milliseconds + cumulativeTime < 10)
            {
                cumulativeTime += gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                cumulativeTime = 0;
                foreach (ICar car in carList) //Update Logic for each car
                {
                    //Only allow players to perform updates each 0.01s to prevent overloading the computer
                    timer += gameTime.ElapsedGameTime.Milliseconds;
                    if (timer > TIMER_RESET)
                    {
                        timer = 0;
                        car.Update(gameTime, carVariablesList[car.id]);
                    }

                    carVariablesList[car.id].onRoad = CheckOnRoad(car.id);
                    
                    #region Update calculus
                    if (car.IsAccelerating)
                    {
                        if (carVariablesList[car.id].normalSpeed < 0) //if accelarating while moving backwards
                        {
                            carVariablesList[car.id].normalSpeed += 1;
                        }
                        else
                        {
                            double tSpeed = Math.Exp(carVariablesList[car.id].normalSpeed);
                            float t1 = (float)Math.Log(tSpeed + SPEED_INCREASE);
                            float t2 = (float)Math.Log(tSpeed);
                            if (carVariablesList[car.id].onRoad)
                                carVariablesList[car.id].normalSpeed += (t1 - t2);
                            else
                                carVariablesList[car.id].normalSpeed += (t1 - t2) / ON_GRASS_MODIFIER;
                            if (carVariablesList[car.id].normalSpeed > MAX_SPEED)
                            {
                                carVariablesList[car.id].normalSpeed = MAX_SPEED;
                                //tSpeed = Math.Exp(normalSpeed);
                            }
                        }
                    }
                    else if (car.IsBreaking)
                    {
                        if (carVariablesList[car.id].onRoad)
                            carVariablesList[car.id].normalSpeed -= BRAKE_SPEED;
                        else
                            carVariablesList[car.id].normalSpeed -= BRAKE_SPEED / ON_GRASS_MODIFIER;

                        if (carVariablesList[car.id].normalSpeed < MIN_SPEED)
                            carVariablesList[car.id].normalSpeed = MIN_SPEED;
                    }
                    else //If not accelerating or breaking
                    {
                        double tSpeed = Math.Exp(carVariablesList[car.id].normalSpeed);
                        if (carVariablesList[car.id].normalSpeed < 0.001f)
                            carVariablesList[car.id].normalSpeed = 0;
                        else
                            carVariablesList[car.id].normalSpeed /= SPEED_REDUCTION;
                    }

                    if (car.IsTurningLeft)
                    {
                        if (carVariablesList[car.id].onRoad)
                        {
                            if (carVariablesList[car.id].normalSpeed < 0)
                                carVariablesList[car.id].turning += TURN_SPEED;
                            else
                                carVariablesList[car.id].turning -= TURN_SPEED;
                        }
                        else //on grass
                        {
                            if (carVariablesList[car.id].normalSpeed < 0)
                                carVariablesList[car.id].turning += TURN_SPEED / ON_GRASS_MODIFIER;
                            else
                                carVariablesList[car.id].turning -= TURN_SPEED / ON_GRASS_MODIFIER;
                        }
                    }
                    else if (car.IsTurningRight)
                    {
                        if (carVariablesList[car.id].onRoad)
                        {
                            if (carVariablesList[car.id].normalSpeed > 0)
                                carVariablesList[car.id].turning += TURN_SPEED;
                            else
                                carVariablesList[car.id].turning -= TURN_SPEED;
                        }
                        else
                        {
                            if (carVariablesList[car.id].normalSpeed > 0)
                                carVariablesList[car.id].turning += TURN_SPEED / ON_GRASS_MODIFIER;
                            else
                                carVariablesList[car.id].turning -= TURN_SPEED / ON_GRASS_MODIFIER;
                        }
                    }
                    else
                    {
                        carVariablesList[car.id].turning /= TURNING_REDUCTION;
                    }
                    //Don't allow to turn more than Maximum
                    if (carVariablesList[car.id].turning > MAX_TURNING)
                        carVariablesList[car.id].turning = MAX_TURNING;
                    else if (carVariablesList[car.id].turning < -MAX_TURNING)
                        carVariablesList[car.id].turning = -MAX_TURNING;

                    //Adjust turning to the orientation (rad) in function of the maximum speed
                    float fTemp;
                    if (Math.Abs(carVariablesList[car.id].normalSpeed) == 0f)//no turning
                        fTemp = 0;
                    else if (carVariablesList[car.id].normalSpeed > 0 && Math.Abs(carVariablesList[car.id].normalSpeed) < 2f)
                        fTemp = TURNING_CONSTANT / 4;
                    else if (carVariablesList[car.id].normalSpeed < 0 && Math.Abs(carVariablesList[car.id].normalSpeed) < 2f)
                        fTemp = TURNING_CONSTANT / -4;
                    else
                        fTemp = TURNING_CONSTANT / (carVariablesList[car.id].normalSpeed * carVariablesList[car.id].normalSpeed);

                    carVariablesList[car.id].orientation += carVariablesList[car.id].turning * fTemp;

                    //Adjust speed variation due to turning and new speed
                    carVariablesList[car.id].angle = (float)Math.Cos((double)carVariablesList[car.id].orientation);
                    carVariablesList[car.id].speedX = carVariablesList[car.id].normalSpeed * carVariablesList[car.id].angle;
                    carVariablesList[car.id].angle = (float)Math.Sin((double)carVariablesList[car.id].orientation);
                    carVariablesList[car.id].speedY = carVariablesList[car.id].normalSpeed * carVariablesList[car.id].angle;

                    //Update position
                    //lastPosition = position;
                    carVariablesList[car.id].positionX += carVariablesList[car.id].speedX;
                    carVariablesList[car.id].positionY += carVariablesList[car.id].speedY;
                    #endregion

                    #region Check if crossing the finish line
                    if (carVariablesList[car.id].positionX > 1300 && carVariablesList[car.id].positionX < 2000 &&
                        carVariablesList[car.id].positionY > 600 && carVariablesList[car.id].positionY < 620)
                    {
                        if (!carVariablesList[car.id].crossingFinishLine) //check if entering the area and not already in there
                        {
                            carVariablesList[car.id].crossingFinishLine = true;

                            if (carVariablesList[car.id].positionY < 610) // entering from above
                            {
                                carVariablesList[car.id].currentLap += 1;
                                if (carVariablesList[car.id].currentLap > carVariablesList[car.id].countingLap)
                                {
                                    carVariablesList[car.id].countingLap += 1;


                                    if (carVariablesList[car.id].lapTime < carVariablesList[car.id].bestLap)
                                        carVariablesList[car.id].bestLap = carVariablesList[car.id].lapTime;

                                    carVariablesList[car.id].lapTime = 0;
                                }
                            }
                            else //entering from below
                            {
                                carVariablesList[car.id].currentLap -= 1;
                            }
                        }
                    }
                    else
                    {
                        carVariablesList[car.id].crossingFinishLine = false;
                    }
                    #endregion

                    //If crashing against the wall
                    carVariablesList[car.id].onWall = CheckonWall(car.id);
                    if (carVariablesList[car.id].onWall)
                    {
                        carVariablesList[car.id].positionX -= carVariablesList[car.id].speedX;
                        carVariablesList[car.id].positionY -= carVariablesList[car.id].speedY;
                        carVariablesList[car.id].normalSpeed = 0;
                    }
                    
                }
            }

            //Update Camera Position
            camera.Update(gameTime, carVariablesList[0].positionX, carVariablesList[0].positionY );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform); //Initialize drawing with Camera
            spriteBatch.Draw(trackTexture, new Vector2(-400,0), Color.White); //Background
            spriteBatch.Draw(yellowPixelTexture, new Rectangle(1312, 600, 151, 8), Color.White); //finish line

            foreach (ICar car in carList)
            {
                //Get car variables
                Vector2 origin = new Vector2(carVariablesList[car.id].texture.Width / 2, carVariablesList[car.id].texture.Height / 2);
                carVariablesList[car.id].angle = carVariablesList[car.id].orientation + (float)Math.PI / 2;

                //Draw Car
                spriteBatch.Draw(carVariablesList[car.id].texture, 
                    new Vector2(carVariablesList[car.id].positionX, carVariablesList[car.id].positionY), 
                    null, 
                    Color.White, 
                    carVariablesList[car.id].angle, 
                    origin, 
                    0.5f, 
                    SpriteEffects.None, 
                    0f);
            }
            //Write Text
            // Lap
            spriteBatch.DrawString(font, "Lap : " + carVariablesList[0].currentLap, new Vector2(carVariablesList[0].positionX + 160, carVariablesList[0].positionY + 180), Color.Red);
            //Velocity
            spriteBatch.DrawString(font, "Speed: " + ((int)(carVariablesList[0].normalSpeed * 40)).ToString() + " km/h", new Vector2(carVariablesList[0].positionX - 350, carVariablesList[0].positionY - 220), Color.Red);
            //Time
            int lapMilisecond = carVariablesList[0].lapTime % 1000;
            int lapMinute = (int)carVariablesList[0].lapTime / 60000;
            int lapSecond = (int)carVariablesList[0].lapTime / 1000 - lapMinute * 60;
            spriteBatch.DrawString(font, "Lap time: " + lapMinute + " m " + lapSecond + " s " + lapMilisecond, new Vector2(carVariablesList[0].positionX + 160, carVariablesList[0].positionY + 200), Color.Red);

            if (carVariablesList[0].countingLap>1) //if has best time
            {
                int bestLapMilisecond = carVariablesList[0].bestLap % 1000;
                int bestLapMinute = (int)carVariablesList[0].bestLap / 60000;
                int bestLapSecond = (int)carVariablesList[0].bestLap / 1000 - bestLapMinute * 60;
                spriteBatch.DrawString(font, "Best lap: " + bestLapMinute + " m " + bestLapSecond + "s" + bestLapMilisecond, new Vector2(carVariablesList[0].positionX + 160, carVariablesList[0].positionY + 220), Color.Red);
            }
            else
            {
                spriteBatch.DrawString(font, "Best lap: - m -- s ---", new Vector2(carVariablesList[0].positionX + 160, carVariablesList[0].positionY + 220), Color.Red);
            }
        }

        private bool CheckOnRoad(int id)
        {
            bool isOnRoad;

            try
            {
                if (color2d[(int)carVariablesList[id].positionX + 400, (int)carVariablesList[id].positionY].A < 10) //Not on track
                    isOnRoad = false;
                else
                    isOnRoad = true;
            }
            catch
            { isOnRoad = false; }

            return isOnRoad;
        }

        private bool CheckonWall(int id)
        {
            bool onWall;

            try
            {
                //if (color2d[(int)positionX + 400 - texture.Width / 2 + 10, (int)positionY - texture.Height / 2 + 30].B < 200) //On wall
                if (color2d[(int)carVariablesList[id].positionX + 400, (int)carVariablesList[id].positionY].B < 200) //On wall
                    onWall = false;
                else
                    onWall = true;
            }
            catch
            {
                onWall = false;
            }

            return onWall;
        }
    }
}
