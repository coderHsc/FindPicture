using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
namespace WindowsFormsApplication1
{


    static class Program
    {
        struct NumBody
        {
            public int num;//数字
            public int matchNum;//匹配的个数
            public int matchSum;
            public double matchRate;//匹配度
            public System.Drawing.Point point;
            public List<System.Drawing.Point> bodyCollectionPoint;//该数字所有像素在大图中的坐标
        }
        static bool ColorAEqualColorB(System.Drawing.Color colorA, System.Drawing.Color colorB, byte errorRange = 10)
        {
            return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;

        }
        static bool ListContainsPoint(List<System.Drawing.Point> listPoint, System.Drawing.Point point, double errorRange = 10)
        {
            bool isExist = false;
            foreach (var item in listPoint)
            {
                if (item.X <= point.X + errorRange && item.X >= point.X - errorRange && item.Y <= point.Y + errorRange && item.Y >= point.Y - errorRange)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
        static bool ListTextBodyContainsPoint(List<NumBody> listPoint, System.Drawing.Point point, double errorRange = 10)
        {
            bool isExist = false;
            foreach (var item in listPoint)
            {

                if (item.point.X <= point.X + errorRange && item.point.X >= point.X - errorRange && item.point.Y <= point.Y + errorRange && item.point.Y >= point.Y - errorRange)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
        /// <summary>
        /// 查找图片，不能镂空
        /// </summary>
        /// <param name="subPic"></param>
        /// <param name="parPic"></param>
        /// <param name="searchRect">如果为empty，则默认查找整个图像</param>
        /// <param name="errorRange">容错，单个色值范围内视为正确0~255</param>
        /// <param name="matchRate">图片匹配度，默认90%</param>
        /// <param name="isFindAll">是否查找所有相似的图片</param>
        /// <returns>返回查找到的图片的中心点坐标</returns>
        static List<System.Drawing.Point> FindPicture(string subPic, string parPic, System.Drawing.Rectangle searchRect, byte errorRange, double matchRate = 0.9, bool isFindAll = false)
         {
             List<System.Drawing.Point> ListPoint = new List<System.Drawing.Point>();
             var subBitmap = new Bitmap(subPic);
             var parBitmap = new Bitmap(parPic);
             int subWidth = subBitmap.Width;
             int subHeight = subBitmap.Height;
             int parWidth = parBitmap.Width;
             int parHeight = parBitmap.Height;
             if (searchRect.IsEmpty)
             {
                 searchRect = new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height);
             }

             var searchLeftTop = searchRect.Location;
             var searchSize = searchRect.Size;
             System.Drawing.Color startPixelColor = subBitmap.GetPixel(0, 0);
             var subData = subBitmap.LockBits(new System.Drawing.Rectangle(0, 0, subBitmap.Width, subBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
             var parData = parBitmap.LockBits(new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var byteArrarySub = new byte[subData.Stride * subData.Height];
            var byteArraryPar = new byte[parData.Stride * parData.Height];
            Marshal.Copy(subData.Scan0, byteArrarySub, 0, subData.Stride * subData.Height);
            Marshal.Copy(parData.Scan0, byteArraryPar, 0, parData.Stride * parData.Height);

            var iMax = searchLeftTop.Y + searchSize.Height - subData.Height;//行
            var jMax = searchLeftTop.X + searchSize.Width - subData.Width;//列

            int smallOffsetX = 0, smallOffsetY = 0;
            int smallStartX = 0, smallStartY = 0;
            int pointX = -1; int pointY = -1;
            System.Console.WriteLine("1111111111");
            for (int i = searchLeftTop.Y; i < iMax; i++)
            {
                for (int j = searchLeftTop.X; j < jMax; j++)
                {
                    //大图x，y坐标处的颜色值
                    int x = j, y = i;
                    int parIndex = i * parWidth * 4 + j * 4;
                    var colorBig = System.Drawing.Color.FromArgb(byteArraryPar[parIndex + 3], byteArraryPar[parIndex + 2], byteArraryPar[parIndex + 1], byteArraryPar[parIndex]);
                    ;
                    System.Console.WriteLine("2222222222222222");
                    if (ColorAEqualColorB(colorBig, startPixelColor, errorRange))
                    {
                        smallStartX = x - smallOffsetX;//待找的图X坐标
                        smallStartY = y - smallOffsetY;//待找的图Y坐标
                        int sum = 0;//所有需要比对的有效点
                        int matchNum = 0;//成功匹配的点
                        for (int m = 0; m < subHeight; m++)
                        {
                            for (int n = 0; n < subWidth; n++)
                            {
                                int x1 = n, y1 = m;
                                int subIndex = m * subWidth * 4 + n * 4;
                                var color = System.Drawing.Color.FromArgb(byteArrarySub[subIndex + 3], byteArrarySub[subIndex + 2], byteArrarySub[subIndex + 1], byteArrarySub[subIndex]);

                               sum++;
                               int x2 = smallStartX + x1, y2 = smallStartY + y1;
                               int parReleativeIndex = y2 * parWidth * 4 + x2 * 4;//比对大图对应的像素点的颜色
                               var colorPixel = System.Drawing.Color.FromArgb(byteArraryPar[parReleativeIndex + 3], byteArraryPar[parReleativeIndex + 2], byteArraryPar[parReleativeIndex + 1], byteArraryPar[parReleativeIndex]);
                                if (ColorAEqualColorB(colorPixel, color, errorRange))
                                {
                                    matchNum++;
                                }
                            }
                        }
                        if ((double)matchNum / sum >= matchRate)
                        {
                            System.Console.WriteLine("3333333333333333");
                            Console.WriteLine((double)matchNum / sum);
                            pointX = smallStartX + (int)(subWidth / 2.0);
                            pointY = smallStartY + (int)(subHeight / 2.0);
                            var point = new System.Drawing.Point(pointX, pointY);
                            if (!ListContainsPoint(ListPoint, point, 10))
                            {
                                System.Console.WriteLine("4444444444444444");
                                ListPoint.Add(point);
                            }
                            if (!isFindAll)
                            {
                                goto FIND_END;
                            }
                        }
                    }
                    //小图x1,y1坐标处的颜色值
                }
            }
        FIND_END:
            subBitmap.UnlockBits(subData);
            parBitmap.UnlockBits(parData);
            subBitmap.Dispose();
            parBitmap.Dispose();
            GC.Collect();
            return ListPoint;
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var parBitmap = new Bitmap("2.png");
            System.Drawing.Rectangle searchRect = new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height);

            var pointList = FindPicture("1.png", "2.png", searchRect, 255, 0.9, false);
            
            foreach (System.Drawing.Point temp in pointList)
            {
                System.Console.WriteLine(temp.X);
                System.Console.WriteLine(temp.Y);
            }
            System.Console.WriteLine(parBitmap.Width);
      
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
