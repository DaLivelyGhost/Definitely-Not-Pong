using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Assignment_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer gameTimer = new DispatcherTimer();

        //Ball declarization-------------------
        private double gameBallTop;
        private double gameBallLeft;

        private double vertDirection;
        private double horizDirection;

        private double dx;
        private double dy;
        //-------------------------------------
        //Player Paddle declarization----------

        private double playerRectTop;
        private double playerRectLeft;

        private double playerRectDy;

        private bool PlayerMoveDown;
        private bool PlayerMoveUp;
        //-------------------------------------
        //Enemy paddle declarization-----------
        private double enemyRectTop;
        private double enemyRectLeft;

        private double enemyRectDy;

        private Random enemyAi;
        private int enemyNextStep;
        //-------------------------------------
        //Game flow declarization-------------- 
        int flavorColor;
        int ballHits;       //this will count the amount of times the ball has been hit
        int Score;          //Score. pretty self explanatory
        int highScore;
        int Lives;          //Lives. 
        bool active;    //Is the game in progress currently?
        //-------------------------------------
        //Secret declarization-----------------
        private Key[] keyStrokes = new Key[]{};
        private Key[] konamiCode = new Key[] { Key.Up, Key.Up, Key.Down, Key.Down, Key.Left, Key.Right, Key.Left, Key.Right, Key.B, Key.A, Key.Enter };
        private bool fartMode = false;
        private int konamiCount = 0;
        private SoundPlayer SmallFartPlayer = new SoundPlayer();
        private SoundPlayer ShortFartPlayer = new SoundPlayer();
        private SoundPlayer SizeableFartPlayer = new SoundPlayer();
        //-------------------------------------
        public MainWindow()
        {
            InitializeComponent();

            //easter egg initialization
            ShortFartPlayer.SoundLocation = @".\short_fart.wav";
            SmallFartPlayer.SoundLocation = @".\small_fart.wav";
            SizeableFartPlayer.SoundLocation = @".\sizeable_fart.wav";

            //initialize all components to their default values
            setDefaults();

            gameTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            gameTimer.Tick += GameTimer_Tick;

            gameBallTop = Canvas.GetTop(GameBall);
            gameBallLeft = Canvas.GetLeft(GameBall);

            playerRectTop = Canvas.GetTop(PlayerRect);
            playerRectLeft = Canvas.GetLeft(PlayerRect);

            enemyRectTop = Canvas.GetTop(EnemyRect);
            enemyRectLeft = Canvas.GetLeft(EnemyRect);

        }
        private void startGame()
        {
            GameBall.Visibility = Visibility.Visible;

            txtblk_flavor.Visibility = Visibility.Hidden;
            img_flavorBackground.Visibility = Visibility.Hidden;

            txtblk_title.Visibility = Visibility.Hidden;

            txtblk_paused.Visibility = Visibility.Hidden;

            txtblk_lives.Visibility = Visibility.Hidden;

            txtblk_controls.Visibility = Visibility.Hidden;
            txtblk_up.Visibility = Visibility.Hidden;
            txtblk_down.Visibility = Visibility.Hidden;
            txtblk_reminder.Visibility = Visibility.Hidden;

            txtblk_start.Visibility = Visibility.Hidden;

            active = true;
            gameTimer.IsEnabled = true;
        }
        //This is called between lives, or when the player scores a point on the opponent.
        //NOT on gameovers
        private void softReset()
        {
            gameTimer.IsEnabled = false;

            //reset pieces.

            active = false;

            gameBallTop = (PlayField.Height / 2) - GameBall.Height;
            gameBallLeft = (PlayField.Width / 2) - GameBall.Height;

            //redirect the ball away from who was just scored on
            horizDirection *= -1;

            Canvas.SetTop(GameBall, gameBallTop);
            Canvas.SetLeft(GameBall, gameBallLeft);

            txtblk_lives.Visibility = Visibility.Visible;

            txtblk_start.Visibility = Visibility.Visible;

        }
        //When the player loses all their lives.
        private void gameOver()
        {
            gameTimer.IsEnabled = false;
            active = false;

            setDefaults();

            gameBallTop = (PlayField.Height / 2) - GameBall.Height;
            gameBallLeft = (PlayField.Width / 2) - GameBall.Height;

            Canvas.SetTop(GameBall, gameBallTop);
            Canvas.SetLeft(GameBall, gameBallLeft);

            playerRectTop = Canvas.GetTop(PlayerRect);
            playerRectLeft = Canvas.GetLeft(PlayerRect);

            enemyRectTop = Canvas.GetTop(EnemyRect);
            enemyRectLeft = Canvas.GetLeft(EnemyRect);

            GameBall.Visibility = Visibility.Hidden;

            txtblk_flavor.Visibility = Visibility.Hidden;
            img_flavorBackground.Visibility = Visibility.Hidden;

            txtblk_title.Visibility = Visibility.Visible;

            txtblk_paused.Visibility = Visibility.Hidden;

            txtblk_lives.Visibility = Visibility.Hidden;

            txtblk_controls.Visibility = Visibility.Visible;
            txtblk_up.Visibility = Visibility.Visible;
            txtblk_down.Visibility = Visibility.Visible;
            txtblk_reminder.Visibility = Visibility.Visible;

            txtblk_start.Visibility = Visibility.Visible;
        }
        private void setDefaults()
        {
            //ball coordinates
            gameBallTop = 0;
            gameBallLeft = 0;
            //ball velocity direction
            vertDirection = 1;
            horizDirection = -1;
            //ball velocity
            dx = 2.8;
            dy = 2.8;

            //player coordinates
            PlayerRect.Height = 145;
            playerRectTop = 0;
            playerRectLeft = 0;
            //player velocity
            playerRectDy = 3;
            //player movement toggles
            PlayerMoveDown = false;
            PlayerMoveUp = false;

            //enemy coordinates
            EnemyRect.Height = 145;
            enemyRectTop = 0;
            enemyRectLeft = 0;
            //enemy velocity
            enemyRectDy = 3;
            //enemy ai
            enemyAi = new Random();


            //Game backend stuff
            flavorColor = 0;
            ballHits = 0;
            Score = 0;
            Lives = 2;
            active = false;
            txtblk_lives.Text = "Lives: " + Lives;

            //Colors
            PlayerRect.Stroke = Brushes.White;
            PlayerRect.Fill = Brushes.White;
            EnemyRect.Stroke = Brushes.White;
            EnemyRect.Fill = Brushes.White;
            GameBall.Stroke = Brushes.White;
            GameBall.Fill = Brushes.White;
        }
        private void incrementScore(int toIncrement) {
            Score += toIncrement;
            if(Score > highScore)
            {
                highScore = Score;
            }
        }

    private void enemyMovement()
        {
            //The enemy paddle will track the ball as long as it's on the left-ish side of the stage.
            if (gameBallLeft < (PlayField.Width / 2))
            {
                if ((gameBallTop < (EnemyRect.Height / 2) + enemyRectTop) && enemyRectTop >= 0)
                {
                    enemyRectTop -= enemyRectDy;
                }
                if ((gameBallTop > (EnemyRect.Height / 2) + enemyRectTop) && enemyRectTop + enemyRectDy + EnemyRect.Height <= PlayField.Height)
                {
                    enemyRectTop += enemyRectDy;
                }
            }
            //once the ball is on the right side
            else
            {
                if (enemyNextStep > 3)  //there is a high chance that the ai will go for the ball. 
                {
                    if ((gameBallTop < (EnemyRect.Height / 2) + enemyRectTop) && enemyRectTop >= 0)
                    {
                        enemyRectTop -= enemyRectDy;
                    }
                    if ((gameBallTop > (EnemyRect.Height / 2) + enemyRectTop) && enemyRectTop + enemyRectDy + EnemyRect.Height <= PlayField.Height)
                    {
                        enemyRectTop += enemyRectDy;
                    }
                }
                else if (enemyNextStep == 3) //there is a small chance the ai will miss.
                {
                    if (enemyRectTop >= 0)
                    {
                        enemyRectTop -= enemyRectDy;
                    }

                }
                else if(enemyNextStep == 2)
                {
                    if (enemyRectTop + enemyRectDy + EnemyRect.Height <= PlayField.Height)
                    {
                        enemyRectTop += enemyRectDy;
                    }
                }
                else if (enemyNextStep == 1) ; //chance the enemy will lock up
            }
        }
        //------------------------------------
        //Events
        //------------------------------------

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            //If the player is moving.
            //I moved the player movement out of the key down event into the timer event
            //because I hated how choppy it was.
            if(PlayerMoveDown == true || PlayerMoveUp == true) //consolidated into 1 if statement in an effort to cut down on if statements when not moving.
            {
                if(PlayerMoveDown == true)
                {
                    if (playerRectTop + playerRectDy + PlayerRect.Height <= PlayField.Height)
                    {
                        playerRectTop += playerRectDy;
                    }
                }
                if(PlayerMoveUp == true)
                {
                    if (playerRectTop >= 0)
                    {
                        playerRectTop -= playerRectDy;
                    }
                }
            }

            enemyMovement();

            //If a ball hits the ceiling
            if (gameBallTop <= 0 || gameBallTop >= (PlayField.Height - GameBall.Height))
            {
                vertDirection *= -1;

                if(fartMode == true)
                {
                    ShortFartPlayer.Play();
                }
            }
            //if the enemy misses
            if ((gameBallLeft >= PlayField.Width - GameBall.Width))
            {
                if (fartMode == true)
                {
                    SizeableFartPlayer.Play();
                }

                incrementScore(50 * (flavorColor + 1)); //score multiplier is actually 1 less than advertised, hence the + 1
                softReset();
            }
            //If the enemy hits the ball
            if (gameBallLeft + GameBall.Width >= (enemyRectLeft) &&
                gameBallTop + GameBall.Height / 2 >= enemyRectTop &&
                gameBallTop + GameBall.Height / 2 <= enemyRectTop + EnemyRect.Height)
            {
                horizDirection *= -1;

                if (fartMode == true)
                {
                    SmallFartPlayer.Play();
                }
            }
            //if the player misses (booo)
            if (gameBallLeft <= 0)
            {
                if(fartMode == true)
                {
                    SizeableFartPlayer.Play();
                }

                if (Lives != 0) //if they have more lives, soft reset
                {
                    txtblk_lives.Text = "Lives: " + Lives;
                    Lives--;
                    softReset();
                }
                else
                {   //if the player loses all their lives, hard reset.
                    gameOver();
                }
            }

            //if the player hits the ball (good job)
            if(gameBallLeft <= (playerRectLeft + PlayerRect.Width) && 
                gameBallTop + GameBall.Height /2 >= playerRectTop && 
                gameBallTop + GameBall.Height / 2 <= playerRectTop + PlayerRect.Height)
            {

                incrementScore(10 * (flavorColor + 1)); //score multiplier is actually 1 less than advertised, hence the + 1
                horizDirection *= -1;
                ballHits++;
                enemyNextStep = enemyAi.Next(1, 11); //determine the next ai movement.

                if (fartMode == true)
                {
                    SmallFartPlayer.Play();
                }

                //speed up the ball and paddle every 3rd hit
                if (ballHits % 3 == 0)
                {
                    dx += 1.2;
                    dy += 1.2;

                    playerRectDy += 1.5;
                    enemyRectDy += 1.0; //enemy paddle gets faster, but not as fast. This is so that eventually they will guaranteed miss.
                }
                //Combo text for flavor
                //and that flavor is zest
                if(ballHits % 4 == 0)
                {
                    if(PlayerRect.Height > 80) //cap off the amount that the paddles and ball can shrink
                    {
                        PlayerRect.Height = PlayerRect.Height - 10;
                        EnemyRect.Height = EnemyRect.Height - 10;
                        GameBall.Height = GameBall.Height - 2;
                        GameBall.Width = GameBall.Width - 2;

                        flavorColor++;  //I only want the game's color to change when the paddle shrinks
                    }

                    img_flavorBackground.Visibility = Visibility.Visible;
                    txtblk_flavor.Visibility = Visibility.Visible;

                    PlayerRect.Fill = Brushes.Black;
                    EnemyRect.Fill = Brushes.Black;
                    GameBall.Fill = Brushes.Black;

                    switch(flavorColor)
                    {
                        case 1:
                            txtblk_flavor.Text = "x" + (flavorColor + 1);
                            txtblk_flavor.Foreground = Brushes.Purple;
                            PlayerRect.Stroke = Brushes.Purple;
                            EnemyRect.Stroke = Brushes.Purple;
                            GameBall.Stroke = Brushes.Purple;
                            break;
                        case 2:
                            txtblk_flavor.Text = "x" + (flavorColor + 1);
                            txtblk_flavor.Foreground = Brushes.Cyan;
                            PlayerRect.Stroke = Brushes.Cyan;
                            EnemyRect.Stroke = Brushes.Cyan;
                            GameBall.Stroke = Brushes.Cyan;
                            break;
                        case 3:
                            txtblk_flavor.Text = "x" + (flavorColor + 1);
                            txtblk_flavor.Foreground = Brushes.LimeGreen;
                            PlayerRect.Stroke = Brushes.LimeGreen;
                            EnemyRect.Stroke = Brushes.LimeGreen;
                            GameBall.Stroke = Brushes.LimeGreen;
                            break;
                        case 4:
                            txtblk_flavor.Text = "x" + (flavorColor + 1);
                            txtblk_flavor.Foreground = Brushes.Yellow;
                            PlayerRect.Stroke = Brushes.Yellow;
                            EnemyRect.Stroke = Brushes.Yellow;
                            GameBall.Stroke = Brushes.Yellow;
                            break;
                        case 5:
                            txtblk_flavor.Text = "x" + (flavorColor + 1);
                            txtblk_flavor.Foreground = Brushes.Orange;
                            PlayerRect.Stroke = Brushes.Orange;
                            EnemyRect.Stroke = Brushes.Orange;
                            GameBall.Stroke = Brushes.Orange;
                            break;
                        case 6:
                            txtblk_flavor.Text = "x" + (flavorColor + 1);
                            txtblk_flavor.Foreground = Brushes.Red;
                            PlayerRect.Stroke = Brushes.Red;
                            EnemyRect.Stroke = Brushes.Red;
                            GameBall.Stroke = Brushes.Red;
                            break;
                    }
                }
            }
            txtblk_scoreamount.Text = Score.ToString();
            txtblk_highscoreamount.Text = highScore.ToString();

            gameBallLeft += dx * horizDirection;
            gameBallTop += dy * vertDirection;

            Canvas.SetTop(GameBall, gameBallTop);
            Canvas.SetLeft(GameBall, gameBallLeft);

            Canvas.SetTop(PlayerRect, playerRectTop);
            Canvas.SetTop(EnemyRect, enemyRectTop);
        }

        private void gameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (active == false)//if the game is not in progress
            {
                if (e.Key == Key.Space)
                {
                    startGame();
                }
                //keep track if the player is properly inputting the konami code
                //on the main menu.
                else if(e.Key.Equals(konamiCode[konamiCount]))
                {
                    konamiCount++;
                    if(konamiCount == 11)
                    {
                        fartMode = true;
                        konamiCount = 0;
                    }
                }
                else
                {
                    konamiCount = 0;
                }
                
            }
            else
            {
                if(e.Key == Key.Space)
                {
                    if (gameTimer.IsEnabled)
                    {
                        txtblk_paused.Visibility = Visibility.Visible;
                        gameTimer.Stop();
                    }
                    else
                    {
                        txtblk_paused.Visibility = Visibility.Hidden;
                        gameTimer.Start();
                    }
                }
                else
                {
                    if (e.Key == Key.Down)
                    {
                        PlayerMoveDown = true;
                    }
                    else if (e.Key == Key.Up)
                    {
                        PlayerMoveUp = true;
                    }

                }
            }
        }

        private void gameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Down)
            {
                PlayerMoveDown = false;
            }
            if(e.Key == Key.Up)
            {
                PlayerMoveUp = false;
            }
        }
        private void mnuitm_start_Click(object sender, RoutedEventArgs e)
        {
            if (active == false) //if the game is not in progress
            {
                startGame();
            }
            else
            {
                txtblk_paused.Visibility = Visibility.Hidden;
                gameTimer.Start();
            }
        }
        private void mnuitm_pause_Click(object sender, RoutedEventArgs e)
        {
            if (gameTimer.IsEnabled)
            {
                txtblk_paused.Visibility = Visibility.Visible;
                gameTimer.Stop();
            }
        }
        private void mnuitm_quit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void mnuitm_help_Click(object sender, RoutedEventArgs e)
        {
            helpWindow help = new helpWindow();

            help.Show();
        }
    }
}
