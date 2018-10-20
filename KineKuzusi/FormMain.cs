using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Windows;
using Kinect.Toolbox;
using Rectangle = System.Drawing.Rectangle;
using System.IO;

namespace KineKuzusi
{
    public partial class FormMain : Form
    {
        public static GameMain gameMain;
        public static GameOver gameOver;
        private static Panel panel;

        //コンストラクタ
        public FormMain()
        {
            //初期化処理
            InitializeComponent();
            WindowState =  FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            panel = panel1;
            if (!File.Exists(@"Scores.csv")) {
                MessageBox.Show("エラー : Scores.csvが存在しないよ!");
                Close();
            };

            CreateGameOver();
        }

        //ゲーム画面を作成し表示する
        private static void CreateGameMain()
        {
            gameMain = new GameMain();
            gameMain.Disposed += new EventHandler(gameMain_disposed);
            panel.Controls.Add(gameMain);
            gameMain.Dock = DockStyle.Fill;
            gameMain.Visible = true;

            /*
            WMPLib.WindowsMediaPlayer mediaPlayer = new WMPLib.WindowsMediaPlayer();
            mediaPlayer.URL = @"background_music.mp3";
            mediaPlayer.controls.play();
            */
        }

        //ゲームオーバー画面を作成し表示する
        private static void CreateGameOver()
        {
            gameOver = new GameOver();
            gameOver.Disposed += new EventHandler(gameOver_disposed);
            panel.Controls.Add(gameOver);
            gameOver.Dock = DockStyle.Fill;
            gameOver.Visible = true;
        }

        //gameMainが破壊された時呼び出される
        private static void gameMain_disposed(object sender, EventArgs e)
        {
            //MessageBox.Show("GameMain was dead! Creating GameOver ...");
            panel.Controls.Remove(gameMain);
            gameMain.Disposed -= new EventHandler(gameMain_disposed);
            CreateGameOver();
        }

        //gameOverが破壊された時呼び出される
        private static void gameOver_disposed(object sender, EventArgs e)
        {
            //MessageBox.Show("GameOver was dead! Creating GameMain ...");
            panel.Controls.Remove(gameOver);
            gameOver.Disposed -= new EventHandler(gameOver_disposed);
            CreateGameMain();
        }
    }
}