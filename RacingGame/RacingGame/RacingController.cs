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
        //Constants
        //Forward
        const float MAX_SPEED = 1000f;
        const float SPEED_INCREASE = 0.3f;//2
        //Backward
        const float BRAKE_SPEED = 0.1f;
        const float MIN_SPEED = -2f;
        const float SPEED_REDUCTION = 1.01f;
        //Turning
        const float MAX_TURNING = 0.5f;
        const float TURNING_REDUCTION = 1.2f;
        const float TURNING_CONSTANT = 0.3f;
        const float TURN_SPEED = 0.1f;
        //Overall
        const float ON_GRASS_MODIFIER = 100;//20
        const int TIMER_RESET = 100;
        const int CAR_MINIMUM_DISTANCE = 1000; //before Math.sqrt()
        const int UPDATE_RATE = 10; //number of miliseconds between each update

        //Image related variables
        Camera camera;
        Color[] pixelColour = new Color[1];
        Color[,] color2d = new Color[2000, 2000];
        GraphicsDevice graphicsDevice;
        SpriteFont font;
        Texture2D yellowPixelTexture;
        Texture2D trackTexture;
        Viewport viewport;

        //Cars
        List<CarVariables> carVariablesList = new List<CarVariables>();
        List<ICar> carList;
        
        //Others
        int nCars = 0;
        int cumulativeTime = 0;
        int timer = 0;
        
        public RacingController(  SpriteFont newFont,  Texture2D newYellowPixelTexture) 
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
            // Ensure that the game updates constantly at the same rate (About 100 FPS)
            if (gameTime.ElapsedGameTime.Milliseconds + cumulativeTime < UPDATE_RATE)
                cumulativeTime += gameTime.ElapsedGameTime.Milliseconds;
            else
            {
                cumulativeTime = 0;
                foreach (ICar car in carList) //Update Logic for each car
                {
                    carVariablesList[car.id].lapTime += gameTime.ElapsedGameTime.Milliseconds; //Update Lap time

                    //Only allow players to perform updates each 0.01s to prevent overloading the computer
                    timer += gameTime.ElapsedGameTime.Milliseconds;
                    if (timer > TIMER_RESET)
                    {
                        timer = 0;
                        car.Update(gameTime, carVariablesList[car.id]);
                    }

                    carVariablesList[car.id].onRoad = CheckOnRoad(car.id);

                    UpdateCalculus(car);
                    CheckCrossingFinishLine(car);
                    CheckWallCrash(car);
                    CollisionHandling(car);
                }
                //Update Camera Position
                camera.Update(gameTime, carVariablesList[0].positionX, carVariablesList[0].positionY);
            }
        }

        public void UpdateCalculus(ICar car)
        {
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
        }

        public void CheckCrossingFinishLine(ICar car)
        {
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
        }

        public void CheckWallCrash(ICar car)
        {
            carVariablesList[car.id].onWall = CheckonWall(car.id);
            if (carVariablesList[car.id].onWall)
            {
                carVariablesList[car.id].positionX -= carVariablesList[car.id].speedX;
                carVariablesList[car.id].positionY -= carVariablesList[car.id].speedY;
                carVariablesList[car.id].normalSpeed = 0;
            }
        }

        public void CollisionHandling(ICar car)
        {
            foreach (ICar otherCar in carList)
            {
                if (otherCar.id != car.id) //if not the same car
                {
                    //Check if touching each other (within CAR_MINIMUM_DISTANCE)
                    int x1 = (int)carVariablesList[car.id].positionX;
                    int x2 = (int)carVariablesList[otherCar.id].positionX;
                    int y1 = (int)carVariablesList[car.id].positionY;
                    int y2 = (int)carVariablesList[otherCar.id].positionY;
                    int carsDistance = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);

                    if (carsDistance < CAR_MINIMUM_DISTANCE)
                    { //it there's impact
                        //roll back the move for each car
                        carVariablesList[car.id].positionX -= carVariablesList[car.id].speedX;
                        carVariablesList[car.id].positionY -= carVariablesList[car.id].speedY;
                        carVariablesList[otherCar.id].positionX -= carVariablesList[otherCar.id].speedX;
                        carVariablesList[otherCar.id].positionY -= carVariablesList[otherCar.id].speedY;

                        //Exchange velocities
                        float tempX;
                        float tempY;

                        tempX = carVariablesList[car.id].speedX;
                        carVariablesList[car.id].speedX = carVariablesList[otherCar.id].speedX;
                        carVariablesList[otherCar.id].speedX = tempX;
                        tempY = carVariablesList[car.id].speedY;
                        carVariablesList[car.id].speedY = carVariablesList[otherCar.id].speedY;
                        carVariablesList[otherCar.id].speedY = tempY;

                        //Update new position
                        carVariablesList[car.id].positionX += carVariablesList[car.id].speedX;
                        carVariablesList[car.id].positionX += carVariablesList[car.id].speedX;
                        carVariablesList[otherCar.id].positionY += carVariablesList[otherCar.id].speedY;
                        carVariablesList[otherCar.id].positionY += carVariablesList[otherCar.id].speedY;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Initialize drawing with Camera
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform); 

            spriteBatch.Draw(trackTexture, new Vector2(-400,0), Color.White); //Background
            spriteBatch.Draw(yellowPixelTexture, new Rectangle(1312, 600, 151, 8), Color.White); //finish line

            //Draw each car
            DrawCars(spriteBatch);

            //Write Text
            DrawText(spriteBatch);
        }

        public void DrawCars(SpriteBatch spriteBatch)
        {
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
        }

        public void DrawText(SpriteBatch spriteBatch)
        {
            // Lap
            spriteBatch.DrawString(font, "Lap : " + carVariablesList[0].currentLap, new Vector2(carVariablesList[0].positionX + 160, carVariablesList[0].positionY + 180), Color.Red);
            //Velocity
            spriteBatch.DrawString(font, "Speed: " + ((int)(carVariablesList[0].normalSpeed * 40)).ToString() + " km/h", new Vector2(carVariablesList[0].positionX - 350, carVariablesList[0].positionY - 220), Color.Red);
            //Time
            int lapMilisecond = carVariablesList[0].lapTime % 1000;
            int lapMinute = (int)carVariablesList[0].lapTime / 60000;
            int lapSecond = (int)carVariablesList[0].lapTime / 1000 - lapMinute * 60;
            spriteBatch.DrawString(font, "Lap time: " + lapMinute + " m " + lapSecond + " s " + lapMilisecond, new Vector2(carVariablesList[0].positionX + 160, carVariablesList[0].positionY + 200), Color.Red);

            if (carVariablesList[0].countingLap > 1) //if has best time
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
            try{
                if (color2d[(int)carVariablesList[id].positionX + 400, (int)carVariablesList[id].positionY].A < 10) //Not on track
                    isOnRoad = false;
                else
                    isOnRoad = true;
            }
            catch { isOnRoad = false; }

            return isOnRoad;
        }

        private bool CheckonWall(int id)
        {
            bool onWall;
            try{
                if (color2d[(int)carVariablesList[id].positionX + 400, (int)carVariablesList[id].positionY].B < 200) //On wall
                    onWall = false;
                else
                    onWall = true;
            } 
            catch { onWall = false;}

            return onWall;
        }
    }
}
