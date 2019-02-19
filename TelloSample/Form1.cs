#define DECODE_H264

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;//for UDP
using System.Net.Sockets; //for UDP
using System.Threading;//for Interlocked
using System.Diagnostics;
using OpenCvSharp;
using System.IO;
using System.Runtime.InteropServices;

namespace TelloSample
{
    public partial class Form1 : Form
    {
#if (DECODE_H264)
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct H264DecoderResult
        {
            public int w;
            public int h;
            public int size;
            public IntPtr buff;
        }
        [DllImport("h264decoder.dll", EntryPoint = "InitH264Decoder")]
        static extern void _InitH264Decoder();

        [DllImport("h264decoder.dll", EntryPoint = "TermH264Decoder")]
        static extern void _TermH264Decoder();

        [DllImport("h264decoder.dll", EntryPoint = "DecodeH264")]
        static extern bool _DecodeH264(IntPtr buff, int size, ref H264DecoderResult outbuff);

        [DllImport("h264decoder.dll", EntryPoint = "GetH264DecoderLastError")]
        public static extern IntPtr GetH264DecoderLastError();

        [DllImport("h264decoder.dll", EntryPoint = "FreeData")]
        public static extern void FreeData(IntPtr data);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
 #endif

        private UdpClient udpForCmd;     //コマンド結果受信用クライアント
        private UdpClient udpForStsRecv; //ステータスの結果受信用クライアント
#if (DECODE_H264)
        private UdpClient udpForVideo;   //ビデオストリームの受信用
#endif
        public Form1()
        {
            InitializeComponent();
        }

        // コマンドの結果更新用
        private delegate void DelegateUpdateCmdResult(String ret);

        // ステータスの更新用
        private delegate void DelegateUpdateSts(String sts);

        // コマンド結果を更新。ワーカスレッドからの場合はメインスレッドで実行
        private void UpdateCmdResult(String ret)
        {
            if (this.InvokeRequired)
            {
                Object[] param = new Object[1] { ret };

                this.Invoke(new DelegateUpdateCmdResult(this.UpdateCmdResult), param);
                return;
            }
            this.txtRet.Text = ret;
            this.btnCmd.Enabled = true;
        }

        // ステータスを更新。ワーカスレッドからの場合はメインスレッドで実行
        private void UpdateSts(String sts)
        {
            if (this.InvokeRequired)
            {
                Object[] param = new Object[1] { sts };

                this.Invoke(new DelegateUpdateSts(this.UpdateSts), param);
                return;
            }
            this.txtSts.Text = sts;
        }



        // Telloとの通信を設定する
        private void SetupTello()
        {
            this.udpForCmd = new UdpClient(0);
            this.udpForStsRecv = new UdpClient(8890);
#if (DECODE_H264)
            this.udpForVideo = new UdpClient(11111);
#endif

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
                        this.UpdateCmdResult(rcvMsg);
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
                        this.UpdateSts(rcvMsg);
                    }
                    catch (Exception ex)
                    { 
                        Debug.WriteLine(ex.Message);
                    }

                }

            });

#if (DECODE_H264)
            // ビデオストリームの受信処理
            Task.Run(() => {
                IPEndPoint remoteEP = null;//任意の送信元からのデータを受信
                byte[] packetData = new byte[0];
                int cnt = 0;
                _InitH264Decoder();
                var fourcc = VideoWriter.FourCC('m', 'p', '4', 'v');
                var video = new VideoWriter("test.mp4", fourcc, 20, new OpenCvSharp.Size(960, 720) );

                while (true)
                {
                    try
                    {

                        byte[] rcvBytes = udpForVideo.Receive(ref remoteEP);
                        int l = packetData.Length;
                        Array.Resize<byte>(ref packetData, l + rcvBytes.Length);
                        Array.Copy(rcvBytes, 0, packetData, l, rcvBytes.Length);
                        if (rcvBytes.Length != 1460)
                        {

                            int size = Marshal.SizeOf(packetData[0]) * packetData.Length;
                            IntPtr inPtr = Marshal.AllocHGlobal(size);
                            Marshal.Copy(packetData, 0, inPtr, packetData.Length);

                            H264DecoderResult decret = new H264DecoderResult();
                            Debug.WriteLine("DO DECODE");
                            if (_DecodeH264(inPtr, packetData.Length, ref decret))
                            {
                                Debug.WriteLine("DO DECODE,,,,ok");
                                var mat = new Mat(decret.h, decret.w, MatType.CV_8UC3);
                                CopyMemory(mat.Data, decret.buff, (uint)decret.size);
                                var matCv = new Mat();
                                Cv2.CvtColor(mat, matCv, ColorConversionCodes.RGB2BGR);
                                video.Write(matCv);
                                FreeData(decret.buff);
                            }
                            else
                            {
                                Debug.Write(Marshal.PtrToStringAnsi(GetH264DecoderLastError()));
                            }
                            Marshal.FreeHGlobal(inPtr);


                            packetData = new byte[0];
                            ++cnt;

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                }
                // 後片付けの方法はあとで考える。（呼ばれない）
                _TermH264Decoder();
                video.Release();
            });
#endif

        }

        // コマンド送信
        private void sendCmd(string cmd)
        {
            byte[] data = Encoding.ASCII.GetBytes(cmd);
            this.udpForCmd.Send(data, data.Length, "192.168.10.1", 8889);

        }

        // 開始ボタン
        private void btnStart_Click(object sender, EventArgs e)
        {
            SetupTello();

            this.txtRet.Text = "";
            this.btnCmd.Enabled = false;

            sendCmd("command");
        }

        // コマンド送信ボタン押下
        private void btnCmd_Click(object sender, EventArgs e)
        {
            this.txtRet.Text = "";
            this.btnCmd.Enabled = false;
            sendCmd(this.txtCmd.Text);
        }

    }
}
