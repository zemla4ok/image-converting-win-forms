using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Stopwatch sWatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF,*.PNG)|*.BMP;*.JPG;*.GIF,*.PNG|All files(*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image = new Bitmap(ofd.FileName);
                }
                catch
                {
                    MessageBox.Show("Error in opening");
                }
            }
        }


        // синхронно однопоточно
        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                this.sWatch.Start();
                Bitmap input = new Bitmap(pictureBox1.Image);
                Bitmap output = new Bitmap(input.Width, input.Height);

                for (int j = 0; j < input.Height; j++)
                {
                    for (int i = 0; i < input.Width; i++)
                    {
                        UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());

                        float R = (float)((pixel & 0x00FF0000) >> 16);
                        float G = (float)((pixel & 0x0000FF00) >> 8);
                        float B = (float)((pixel & 0x000000FF) >> 0);

                        R = G = B = (R + G + B) / 3.0f;

                        UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

                        output.SetPixel(i, j, Color.FromArgb((int)newPixel));
                    }
                }
                this.sWatch.Stop();
                pictureBox2.Image = output;
                MessageBox.Show("sync one thread: " + this.sWatch.ElapsedMilliseconds.ToString() + "ms");
                this.sWatch.Reset();
            }
        }

        //фсинхронно многопоточно
        private void button3_Click(object sender, EventArgs e)
        {
            this.sWatch.Start();
            this.grayStart();
        }

        async void grayStart()
        {
            await Task.Run(() => this.gray());
        }

        async void gray()
        {
            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                Bitmap output = new Bitmap(input.Width, input.Height);

                for (int j = 0; j < input.Height; j++)
                {
                    for (int i = 0; i < input.Width; i++)
                    {
                        UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());

                        float R = (float)((pixel & 0x00FF0000) >> 16);
                        float G = (float)((pixel & 0x0000FF00) >> 8);
                        float B = (float)((pixel & 0x000000FF) >> 0);

                        R = G = B = (R + G + B) / 3.0f;

                        UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

                        output.SetPixel(i, j, Color.FromArgb((int)newPixel));
                    }
                }
                this.sWatch.Stop();
                pictureBox2.Image = output;
                MessageBox.Show("async one thread: " + this.sWatch.ElapsedMilliseconds.ToString() + "ms");
                this.sWatch.Reset();
            }
        }


        // синхронно однопоточно
        Bitmap outputSync = null;
        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                this.sWatch.Start();
                Bitmap input = new Bitmap(pictureBox1.Image);
                this.outputSync = new Bitmap(input.Width, input.Height);

                Task part1 = new Task(this.grayPart1Sync);
                Task part2 = new Task(this.grayPart2Sync);

                part1.Start();
                part2.Start();

                Task.WaitAll(part1, part2);
                this.sWatch.Stop();
                this.pictureBox2.Image = this.outputSync;
                MessageBox.Show("sync multithread: " + this.sWatch.ElapsedMilliseconds.ToString() + "ms");
                this.sWatch.Reset();
            }
        }

        void grayPart1Sync()
        {
            Bitmap input = new Bitmap(pictureBox1.Image);

            for (int j = 0; j < input.Height / 2; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());

                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)((pixel & 0x000000FF) >> 0);

                    R = G = B = (R + G + B) / 3.0f;

                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

                    lock (this.outputSync)
                    {
                        this.outputSync.SetPixel(i, j, Color.FromArgb((int)newPixel));
                    }
                }
            }
        }

        void grayPart2Sync()
        {
            Bitmap input = new Bitmap(pictureBox1.Image);

            for (int j = input.Height / 2; j < input.Height; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());

                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)((pixel & 0x000000FF) >> 0);

                    R = G = B = (R + G + B) / 3.0f;

                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

                    lock (this.outputSync)
                    {
                        this.outputSync.SetPixel(i, j, Color.FromArgb((int)newPixel));
                    }
                }
            }
        }

        // многопоточно асинхронно
        private void button5_Click(object sender, EventArgs e)
        {
            this.sWatch.Start();
            this.grayStartAsync();
        }

        async void grayStartAsync()
        {
            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                this.outputSync = new Bitmap(input.Width, input.Height);

                await Task.Run(() => this.grayPart1Async());
                await Task.Run(() => this.grayPart2Async());

                this.sWatch.Stop();
                this.pictureBox2.Image = outputSync;
                MessageBox.Show("async multithread: " + this.sWatch.ElapsedMilliseconds.ToString() + "ms");
                this.sWatch.Reset();
            }
        }

        async void grayPart1Async()
        {
            Bitmap input = new Bitmap(pictureBox1.Image);

            for (int j = 0; j < input.Height / 2; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());

                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)((pixel & 0x000000FF) >> 0);

                    R = G = B = (R + G + B) / 3.0f;

                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

                    this.outputSync.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            }
        }

        async void grayPart2Async()
        {
            Bitmap input = new Bitmap(pictureBox1.Image);

            for (int j = input.Height / 2; j < input.Height; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());

                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)((pixel & 0x000000FF) >> 0);

                    R = G = B = (R + G + B) / 3.0f;

                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

                    this.outputSync.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            }
        }
    }
}
