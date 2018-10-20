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
        Score scoreLast;

        List<Score> scores = new List<Score>();

        bool once = true;
        bool noControl = true;
        Tools tools = new Tools();
        Timer noControlTimer = new Timer();

        SwipeGestureDetector swipeDetector = new SwipeGestureDetector();

        public GameOver()
        {
            InitializeComponent();

            if (File.Exists(@"Scores.csv"))
            {
                //スコアリストに代入
                foreach(string r in File.ReadAllText(@"Scores.csv").Split(','))
                {
                    String[] split = r.Split('A');
                    if (split.Length == 2)
                    {
                        Score score = new Score(split[0], split[1]);
                        scores.Add(score);
                    }
                }


                //直前のスコアを代入
                scoreLast = scores[scores.Count - 1];
                //空文字が入っているデータは消す
                scores.RemoveAll(s => s.score == "" || s.date == "");

                //ソートする
                scores.Sort((a, b) => int.Parse(b.score) - int.Parse(a.score));

                //操作禁止タイマーの発動
                noControlTimer.Interval = 3000;
                noControlTimer.Tick += new EventHandler(no_control_evnet);
                noControlTimer.Enabled = true;
            }
            else
            {
                File.Create(@"Scores.csv");
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
                Form f = FindForm();
                if (f != null) f.Close();
            }

            swipeDetector.OnGestureDetected += new Action<string>(swipe_event);
        }

        private void swipe_event(string obj)
        {
            if (once && !noControl)
            {
                once = false;
                Dispose();
            }
        }

        private void no_control_evnet(object sender, EventArgs e)
        {
            noControl = false;
            noControlTimer.Enabled = false;
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
            if (File.Exists(@"Scores.csv") && scores.Count > 0)
            {
                e.Graphics.DrawString("あなたの得点", font, Brushes.CadetBlue, Width / 5, Height * 1 / 5);

                int rank = scores.IndexOf(scoreLast)+1;
                e.Graphics.DrawString(rank.ToString()+"位 : "+scoreLast.score, font, Brushes.MediumVioletRed, Width * 9 / 50, Height * 2 / 5);

                e.Graphics.DrawString("右手を左右に降って\nゲームスタート!", font, Brushes.DimGray, Width / 15, Height *7/10);

                e.Graphics.DrawString("ランキング", font, Brushes.CadetBlue, (float)(Width / 1.5), Height / 20);
                for (int i = 1; i <= (scores.Count < 8 ? scores.Count : 8); i++)
                {
                    DateTime dateTime = DateTime.Now;
                    e.Graphics.DrawString(scores[i-1].date+" "+i.ToString() + "位" + " : " + scores[i-1].score, font, Brushes.CadetBlue, (float)(Width / 2 + Width/12), Height / 20 * (i * 2 + 1));
                }
            }
            //初回起動時
            else
            {
                e.Graphics.DrawString("右手を横に振ってください", font, Brushes.DimGray, Width/4, Height/2);
            }
        }

        private class Score
        {
            public string score { get; set; }
            public string date { get; set; }

            public Score(string s, string d)
            {
                score = s;
                date = d;
            }
        }
    }
}
