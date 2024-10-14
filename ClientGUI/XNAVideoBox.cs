using ClientCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenCvSharp;
using OpenCvSharp.Internal;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

//------------------------------------------
//Author:https://space.bilibili.com/38860280
//视频播放器
//------------------------------------------

namespace ClientGUI
{
    public class XNAVideoBox : XNAPanel
    {

        public XNAVideoBox(WindowManager windowManager) : base(windowManager)
        {
            Game.Activated += (s, e) =>
            {
                actived = true;
            };

            Game.Deactivated += (s, e) =>
            {
                actived = false;
            };

            try
            {
                VideoAllowed = UserINISettings.Instance.ClientAnimationBackground.Value;
            }
            catch (Exception) { };
        }

        private bool _isVideoPaused = false;

        private bool VideoAllowed = true;

        private Texture2D _oldCache;
        private Texture2D _cachedVideo;

        private List<Texture2D> _cachedTextures = new List<Texture2D>();

        private static Object _lock = new Object();

        public string VideoSource { get; set; } = @"./Resources/bg.mp4";

        private bool isTerribleCpuUsage = false;

        private bool actived = true;

        private void UpdateVideo()
        {
            try
            {


                if (Environment.Is64BitProcess)
                {
                    WindowsLibraryLoader.Instance.LoadLibrary("OpenCvSharpExtern", new List<string>() { "./Resources/Binaries/dll/x64/" });
                    WindowsLibraryLoader.Instance.LoadLibrary("opencv_ffmpeg400_64", new List<string>() { "./Resources/Binaries/dll/x64/" });
                }
                else
                {
                    WindowsLibraryLoader.Instance.LoadLibrary("OpenCvSharpExtern", new List<string>() { "./Resources/Binaries/dll/x86/" });
                    WindowsLibraryLoader.Instance.LoadLibrary("opencv_ffmpeg400", new List<string>() { "./Resources/Binaries/dll/x86/" });
                }

                var _videoCapture = new VideoCapture(VideoSource);



                while (true)
                {
                    //var sw = new Stopwatch();
                    //sw.Start();
                    //while (sw.ElapsedMilliseconds <= 25)
                    //{
                    //    Thread.Sleep(0);
                    //}
                    //sw.Stop();
                    //Thread.Sleep(0);
                    //模拟缓冲机制，缓冲多个贴图
                    if (_cachedTextures.Count() < 10)
                    {
                        if (LastStop != null)
                            if (DateTime.Now.Subtract(LastStop.Value).TotalSeconds > 10)
                                _isVideoPaused = true; //超过5s没update的话暂停后台线程

                        if (!_isVideoPaused)
                        {
                            if (_videoCapture.PosFrames >= _videoCapture.FrameCount - 1)
                                _videoCapture.PosFrames = 0;
                            using (var image = new Mat())
                            {
                                if (_videoCapture.Read(image))
                                {
                                    using (var ms = image.ToMemoryStream(".jpeg", new ImageEncodingParam(ImwriteFlags.JpegQuality, 80)))
                                    {
                                        var cache = Texture2D.FromStream(Game.GraphicsDevice, ms);
                                        lock (_lock)
                                        {
                                            _cachedTextures.Add(cache);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(20);
                }
            }catch (Exception ex)
            {
                Logger.Log("Video Update Error" + ex.Message);
            }
        }

        private Thread _videoThread;

        public override void Initialize()
        {
            base.Initialize();

            if (_videoThread == null)
            {
                //后台线程处理视频
                _videoThread = new Thread(UpdateVideo);
                _videoThread.IsBackground = true;
                _videoThread.Start();
            }

        }

        private DateTime? LastStop;

        //private Stopwatch StopWatch = Stopwatch.StartNew();


        public static bool FindYR()
        {
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.Contains("gamemd"))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            try
            {
                LastStop = DateTime.Now;
                if (!_isVideoPaused)
                {
                    //if (StopWatch.ElapsedMilliseconds >= 25)
                    //{
                        //StopWatch.Restart();
                        if (_cachedTextures.Count() > 0)
                        {
                            if (_oldCache != null)
                            {
                                _oldCache.Dispose();
                            }

                            if (_cachedVideo != null)
                            {
                                _oldCache = _cachedVideo;
                            }

                            lock (_lock)
                            {
                                _cachedVideo = _cachedTextures.First();
                                _cachedTextures.RemoveAt(0);
                            }
                        //}
                    }
                }

                if (isTerribleCpuUsage == false && actived && VideoAllowed && !FindYR())
                {
                    _isVideoPaused = false;
                }
                else
                {
                    _isVideoPaused = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Video Update Error" + ex.Message);
            }

            base.Update(gameTime);
        }

        private int cpuCheckDelay;

     

        protected void DrawVideo()
        {
            Color remapColor = base.RemapColor;
            try
            {
                //var pwidth = Parent.Width;
                //var pheight = Parent.Height;

                //var scale = 1d;


                //this.Width = pwidth;
                //scale = pwidth / this.Width;

                //this.Height = (int)scale * pheight;
                
                if(_cachedVideo!=null)
                {
                    //DrawTexture(_cachedVideo, new Rectangle(0, (pheight - this.Height) / 2, this.Width, this.Height), remapColor);
                    DrawTexture(_cachedVideo, new Rectangle(0, 0, this.Width, this.Height), remapColor);
                }
            }
            catch (Exception ex)
            {
                //Logger.Log(ex.Message);
            }
            //if (BackgroundTexture == null)
            //{
            //    return;
            //}

            


        }

      

        public override void Draw(GameTime gameTime)
        {
            //DrawPanel();
            //if (DrawBorders)
            //{
            //    DrawPanelBorders();
            //}
            //if(Visible)
            //{
            //    DrawVideo();
            //}

            DrawVideo();

            base.Draw(gameTime);
        }

    }
}
