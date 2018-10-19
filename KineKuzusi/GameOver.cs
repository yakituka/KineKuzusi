using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinect.Toolbox;
using System.IO;
using Microsoft.Kinect;


namespace KineKuzusi
{
    public partial class GameOver : UserControl
    {
        int score;
        List<string> scoreRanking = new List<string>();
        List<int> intScoreRanking = new List<int>();

        bool once = true;
        Tools tools = new Tools();
        Timer timer = new Timer();

        SwipeGestureDetector swipeDetector = new SwipeGestureDetector();

        public GameOver()
        {
            InitializeComponent();

            if (File.Exists(@"Scores.csv"))
            {
                scoreRanking.AddRange(File.ReadAllText(@"Scores.csv").Split(','));
                scoreRanking.RemoveAll(s => s == "");

                intScoreRanking = scoreRanking.ConvertAll(s => int.Parse(s));

                score = intScoreRanking[intScoreRanking.Count - 1];
                intScoreRanking.Sort(); intScoreRanking.Reverse();
            }

            try
            {
                if (KinectSensor.KinectSensors.Count <= 0)
                {
                    throw new Exception("Kinectを接続してください。");
                }

                StartKinect(KinectSensor.KinectSensors[0]);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            swipeDetector.OnGestureDetected += new Action<string>(swipe_event);
        }

        private void swipe_event(string obj)
        {
            if (once)
            {
                once = false;
                Dispose();
            }
        }

        private void StartKinect(KinectSensor kinect)
        {
            //FormMain.FormMainInstance.SetDesktopBounds(0, 0, 1440, 810);
            //SetBounds(0, 0, 1440, 810);
            Form f = FindForm();
            if (f != null) f.SetDesktopBounds(0, 0, 1440, 810);

            //スケルトンフレームを有効にし、イベントを登録する
            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(skeleton_update);

            //スワイプジェスチャーのイベント登録
            //swipeDetector.OnGestureDetected += new Action<string>(swipe_event);

            kinect.Start();
        }

        private void skeleton_update(object sender, SkeletonFrameReadyEventArgs e)
        {

            KinectSensor kinect = sender as KinectSensor;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    foreach (Skeleton skeleton in skeletons)
                    {
                        Joint handLeft = skeleton.Joints[JointType.HandLeft];
                        Joint handRight = skeleton.Joints[JointType.HandRight];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                            && handRight.TrackingState == JointTrackingState.Tracked)
                        {
                            swipeDetector.Add(handRight.Position, kinect);
                        }
                    }
                }
            }
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            Font font = new Font("HGSSoeiKakupoptai Regular", 70);
            //2度目のゲーム
            if (File.Exists(@"Scores.csv"))
            {
                e.Graphics.DrawString("あなたの得点", font, Brushes.CadetBlue, Width / 5, Height * 1 / 5);
                e.Graphics.DrawString(score.ToString(), font, Brushes.MediumVioletRed, Width * 15 / 50, Height * 2 / 5);

                e.Graphics.DrawString("ランキング", font, Brushes.CadetBlue, (float)(Width / 1.5), Height / 20);
                for (int i = 1; i <= (intScoreRanking.Count < 8 ? intScoreRanking.Count : 8); i++)
                {
                    DateTime dateTime = DateTime.Now;
                    string date = dateTime.ToString("HH時mm分");
                    e.Graphics.DrawString(date+" "+i.ToString() + "位" + " : " + intScoreRanking[i - 1].ToString(), font, Brushes.CadetBlue, (float)(Width / 2), Height / 20 * (i * 2 + 1));
                }
            }
            //初回起動時
            else
            {
                e.Graphics.DrawString("右手を横に振ってください", font, Brushes.DimGray, Width/4, Height/2);
            }
        }
    }
}
