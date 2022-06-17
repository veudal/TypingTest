using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
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

namespace TypingTest
{
    public partial class MainWindow : Window
    {
        string sentences = string.Empty;
        bool started = false;
        bool finished = false;
        int totalWords = 0;
        int writtenWords = 0;
        int mistakes = 0;
        readonly Stopwatch stopwatch = new Stopwatch();
        readonly SoundPlayer player = new SoundPlayer(Properties.Resources.KeyboardSoundEffect);


        public MainWindow()
        {
            InitializeComponent();
            Prepare();
        }

        private void Prepare()
        {
            WPM.Text = "-- WPM";
            Mistakes.Text = "0";
            sentences = GetRandomSentences();
            RandomText.Text = sentences;
            totalWords = CountWords(sentences);
            RemainingWords.Text = totalWords.ToString();
            InputTextBox.Focus();
        }

        private string GetRandomSentences()
        {
            string str = string.Empty;
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                str += Properties.Resources.ResourceManager.GetString(Convert.ToString(rand.Next(1, 250))) + " ";
            }
            return str.Remove(str.Length - 1);
        }

        private void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }


        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (finished == false)
            {
                HandleStart();

                string input = InputTextBox.Text;
                if (!sentences.StartsWith(input))
                {
                    HandleWrongKeys(input);
                    return;
                }

                if (input.EndsWith(" "))
                {
                    HandleWordFinished(input);
                }
                else if (input == sentences)
                {
                    HandleEnd();
                }
            }
        }

        private void HandleWrongKeys(string input)
        {
            Thread thread = new Thread(VisualizeMistake);
            thread.Start();
            Thread thread2 = new Thread(VisulizeMistakeCounter);
            thread2.Start();
            mistakes++;
            Mistakes.Text = mistakes.ToString();
            InputTextBox.Text = input.Remove(input.Length - 1);
            InputTextBox.CaretIndex = input.Length;

        }

        private void VisualizeMistake()
        {
            Dispatcher.BeginInvoke(new Action(() => InputTextBox.Background = Brushes.IndianRed));
            Thread.Sleep(500);
            Dispatcher.BeginInvoke(new Action(() => InputTextBox.Background = (Brush)new BrushConverter().ConvertFrom("#FF171717")));
        }

        private void VisulizeMistakeCounter()
        {
            Dispatcher.BeginInvoke(new Action(() => Mistakes.TextDecorations = TextDecorations.Underline));
            Dispatcher.BeginInvoke(new Action(() => Mistakes.FontWeight = FontWeights.Bold));
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Mistakes.FontSize < 65)
                    {
                        Mistakes.FontSize++;
                    }

                }));

            }
            Thread.Sleep(300);
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(20);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Mistakes.FontSize > 20)
                    {
                        Mistakes.FontSize--;
                    }

                }));

            }
            Dispatcher.BeginInvoke(new Action(() => Mistakes.TextDecorations = null));
            Dispatcher.BeginInvoke(new Action(() => Mistakes.FontWeight = FontWeights.Normal));
            Dispatcher.BeginInvoke(new Action(() => Mistakes.FontSize = 20));
        }

        private void HandleWordFinished(string input)
        {
            player.Play();
            writtenWords++;
            InputTextBox.Clear();
            sentences = sentences.Substring(input.IndexOf(" ") + 1);
            RandomText.Text = sentences;
            RemainingWords.Text = CountWords(sentences).ToString();
        }

        private void HandleEnd()
        {
            finished = true;
            stopwatch.Stop();
            string wpm = CalculateWPM(totalWords, stopwatch.ElapsedMilliseconds);
            InputTextBox.Text = "Your speed was: " + wpm + " with " + mistakes + " mistakes";
            RemainingWords.Text = "0";
            RandomText.Text = string.Empty;
            WPM.Text = wpm;
            InputTextBox.CaretBrush = Brushes.Transparent;
            InputTextBox.IsReadOnly = true;
            AgainBtn.Visibility = Visibility.Visible;

        }

        private void HandleStart()
        {
            if (started == false)
            {
                started = true;
                Info.Visibility = Visibility.Collapsed;
                stopwatch.Start();
                Thread thread = new Thread(WPM_Refresher);
                thread.Start();
            }
        }

        private void WPM_Refresher()
        {
            while (finished == false)
            {
                Dispatcher.BeginInvoke(new Action(() => WPM.Text = CalculateWPM(writtenWords, stopwatch.ElapsedMilliseconds)));
                Thread.Sleep(800);
            }
        }

        private int CountWords(string str)
        {
            return str.Split((char)32).Length;
        }

        private string CalculateWPM(int words, long milliseconds)
        {
            double minutes = milliseconds / 60000.0;
            string WPM = Convert.ToString(Math.Round((double)words / minutes)) + " WPM";
            return WPM;
        }

        private void Reset()
        {
            AgainBtn.Visibility = Visibility.Hidden;
            started = false;
            writtenWords = 0;
            mistakes = 0;
            stopwatch.Reset();
            InputTextBox.Clear();
            InputTextBox.CaretBrush = Brushes.White;
            InputTextBox.IsReadOnly = false;
            Info.Visibility = Visibility.Visible;
            finished = false;
            Prepare();
        }

        private void AgainBtn_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }
}
