using NAudio.Gui;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;


namespace Midi
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Metro
        private DispatcherTimer timer;
        private SoundPlayer metroSong;
        bool metroState = false;
        int metroTemp = 120;
        // Instrument
        int instrumentarr = 1;
        string instrument = "Piano";
        int octNumber = 3;
        public string OctNumber { get => octNumber.ToString(); }
        Dictionary<Key, string> drumKeys = new Dictionary<Key, string>()
        {
            { Key.P, "Pad 1" },
            { Key.OemOpenBrackets, "Pad 2" },
            { Key.OemCloseBrackets, "Pad 3" },
            { Key.OemPipe, "Pad 4" },
            { Key.L, "Pad 5" },
            { Key.OemSemicolon, "Pad 6" },
            { Key.OemQuotes, "Pad 7" },
            { Key.Enter, "Pad 8" }
        };
        Dictionary<Key, Func<int, string>> midi = new Dictionary<Key, Func<int, string>>()
        {
            { Key.Z, (i)=>$"C{i}" },
            { Key.S, (i)=>$"C#{i}" },//#
            { Key.X, (i)=>$"D{i}" },
            { Key.D, (i)=>$"D#{i}" },//#
            { Key.C, (i)=>$"E{i}" },
            { Key.V, (i)=>$"F{i}" },
            { Key.G, (i)=>$"F#{i}" },//#
            { Key.B, (i)=>$"G{i}" },
            { Key.H, (i)=>$"G#{i}" },//#
            { Key.N, (i)=>$"A{i}" },
            { Key.J, (i)=>$"A#{i}" },//#
            { Key.M, (i)=>$"B{i}" },

            { Key.W, (i)=>$"C{i + 1}" },
            { Key.D3, (i)=>$"C#{i + 1}" },//#
            { Key.E, (i)=>$"D{i + 1}" },
            { Key.D4, (i)=>$"D#{i + 1}" },//#
            { Key.R, (i)=>$"E{i + 1}" },
            { Key.T, (i)=>$"F{i + 1}" },
            { Key.D6, (i)=>$"F#{i + 1}" },//#
            { Key.Y, (i)=>$"G{i + 1}" },
            { Key.D7, (i)=>$"G#{i + 1}" },//#
            { Key.U, (i)=>$"A{i + 1}" },
            { Key.D8, (i)=>$"A#{i + 1}" },//#
            { Key.I, (i)=>$"B{i + 1}" },
            { Key.O, (i)=>$"C{i + 3}" }
        };
        Dictionary<Key, Button> midiButton;
        Key lastkey = Key.None;
        Key lastkey2 = Key.None;
        // Sus
        bool sustein = false;
        // On/Off
        bool onOff = false;
        // Music
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFileReader;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            metroStart.Foreground = Brushes.Black;
            this.DataContext = this;
            midiButton = new Dictionary<Key, Button>()
            {
                { Key.Z, C1 },
                { Key.S, Cd1 },//#
                { Key.X, D1 },
                { Key.D, Dd1 },//#
                { Key.C, E1 },
                { Key.V, F1 },
                { Key.G, Fd1 },//#
                { Key.B, G1 },
                { Key.H, Gd1 },//#
                { Key.N, A1 },
                { Key.J, Ad1 },
                { Key.M, B1 },

                { Key.W, C2 },
                { Key.D3, Cd2 },//#
                { Key.E, D2 },
                { Key.D4, Dd2 },//#
                { Key.R, E2 },
                { Key.T, F2 },
                { Key.D6, Fd2 },//#
                { Key.Y, G2 },
                { Key.D7, Gd2 },//#
                { Key.U, A2 },
                { Key.D8, Ad2 },//#
                { Key.I, B2 },
                { Key.O, C3 },

                { Key.Enter, Pad1 },
                { Key.NumPad4, Pad2 },
                { Key.NumPad5, Pad3 },
                { Key.NumPad6, Pad4 },
                { Key.RightShift, Pad5 },
                { Key.NumPad1, Pad6 },
                { Key.NumPad2, Pad7 },
                { Key.NumPad3, Pad8 }
            };

            C1.Content = $"C{octNumber}";
            D1.Content = $"D{octNumber}";
            E1.Content = $"E{octNumber}";
            F1.Content = $"F{octNumber}";
            G1.Content = $"G{octNumber}";
            A1.Content = $"A{octNumber}";
            B1.Content = $"B{octNumber}";
            C2.Content = $"C{octNumber + 1}";
            D2.Content = $"D{octNumber + 1}";
            E2.Content = $"E{octNumber + 1}";
            F2.Content = $"F{octNumber + 1}";
            G2.Content = $"G{octNumber + 1}";
            A2.Content = $"A{octNumber + 1}";
            B2.Content = $"B{octNumber + 1}";
            C3.Content = $"C{octNumber + 2}";

            foreach (UIElement c in Pads.Children)
            {
                if (c is Button)
                {
                    ((Button)c).Click += Button_Click;
                }
            }

            metroSong = new SoundPlayer($"Resources/Metro.wav"); // Замените на путь к вашему звуку
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
        }
        public void ButtonOnOff_Click(object sender, RoutedEventArgs e)
        {
            
        }
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = null;
            try
            {
                string buttonContent = (string)((Button)e.OriginalSource).Content;

                switch (buttonContent)
                {
                    case "Pad 1":
                        path = Path.GetFullPath($"Resources/Drum/Pad 1.wav");
                        break;
                    case "Pad 2":
                        path = Path.GetFullPath($"Resources/Drum/Pad 2.wav");
                        break;
                    case "Pad 3":
                        path = Path.GetFullPath($"Resources/Drum/Pad 3.wav");
                        break;
                    case "Pad 4":
                        path = Path.GetFullPath($"Resources/Drum/Pad 4.wav");
                        break;
                    case "Pad 5":
                        path = Path.GetFullPath($"Resources/Drum/Pad 5.wav");
                        break;
                    case "Pad 6":
                        path = Path.GetFullPath($"Resources/Drum/Pad 6.wav");
                        break;
                    case "Pad 7":
                        path = Path.GetFullPath($"Resources/Drum/Pad 7.wav");
                        break;
                    case "Pad 8":
                        path = Path.GetFullPath($"Resources/Drum/Pad 8.wav");
                        break;
                }
                PlaySound(path);
            }
            catch 
            {
                return;
            }
            
        }

        private void PlaySound(string filePath)
        {
            _audioFileReader = new AudioFileReader(filePath);
            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_audioFileReader);
            _outputDevice.Play();
        }
        private void Pads_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != lastkey)
            {
                try
                {
                    string path = Path.GetFullPath($"Resources/Drum/{drumKeys[e.Key]}.wav");
                    PlaySound(path); 
                }
                catch
                {
                    return;
                }
            }
        }
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != lastkey)
            {
                try
                {
                    string path = Path.GetFullPath($"Resources/{instrument}/{midi[e.Key](octNumber)}.wav");
                    Console.WriteLine(path);
                    midiButton[e.Key].Background = Brushes.Gray;
                    PlaySound(path);
                    lastkey = e.Key;
                }
                catch
                {
                    return;
                }
            }
        }
        private void Canvas_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                Color back = (Color)ColorConverter.ConvertFromString("#9BA8AF");
                Color backSharp = (Color)ColorConverter.ConvertFromString("#0f1c25");
                if (e.Key == Key.S || e.Key == Key.D || e.Key == Key.G || e.Key == Key.H || e.Key == Key.J || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8)
                {
                    midiButton[e.Key].Background = new SolidColorBrush(backSharp);
                }
                else
                {
                    midiButton[e.Key].Background = new SolidColorBrush(back);
                }
                lastkey = Key.None;
                if (sustein)
                {
                    _outputDevice?.Stop();
                    _audioFileReader?.Dispose();
                }
            }
            catch
            {
                return;
            }
        }
        private void octPlus_Click(object sender, RoutedEventArgs e)
        {
            if (octNumber < 5)
                octNumber++;

            C1.Content = $"C{octNumber}";
            D1.Content = $"D{octNumber}";
            E1.Content = $"E{octNumber}";
            F1.Content = $"F{octNumber}";
            G1.Content = $"G{octNumber}";
            A1.Content = $"A{octNumber}";
            B1.Content = $"B{octNumber}";
            C2.Content = $"C{octNumber + 1}";
            D2.Content = $"D{octNumber + 1}";
            E2.Content = $"E{octNumber + 1}";
            F2.Content = $"F{octNumber + 1}";
            G2.Content = $"G{octNumber + 1}";
            A2.Content = $"A{octNumber + 1}";
            B2.Content = $"B{octNumber + 1}";
            C3.Content = $"C{octNumber + 2}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OctNumber"));
        }

        private void octMinus_Click(object sender, RoutedEventArgs e)
        {
            if (octNumber > 1)
                octNumber--;

            C1.Content = $"C{octNumber}";
            D1.Content = $"D{octNumber}";
            E1.Content = $"E{octNumber}";
            F1.Content = $"F{octNumber}";
            G1.Content = $"G{octNumber}";
            A1.Content = $"A{octNumber}";
            B1.Content = $"B{octNumber}";
            C2.Content = $"C{octNumber + 1}";
            D2.Content = $"D{octNumber + 1}";
            E2.Content = $"E{octNumber + 1}";
            F2.Content = $"F{octNumber + 1}";
            G2.Content = $"G{octNumber + 1}";
            A2.Content = $"A{octNumber + 1}";
            B2.Content = $"B{octNumber + 1}";
            C3.Content = $"C{octNumber + 2}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OctNumber"));
        }

        private void Sustein_Click(object sender, RoutedEventArgs e)
        {
            if (!sustein)
            {
                Susteinlable.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f26037"));
                sustein = true;
            }
            else
            {
                Susteinlable.Foreground = Brushes.Black;
                sustein = false;
            }
        }
        private void instPlus_Click(object sender, RoutedEventArgs e)
        {
            if (instrumentarr < 2)
            {
                instrumentarr++;
                postInstr();
            }
        }
        private void instMin_Click(object sender, RoutedEventArgs e)
        {
            if (instrumentarr > 1)
            {
                instrumentarr--;
                postInstr();
            }
        }
        private void postInstr()
        {
            if (instrumentarr == 1)
            {
                instr.Content = "Piano";
                instrument = "Piano";
            } else if (instrumentarr == 2)
            {
                instr.Content = "Bass";
                instrument = "Bass";
            } else if (instrumentarr == 0 || instrumentarr > 2)
            {
                MessageBox.Show("Не удалось найти путь", "Ошибка загрузки семпла", MessageBoxButton.OK);  
                this.Close();
            }
        }

        private void Metro_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            metroState = false;
            metroStart.Foreground = Brushes.Black;
            if (metroTemp < 220)
            {
                metroTemp = metroTemp + 5;
                tempNumber.Content = $"{metroTemp.ToString()}bpm";
            }
            else if (metroTemp == 220)
            {
                metroTemp = 30;
                tempNumber.Content = $"{metroTemp.ToString()}bpm";
            }
        }
        private void metroStart_Click(object sender, RoutedEventArgs e)
        {
            if (!metroState)
            {
                metroStart.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f26037"));
                timer.Interval = TimeSpan.FromMilliseconds(60000.0 / metroTemp);
                timer.Start();
                metroState = true;
            } 
            else
            {
                metroStart.Foreground = Brushes.Black;
                timer.Stop();
                metroState = false;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            metroSong.Play();
        }
    }
}
