using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Windows;
using Kinect.Toolbox;
using Rectangle = System.Drawing.Rectangle;

namespace KineKuzusi
{
    public partial class GameMain : UserControl
    {
        //グローバル変数群
        bool once = true;
        bool ballLaunched = false;
        int durabilityStart;
        int durabilityEnd;
        int scoreCounter;
        Paddle paddle;
        Ball ball;
        public static Blocks blocks;
        Rectangle leftWall;
        Rectangle rightWall;

        Timer timer = new Timer();
        Timer speedUpTimer = new Timer();
        Timer ballLaunchTimer = new Timer();

        Tools tools = new Tools();

        //SwipeGestureDetector swipeDetector = new SwipeGestureDetector();

        //コンストラクタ
        public GameMain()
        {
            InitializeComponent();

            //グローバル変数の初期化
            paddle = new Paddle(
                     new SolidBrush(Color.DimGray),
                     new Rectangle(Width / 2, Height * 9 / 10, Width / 8, Height / 80),
                     Width / 7,
                     Width / 3
            );

            ball = new Ball(
                   new SolidBrush(Color.HotPink),
                   new Vector(Width / 2 + Width / 16, Height * 8 / 10 - Height / 20),
                   new Vector(0,0),
                   Height / 40,
                   false
            );

            blocks = new Blocks(
                     new Rectangle(Width / 3, Height / 20, Width / 10, Height / 20),
                     new Vector(0, 0),
                     5,
                     4,
                     tools.ArrayRandomize(5*4, 1, 4)
            );

            durabilityStart = tools.ElementSum(blocks.DurabilityArray, blocks.Column, blocks.Row);

            leftWall = new Rectangle(0, 0, Width / 10, Height);
            rightWall = new Rectangle(Width * 9 / 10, 0, Width / 10, Height);

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
                if(f != null) f.Close();
            }
        }

        //Kinectの動作を開始する
        private void StartKinect(KinectSensor kinect)
        {
            //FormMain.FormMainInstance.SetDesktopBounds(0, 0, 1440, 810);
            //SetBounds(0, 0, 1440, 810);
            Form f = FindForm();
            if (f != null) f.SetDesktopBounds(0, 0, 1440, 810);

            //スケルトンフレームを有効にし、イベントを登録する
            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(skeleton_update);

            timer.Interval = 3000;
            timer.Tick += new EventHandler(timer_event);

            speedUpTimer.Interval = 5000;
            speedUpTimer.Tick += new EventHandler(speed_up_event);
            speedUpTimer.Enabled = true;

            ballLaunchTimer.Interval = 5000;
            ballLaunchTimer.Tick += new EventHandler(ball_launch_event);
            ballLaunchTimer.Enabled = true;
                
            kinect.Start();
        }

        //スケルトンが更新される時に呼び出される
        private void skeleton_update(object sender, SkeletonFrameReadyEventArgs e)
        {

            KinectSensor kinect = sender as KinectSensor;


            //Form fm = FindForm();
            //if (fm != null) fm.SetDesktopBounds(0, 0, 1920, 1080);


            //ジョイント座標の更新
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
                        Joint spine = skeleton.Joints[JointType.Spine];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                            && handLeft.TrackingState == JointTrackingState.Tracked
                            && handRight.TrackingState == JointTrackingState.Tracked
                            && spine.TrackingState == JointTrackingState.Tracked)
                        {

                            //swipeDetector.Add(handRight.Position, kinect);

                            //RGB画像の座標への変換
                            var coordinateMapper = KinectSensor.KinectSensors[0].CoordinateMapper;
                            ColorImagePoint rightHandPos = coordinateMapper.MapSkeletonPointToColorPoint(handRight.Position, KinectSensor.KinectSensors[0].ColorStream.Format);
                            ColorImagePoint leftHandPos = coordinateMapper.MapSkeletonPointToColorPoint(handLeft.Position, KinectSensor.KinectSensors[0].ColorStream.Format);
                            ColorImagePoint spinePos = coordinateMapper.MapSkeletonPointToColorPoint(spine.Position, KinectSensor.KinectSensors[0].ColorStream.Format);

                            int leftdiff = leftHandPos.X - spinePos.X;
                            int rightdiff = rightHandPos.X - spinePos.X;
                            int center = spinePos.X;

                            if (-leftdiff < paddle.MinimumWidth / 5)
                            {
                                leftdiff = -paddle.MinimumWidth / 5;
                            }
                            else if (-leftdiff > paddle.MaximumWidth / 5)
                            {
                                leftdiff = -paddle.MaximumWidth / 5;
                            }

                            if (rightdiff < paddle.MinimumWidth / 5)
                            {
                                rightdiff = paddle.MinimumWidth / 5;
                            }
                            else if (rightdiff > paddle.MaximumWidth / 5)
                            {
                                rightdiff = paddle.MaximumWidth / 5;
                            }

                            paddle.Size = new Rectangle( (center + leftdiff) * Width / 640, Height * 9 / 10, (-leftdiff + rightdiff) * Width / 640, Height / 80);
                        }
                    }
                }
            }


            //ボールをウィンドウサイズに合わせる
            if (!ballLaunched)
            {
                ball.Speed = new Vector(0, 0);
                ball.Position = new Vector(paddle.Size.X + paddle.Size.Width / 2, paddle.Size.Y - ball.Radius);
            }
            //ball.SetSpeed(Bounds.X / 50, Bounds.Y / 50);
            ball.Radius = Height / 40;
            paddle.MinimumWidth = Width / 7;
            paddle.MaximumWidth = Width / 3;

            //壁をウィンドウサイズに合わせる
            leftWall = new Rectangle(0, 0, Width / 10, Height);
            rightWall = new Rectangle(Width * 9 / 10, 0, Width / 10, Height);

            //ブロックをウィンドウサイズに合わせる
            blocks.Size = new Rectangle(Width / 5, Height / 20, Width / 10, Height / 20);
            blocks.BlockInterval = new Vector(5, 5);

            //球と壁との衝突を判定する
            if (ball.Position.X + ball.Radius > rightWall.Left) ball.ReverseSpeedX();
            if (ball.Position.X - ball.Radius < leftWall.Right) ball.ReverseSpeedX();
            if (ball.Position.Y + ball.Radius <= 0) ball.ReverseSpeedY();

            //球と画面下の衝突を判定する
            if (ball.Position.Y + ball.Radius >= Height && once)
            {
                once = false;
                durabilityEnd = tools.ElementSum(blocks.DurabilityArray, blocks.Column, blocks.Row);
                int score = scoreCounter * 100;
                DateTime date = DateTime.Now;
                string dateString = date.ToString("HH:mm");
                File.AppendAllText(@"Scores.csv", score.ToString() + "A" + dateString + ",");
                Dispose();
            }

            //球の移動
            ball.Position += ball.Speed;

            //全消ししたら復活する
            if (blocks.DurabilityArray.All(s => s == 0)) { blocks.DurabilityArray = tools.ArrayRandomize(5*4, 1, 4);}

            //球とパドルの当たり判定
            if (tools.IsCollisionBase(new Vector(paddle.Size.X, paddle.Size.Y), new Vector(paddle.Size.X + paddle.Size.Width, paddle.Size.Y), ball.Position, ball.Radius))
            {
                if (paddle.Size.Width < Width / 5)
                {
                    timer.Enabled = true;
                    ball.Brush = new SolidBrush(Color.Purple);
                    ball.IsDoubleDamaged = true;
                }
                ball.Speed = tools.BallReflection(new Vector(paddle.Size.X, paddle.Size.Y), new Vector(paddle.Size.X + paddle.Size.Width, paddle.Size.Y), ball.Position, ball.Radius, paddle.Size, ball.Speed);
            }

            //球とブロックの当たり判定
            for (int i = 0; i < blocks.Column; i++)
            {
                for (int j = 0; j < blocks.Row; j++)
                {
                    int x = j * blocks.Column + i;
                    if (blocks.DurabilityArray[x] > 0)
                    {
                        int collision = tools.IsCollisionBlock(blocks.BlockPosition(j, i), ball);
                        if (collision == 1 || collision == 2)
                        {
                            if (ball.IsDoubleDamaged) { blocks.DurabilityArray[x]--; scoreCounter++; } 
                            blocks.DurabilityArray[x]--; scoreCounter++;

                            ball.ReverseSpeedY();
                        }
                        else if (collision == 3 || collision == 4)
                        {
                            if (ball.IsDoubleDamaged) { blocks.DurabilityArray[x]--; scoreCounter++; }
                            blocks.DurabilityArray[x]--; scoreCounter++;

                            ball.ReverseSpeedX();
                        }
                    }
                }
            }

            //パドルの特殊処理
            if (paddle.Size.Width < Width / 5)
                paddle.Brush = new SolidBrush(Color.LightGreen);
            else
                paddle.Brush = new SolidBrush(Color.DimGray);

            //壁とパドルの当たり判定
            if (leftWall.Width >= paddle.Size.X)
            {
                int x = paddle.Size.X;
                x = leftWall.Width;
            }
            else if(rightWall.X <= paddle.Size.Right)
            {
                int y = paddle.Size.Y;
                y = rightWall.X;
            }

            //Drawの再描画
            Invalidate();
        }

        private void speed_up_event(object sender, EventArgs e)
        {
            ball.Speed *= 1.1;
        }

        private void ball_launch_event(object sender, EventArgs e)
        {
            ballLaunched = true;
            ball.Speed = tools.VectorRandomize(new Vector(Width / 80, Height / 80));
            ballLaunchTimer.Enabled = false;
        }
        
        //タイマーが経過した
        private void timer_event(object sender, EventArgs e)
        {
            ball.Brush = new SolidBrush(Color.HotPink);
            ball.IsDoubleDamaged = false;
            timer.Enabled = false;
        }

        //描画関数
        private void Draw(object sender, PaintEventArgs e)
        {

            //球の描画
            float px = (float)ball.Position.X - ball.Radius;
            float py = (float)ball.Position.Y - ball.Radius;
            e.Graphics.FillEllipse(ball.Brush, px, py, ball.Radius * 2, ball.Radius * 2);

            //パドルの描画
            e.Graphics.FillRectangle(paddle.Brush, paddle.Size);

            //壁の配置
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), leftWall);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), rightWall);

            //スコアの描画
            e.Graphics.DrawString(scoreCounter.ToString(),
                new Font("HGSSoeiKakupoptai Regular", 70),
                Brushes.DarkKhaki,
                Width*9/10,
                Height/10
                );

            //ブロックの描画
            for (int i = 0; i < blocks.Column; i++)
            {
                for (int j = 0; j < blocks.Row; j++)
                {
                    int x = j * blocks.Column + i;
                    if (blocks.DurabilityArray[x] >= 4)
                        e.Graphics.FillRectangle(new SolidBrush(Color.Violet), blocks.BlockPosition(j, i));
                    else if (blocks.DurabilityArray[x] == 3)
                        e.Graphics.FillRectangle(new SolidBrush(Color.Red), blocks.BlockPosition(j, i));
                    else if (blocks.DurabilityArray[x] == 2)
                        e.Graphics.FillRectangle(new SolidBrush(Color.Yellow), blocks.BlockPosition(j, i));
                    else if (blocks.DurabilityArray[x] == 1)
                        e.Graphics.FillRectangle(new SolidBrush(Color.Green), blocks.BlockPosition(j, i));
                }
            }

        }

        public class Ball
        {
            public SolidBrush Brush { get; set; }
            public Vector Position { get; set; }
            public Vector Speed { get; set; }
            public int Radius { get; set; }
            public bool IsDoubleDamaged { get; set; }

            public Ball(SolidBrush brush, Vector position, Vector speed, int radius, bool isPenetrated)
            {
                Brush = brush;
                Position = position;
                Speed = speed;
                Radius = radius;
                IsDoubleDamaged = isPenetrated;
            }

            public void ReverseSpeedX()
            {
                Speed = new Vector(Speed.X * -1, Speed.Y);
            }

            public void ReverseSpeedY()
            {
                Speed = new Vector(Speed.X, Speed.Y * -1);
            }
        }

        public class Paddle
        {
            public SolidBrush Brush { get; set; }
            public Rectangle Size { get; set; }
            public int MinimumWidth { get; set; }
            public int MaximumWidth { get; set; }

            public Paddle(SolidBrush brush, Rectangle size, int minimum, int maximum)
            {
                Brush = brush;
                Size = size;
                MinimumWidth = minimum;
                MaximumWidth = maximum;
            }
        }

        public class Blocks
        {
            public Rectangle Size { get; set; }
            public Vector BlockInterval { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
            public int[] DurabilityArray { get; set; }

            public Blocks(Rectangle size, Vector blockInterval, int row, int column, int[] durabilityArray)
            {
                Size = size;
                BlockInterval = blockInterval;
                Row = row;
                Column = column;
                DurabilityArray = durabilityArray;
            }
            public Rectangle BlockPosition(int row, int column)
            {
                int x = Size.X + ((int)BlockInterval.X + Size.Width) * row;
                int y = Size.Y + ((int)BlockInterval.Y + Size.Height) * column;
                return new Rectangle(x, y, Size.Width, Size.Height);
            }
        }
    }
}
