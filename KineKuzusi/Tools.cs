using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KineKuzusi
{
    class Tools
    {
        // 内積計算
        private double DotProduct(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        //球と物体の衝突を判定する
        public bool IsCollisionBase(Vector p1, Vector p2, Vector center, float radius)
        {
            Vector lineDir = (p2 - p1);
            Vector n = new Vector(lineDir.Y, -lineDir.X);
            n.Normalize();

            Vector dir1 = center - p1;
            Vector dir2 = center - p2;

            double dist = Math.Abs(DotProduct(dir1, n));
            double a1 = DotProduct(dir1, lineDir);
            double a2 = DotProduct(dir2, lineDir);

            return (a1 * a2 < 0 && dist < radius) ? true : false;
        }

        //球とパドルの衝突を判定する
        public Vector BallReflection(Vector p1, Vector p2, Vector ballCenter, float radius, Rectangle rect, Vector Speed)
        {
                double paddleCenter = rect.X + rect.Width / 2;
                double diff = ballCenter.X - paddleCenter;
                double x, y;
                if(diff >= 0)
                {
                    double angle = diff / (rect.Width/2) * 45;
                    y = -Math.Cos(angle * Math.PI / 180) * Speed.Length;
                    x = Math.Sin(angle * Math.PI / 180) * Speed.Length;
                }
                else
                {
                    diff *= -1;
                    double angle = diff / (rect.Width / 2) * 45;
                    y = -Math.Cos(angle * Math.PI / 180) * Speed.Length;
                    x = -Math.Sin(angle * Math.PI / 180) * Speed.Length;
                }

                return new Vector(x,y);
        }

        //球とブロックの衝突を判定する
        public int IsCollisionBlock(Rectangle block, GameMain.Ball ball)
        {
            if (IsCollisionBase(new Vector(block.Left, block.Top),
                new Vector(block.Right, block.Top), ball.Position, ball.Radius))
                return 1;

            if (IsCollisionBase(new Vector(block.Left, block.Bottom),
                new Vector(block.Right, block.Bottom), ball.Position, ball.Radius))
                return 2;

            if (IsCollisionBase(new Vector(block.Right, block.Top),
                new Vector(block.Right, block.Bottom), ball.Position, ball.Radius))
                return 3;

            if (IsCollisionBase(new Vector(block.Left, block.Top),
                new Vector(block.Left, block.Bottom), ball.Position, ball.Radius))
                return 4;

            return -1;
        }

        //ベクトルの向きを引数の角度の範囲でランダマイズする
        public Vector VectorRandomize(Vector v)
        {
            Random r = new Random();            

            double radian = (double)r.Next(1, 180) * Math.PI / 180;
            
            double x,y;
            x = Math.Cos(radian);
            y = Math.Sin(radian);

            return v.Length*(new Vector(x, y));
        }

        //配列の中身を範囲の値でランダマイズする
        public int[] ArrayRandomize(int length, int a, int b)
        {
            int[] vs = new int[length];
            Random r = new Random();
            for (int i = 0; i < vs.Length; i++)
                vs[i] = r.Next(a, b);
            return vs;
        }

        //配列の一致する要素の数を数える
        public int ElementEqualCount(int key,int[] vs, int x, int y)
        {
            int count=0;

            for(int i=0; i < x; i++)
            {
                for(int j=0; j < y; j++)
                {
                    if (vs[j*x+i] == key) count++;
                }
            }

            return count;
        }

        //配列の全ての要素の数を合計する
        public int ElementSum(int[] vs, int x, int y)
        {
            int sum = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    
                    sum += (vs[j*x+i] >= 0) ?  vs[j*x+i] : 0;
                }
            }

            return sum;
        }
    }
}
