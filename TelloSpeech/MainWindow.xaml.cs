using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Speech.Recognition;
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

namespace TelloSpeech
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private UdpClient udpForCmd;     //コマンド結果受信用クライアント
        private UdpClient udpForStsRecv; //ステータスの結果受信用クライアント
        SpeechRecognitionEngine recognizer; //

        public MainWindow()
        {
            InitializeComponent();
        }


        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            SetupTello();
            sendCmd("command");
            SetUpSpeech();
            btnConnect.IsEnabled = false;

        }

        // Telloとの通信を設定する
        private void SetupTello()
        {
            this.udpForCmd = new UdpClient(0);
            this.udpForStsRecv = new UdpClient(8890);

            // コマンド結果の受信処理
            Task.Run(() => {
                IPEndPoint remoteEP = null;//任意の送信元からのデータを受信
                while (true)
                {
                    try
                    {
                        String rcvMsg = "";
                        byte[] rcvBytes = udpForCmd.Receive(ref remoteEP);
                        Interlocked.Exchange(ref rcvMsg, Encoding.ASCII.GetString(rcvBytes));
                        txtResult.Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                txtResult.Text = rcvMsg;
                            })
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

            });

            // ステータスの受信処理
            Task.Run(() => {
                IPEndPoint remoteEP = null;//任意の送信元からのデータを受信
                while (true)
                {
                    try
                    {
                        String rcvMsg = "";
                        byte[] rcvBytes = udpForStsRecv.Receive(ref remoteEP);
                        Interlocked.Exchange(ref rcvMsg, Encoding.ASCII.GetString(rcvBytes));
                        rcvMsg = rcvMsg.Replace(";", "\r\n");
                        txbStatus.Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                txbStatus.Text = rcvMsg;
                            })
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                }

            });
        }

        // コマンド送信
        private void sendCmd(string cmd)
        {
            txtCmd.Text = cmd;
            byte[] data = Encoding.ASCII.GetBytes(cmd);
            this.udpForCmd.Send(data, data.Length, "192.168.10.1", 8889);

        }

        private void SetUpSpeech()
        {
            recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("ja-JP"));

            Choices singleCommands = CreateSingleCommandChoices();
            GrammarBuilder cmdGb = new GrammarBuilder();
            cmdGb.Append(new SemanticResultKey("singleCommands", singleCommands));
            Grammar cmdG = new Grammar(cmdGb);
            recognizer.LoadGrammar(cmdG);

            //
            Choices numbers = CreateNumberChoices();
            Choices directions = CreateDirections();
            // 


            // Create a GrammarBuilder object and append the Choices object.
            GrammarBuilder moveGb = new GrammarBuilder();
            moveGb.Append(new SemanticResultKey("moveCommands", directions));
            moveGb.Append(new SemanticResultKey("moveCm" , numbers));

            // Create the Grammar instance and load it into the speech recognition engine.
            Grammar moveG = new Grammar(moveGb);

            // Create and load a dictation grammar.  
            recognizer.LoadGrammar(moveG);

            // Add a handler for the speech recognized event.  
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);



            // Configure input to the speech recognizer.  
            recognizer.SetInputToDefaultAudioDevice();

            // Start asynchronous, continuous speech recognition.  
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }
        Choices CreateSingleCommandChoices()
        {
            Choices ret = new Choices();
            ret.Add(new SemanticResultValue("てーくおふ", "takeoff"));
            ret.Add(new SemanticResultValue("はっしん", "takeoff"));
            ret.Add(new SemanticResultValue("あくしょん", "takeoff"));
            ret.Add(new SemanticResultValue("らんど", "land"));
            ret.Add(new SemanticResultValue("ちゃくりく", "land"));
            ret.Add(new SemanticResultValue("きろくかいし", "streamon"));
            ret.Add(new SemanticResultValue("きろくていし", "streamoff"));

            return ret;
        }
        Choices CreateNumberChoices()
        {
            Choices numbers = new Choices();
            for (int i = 0; i < 300; ++i)
            {
                numbers.Add(new SemanticResultValue(i.ToString(), i));
            }
            return numbers;
        }

        Choices CreateDirections()
        {
            Choices ret = new Choices();
            ret.Add(new SemanticResultValue("ふぉわーど", "forward"));
            ret.Add(new SemanticResultValue("すすめ", "forward"));

            ret.Add(new SemanticResultValue("ばっく", "back"));
            ret.Add(new SemanticResultValue("もどれ", "back"));

            ret.Add(new SemanticResultValue("あっぷ", "up"));
            ret.Add(new SemanticResultValue("あがれ", "up"));

            ret.Add(new SemanticResultValue("だうん", "down"));
            ret.Add(new SemanticResultValue("さがれ", "down"));

            ret.Add(new SemanticResultValue("らいと", "right"));
            ret.Add(new SemanticResultValue("みぎ", "right"));

            ret.Add(new SemanticResultValue("れふと", "left"));
            ret.Add(new SemanticResultValue("ひだり", "left"));

            ret.Add(new SemanticResultValue("かいてん", "cw"));
            ret.Add(new SemanticResultValue("まわれ", "cw"));
            ret.Add(new SemanticResultValue("ぎゃくかいてん", "ccw"));
            return ret;
        }

        // Handle the SpeechRecognized event.  
        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Debug.WriteLine("SpeechRecognized...");
            if (e.Result.Semantics != null)
            {
                if (e.Result.Semantics.ContainsKey("singleCommands"))
                {
                    Debug.WriteLine("..." + e.Result.Semantics["singleCommands"].Value);
                    sendCmd((string)e.Result.Semantics["singleCommands"].Value);

                }
                else if (e.Result.Semantics.ContainsKey("moveCommands") &&
                         e.Result.Semantics.ContainsKey("moveCm"))
                {
                    Debug.WriteLine("..." + e.Result.Semantics["moveCommands"].Value + " " + e.Result.Semantics["moveCm"].Value);
                    sendCmd((string)e.Result.Semantics["moveCommands"].Value + " " + ((int)e.Result.Semantics["moveCm"].Value).ToString());

                }
            }
        }
    }
}
