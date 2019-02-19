using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace OpenCVVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            var camera = VideoCapture.FromFile("udp://127.0.0.1:11111");
            using (var normalWindow = new Window("normal"))
            {
                var normalFrame = new Mat();
                var srFrame = new Mat();
                while (true)
                {
                    camera.Read(normalFrame);
                    if (normalFrame.Empty())
                        break;

                    normalWindow.ShowImage(normalFrame);
                    int key = Cv2.WaitKey(100);
                    if (key == 27) break;   // ESC キーで閉じる
                }
            }

        }
    }
}
