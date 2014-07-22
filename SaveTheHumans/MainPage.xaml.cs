using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Xaml.Media.Animation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace SaveTheHumans
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : SaveTheHumans.Common.LayoutAwarePage
    {
        Random random = new Random(); //Random to randomize target position
        DispatcherTimer enemyTimer = new DispatcherTimer(); //timer to spawn enemies
        DispatcherTimer targetTimer = new DispatcherTimer(); // timer to complete the level
        bool humanCapped = false; // is human captured?

        public MainPage() // main game logic
        {
            this.InitializeComponent(); 

            enemyTimer.Tick += enemyTimer_Tick; //when the timer ticks, execute enemyTimer_Tick
            enemyTimer.Interval = TimeSpan.FromSeconds(2); //spawn new enemy every two seconds

            targetTimer.Tick += targetTimer_Tick; //when ticks (every .1 second) it executes targetTimer_Tick
            targetTimer.Interval = TimeSpan.FromSeconds(.1); //tick every .1 second
        }

        void targetTimer_Tick(object sender, object e)
        {
            progressBar.Value += 1;  // every tick adds '1' to the progress bar
            if (progressBar.Value >= progressBar.Maximum)
            {
                endTheGame(); // if progress bar is full, game is over.
            }
        }

        private void endTheGame()
        {
            if (!playArea.Children.Contains(gameOverText)) // if the game over text is not visible (part of playArea canvas)
            {
                enemyTimer.Stop(); //stop enemy timer (actually just garbage collecting)
                targetTimer.Stop(); // stop progress bar (GC)
                humanCapped = false; // set humanCaptured to false
                startButton.Visibility = Visibility.Visible; // make startbutton visible again
                playArea.Children.Add(gameOverText); // show GameOver Text to tell player the game is over.

            }
        }

        private void enemyTimer_Tick(object sender, object e)
        {
            AddEnemy(); // every tick, add an enemy
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startGame(); // when pressed the startButton, execute startGame_routine.
        }

        private void startGame()
        {
            human.IsHitTestVisible = true; // IsHitTestVisible defines if an object is subject to collisions
            humanCapped = false; // game just started so make sure human is not captured
            progressBar.Value = 0; // set progress bar to 0
            startButton.Visibility = Visibility.Collapsed; // hide startButton
            playArea.Children.Clear(); //make canvas empty
            playArea.Children.Add(human); // add human to canvas
            playArea.Children.Add(target); //add target to canvas
            enemyTimer.Start(); //start the enemy timer
            targetTimer.Start(); //start progress bar
        }

        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl();
            enemy.Template = Resources["EnemyTemplate"] as ControlTemplate;
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, random.Next((int)playArea.ActualHeight - 100), random.Next((int)playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy); //add enemy to playing field
            enemy.PointerEntered += enemy_PointerEntered; //if pointer hits enemy, execute enemy_pointerentered
        }

        private void enemy_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (humanCapped)
            {
                endTheGame();
            }
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void human_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (enemyTimer.IsEnabled)
            {
                humanCapped = true;
                human.IsHitTestVisible = false;
            }

        }

        private void target_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (targetTimer.IsEnabled && humanCapped)
            {
                progressBar.Value = 0;
                Canvas.SetLeft(target, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(target, random.Next(100, (int)playArea.ActualHeight - 100));
                Canvas.SetLeft(human, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(human, random.Next(100, (int)playArea.ActualHeight - 100));
                humanCapped = false;
                human.IsHitTestVisible = true;

            }
        }

        private void playArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (humanCapped)
            {
                Point pointerPosition = e.GetCurrentPoint(null).Position;
                Point relativePosition = grid.TransformToVisual(playArea).TransformPoint(pointerPosition);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth * 3) || (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * 3))
                {
                    humanCapped = false;
                    human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth);
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight);
                }

            }
        }

        private void playArea_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (humanCapped)
            {
                endTheGame();
            }
        }


    }
}

