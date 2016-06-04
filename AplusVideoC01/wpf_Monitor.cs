using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using VideoMediaElementCore;

namespace AplusVideoC01
{
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [ComVisible(true)]
    public interface IClientEvent
    {
        //Add a DispIdAttribute to any members in the source interface to specify the COM DispId.
        //Let methods will be visible in JS
        [DispId(6)]
        void CapturePictureComplete(string val);
    }
    
    [ComVisible(true)]
    [Guid("2B2CF1DD-D200-4C05-B974-2111B92C9BB2")]
    [ClassInterface(ClassInterfaceType.AutoDual), ComSourceInterfaces(typeof(IClientEvent))]    //Implementing interface that will be visible in JS
    public partial class wpf_Monitor : System.Windows.Forms.UserControl
    {
        //********************************************************************************
       
        #region Private Properties

        private int m_Attrflag = 0xFFFF;

        private string IP = "192.168.1.101";
        private int PORT = 8888;
        private string UID = "admin";
        private string PWD = "1234";
        private string _ErrDesc = "";
        private int channel = 0;

        private string snap_PATH = "D:\\Print Only";
        private string snap_FILENAME = "1111";
        private int snap_EXT = 1;               //0:tiff, 1:jpg, 2:bmp, 3:gif

        private System.Windows.Controls.Canvas wpfmain;             //WPF Element
        private MediaElementCore apv;
        
        #endregion

        //********************************************************************************

        #region Public Properties

        public string ErrDesc
        {
            get { return _ErrDesc; }
        }

        public bool IsFocus
        {
             set 
             {
                 if(value)
                     this.BackColor = System.Drawing.Color.Red;
                 else
                     this.BackColor = System.Drawing.Color.Blue;

             }
        }

        public string CamID
        {
            get { return channel.ToString().PadLeft(2,'0'); }
        }

        public lsStatus Status;

        public enum lsStatus
        {
            Login,
            Logout
        };

        #endregion

        //********************************************************************************
        
        #region Events

        public delegate void CapturePictureCompleteHandler(string SavePath);

        public event CapturePictureCompleteHandler CapturePictureComplete;

        //private void wpf_Monitor_CapturePictureComplete(string SavePath)
        //{
        //    CapturePictureComplete(SavePath);
        //}

        #endregion
        
        //********************************************************************************

        #region Private Method

        private void wpfmain_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Canvas _wpfmain = sender as Canvas;
            foreach (UIElement uee in _wpfmain.Children)
            {
                VideoMediaElementCore.MediaElementCore ele = uee as VideoMediaElementCore.MediaElementCore;
                ele.set_focus(0);
            }
        }

        private void snap_shot_callback(int camid, string label, string time, byte[] img_data, int size, int ww, int hh, int fmt)
        {
            //Console.WriteLine("snap_shot_callback camid={0}, label={1}, img_size{2} {3}x{4}.", camid, label, size, ww, hh);
            string save_path = "";

            WriteableBitmap snapBmp = null;
            try
            {
                System.Drawing.Imaging.PixelFormat pfmt = new System.Drawing.Imaging.PixelFormat();
                if (fmt == (int)VideoFmt.VIDEO_RGB24)
                {
                    snapBmp = new WriteableBitmap(ww, hh, 96, 96, PixelFormats.Rgb24, null);
                    pfmt = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                }
                else
                {
                    snapBmp = new WriteableBitmap(ww, hh, 96, 96, PixelFormats.Bgr565, null);
                    pfmt = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                }

                int bitsPerPixel = 16;
                if (fmt == (int)VideoFmt.VIDEO_RGB24) bitsPerPixel = 24;
                int stride = (ww * bitsPerPixel + 7) / 8;
                snapBmp.Lock();
                snapBmp.WritePixels(new Int32Rect(0, 0, ww, hh), img_data.ToArray(), stride, 0);
                var bmp = new System.Drawing.Bitmap(snapBmp.PixelWidth, snapBmp.PixelHeight,
                                         snapBmp.BackBufferStride,
                                         pfmt,
                                         snapBmp.BackBuffer);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
                System.Drawing.PointF pt = new System.Drawing.PointF();
                pt.X = (float)40.0;
                pt.Y = (float)(snapBmp.PixelHeight - 40);
                g.DrawString(time + "-" + label, new System.Drawing.Font("Arial", (float)20.0), new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 0, 0)), pt);
                g.Dispose();
                bmp.Dispose();
                snapBmp.AddDirtyRect(new System.Windows.Int32Rect(0, 0, ww, hh));
                snapBmp.Unlock();

                BitmapSource tmp = snapBmp.Clone();

                if (snap_PATH == "") snap_PATH = "D:\\";
                if (snap_FILENAME == "") snap_FILENAME = "CH_" + (camid + 1).ToString("D2") + "-" + label;

                if (snap_EXT == 0)
                {
                    save_path = snap_PATH + snap_FILENAME + ".tiff";

                    using (System.IO.FileStream stream = new System.IO.FileStream(save_path, System.IO.FileMode.Create))
                    {
                        TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(tmp));
                        encoder.Save(stream);
                        stream.Close();
                    }
                }
                else if (snap_EXT == 1)
                {
                    save_path = snap_PATH + snap_FILENAME + ".jpg";

                    using (System.IO.FileStream stream = new System.IO.FileStream(save_path, System.IO.FileMode.Create))
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(tmp));
                        encoder.Save(stream);
                        stream.Close();
                    }
                }
                else if (snap_EXT == 2)
                {
                    save_path = snap_PATH + snap_FILENAME + ".bmp";

                    using (System.IO.FileStream stream = new System.IO.FileStream(save_path, System.IO.FileMode.Create))
                    {
                        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(tmp));
                        encoder.Save(stream);
                        stream.Close();
                    }
                }
                else if (snap_EXT == 3)
                {
                    save_path = snap_PATH + snap_FILENAME + ".gif";

                    using (System.IO.FileStream stream = new System.IO.FileStream(save_path, System.IO.FileMode.Create))
                    {
                        GifBitmapEncoder encoder = new GifBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(tmp));
                        encoder.Save(stream);
                        stream.Close();
                    }
                }

                //拋出一個完成存檔的event
                CapturePictureComplete(save_path);
            }
            catch (Exception ex)
            {
                //錯誤則拋出例外訊息
                CapturePictureComplete("CH_" + (camid + 1).ToString("D2") + ": " + ex.Message);
            }    
            
        }

        private Int64 ConvertToTimestamp(DateTime d)
        {
            //GMT time
            Int64 timestamp = Convert.ToInt64((d - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            return timestamp;
        }

        internal enum VideoFmt
        {
            VIDEO_RGB565 = 0,
            VIDEO_BGR565,
            VIDEO_RGB24
        };

        #endregion

        //********************************************************************************

        public wpf_Monitor()
        {
            m_Attrflag = 0;
            InitializeComponent();
        }

        public wpf_Monitor(int flag)
        {
            m_Attrflag = flag;
            InitializeComponent();
        }

        private void wpf_Monitor_Load(object sender, EventArgs e)
        {
            //Loading the video elements. (WPF)
            wpfmain = new System.Windows.Controls.Canvas();
            wpfmain.Width = elementHost1.Width;
            wpfmain.Height = elementHost1.Height;
            wpfmain.Name = "wpfmain";
            elementHost1.Child = wpfmain;
        }

        /// <summary>
        /// 使用者輸入IP、Port、UserName、PassWord，登入至DVR
        /// </summary>
        /// <param name="sIP">IP 位址</param>
        /// <param name="sPort">使用port</param>
        /// <param name="sUID">使用者帳號</param>
        /// <param name="sPWD">使用者密碼</param>
        /// <returns></returns>
        public int Device_Login(string sIP, string sPort, string sUID, string sPWD)
        {
            int rtnValue = 0;

            Console.WriteLine("Device_Login sIP = {0}", sIP);
            try
            {
                IP = sIP;
                PORT = int.Parse(sPort);
                UID = sUID;
                PWD = sPWD;

                rtnValue = 1;
            }
            catch(Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            Console.WriteLine("Device_Login IP = {0}", IP);
            return rtnValue;
        }

        /// <summary>
        /// 使用者登出
        /// </summary>
        public void Device_Logout()
        {
            if (apv != null) 
                apv.stop();

            apv = null;
            wpfmain.Children.Clear();
            //Status = lsStatus.Logout;
            //elementHost1.Child = null;
        }

        /// <summary>
        /// 單一鏡頭即時影像播放
        /// </summary>
        /// <param name="iCH">鏡頭編號 (0~15)</param>
        /// <param name="iAudio">傳輸聲音資料 
        /// 0: do not send audio data 
        /// 1: send audio data, if any
        /// </param>
        /// <param name="iStream">使用串流 
        /// 0: use primary stream
        /// 1: try to use secondary stream
        /// </param>
        /// <returns></returns>
        public int Device_RealPlay(int iCH, int iAudio, int iStream)
        {
            int rtnVal = 0;
            try
            {
                Device_Logout();
                channel = iCH;

                apv = new VideoMediaElementCore.MediaElementCore(iCH + 1, 0);
                wpfmain.Children.Add(apv);
                wpfmain.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(wpfmain_MouseLeftButtonUp);
                apv.set_size(elementHost1.Width, elementHost1.Height);
                //apv.set_aes_key("1234567890123456");
                Console.WriteLine("Ip = {0} Port = {1}", IP, PORT);
                apv.set_source(0, IP, PORT, iCH, (byte)iAudio, (byte)iStream, 1);
                apv.set_print_msg(1);
                //MessageBox.Show();
                //wpfmain.Children.Add(apv);

                //0 => send request to get one frame
                //1 => send request to get one I-frame
                //2 => play continuous stream
                apv.set_read_mode(2);
                apv.play();

                rtnVal = 1;
            }
            catch(Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            return rtnVal;
        }

        /// <summary>
        /// 停止即時播放
        /// </summary>
        public void Device_StopRealPlay()
        {
            if (apv != null)
            {
                apv.stop();
                apv = null;
            }
        }

        /// <summary>
        /// 開起音效
        /// </summary>
        public void Device_OpenSound()
        {
            apv.play_audio(1);
        }

        /// <summary>
        /// 關閉音效
        /// </summary>
        public void Device_CloseSound()
        {
            apv.play_audio(0);
        }

        /// <summary>
        /// 擷取鏡頭圖片
        /// </summary>
        /// <param name="sPath">儲存路徑</param>
        /// <param name="sFileName">主檔名(不含副檔名)</param>
        /// <param name="iExtType">檔案格式。0: Tiff, 1: Jpeg, 2: Bmp, 3: Gif</param>
        /// <returns></returns>
        public int Device_CapturePicture(string sPath, string sFileName, int iExtType)
        {
            int rtnVal = 0;
            try
            {
                //ivm.CapturePictureComplete += ivm_CapturePictureComplete;
                snap_PATH = sPath;
                snap_FILENAME = sFileName;
                snap_EXT = iExtType;
                
                apv.set_snap_shot_cb(snap_shot_callback);
                apv.set_snap_shot();

                rtnVal = 1;
            }
            catch (Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            return rtnVal;
        }

        /// <summary>
        /// 回放開始時間設定
        /// </summary>
        /// <param name="iCH">鏡頭編號 (0~15)</param>
        /// <param name="iAudio">傳輸聲音資料 
        /// 0: do not send audio data 
        /// 1: send audio data, if any
        /// </param>
        /// <param name="sStartTime">開始時間(yyyy/MM/dd HH:mm)</param>
        /// <returns></returns>
        public int Device_PlayBackByTime(int iCH, int iAudio, string sStartTime)
        {
            return _PlayBackByTime(iCH, iAudio, sStartTime, 1);
        }

        public int Device_PlayBackByTimeL(int iCH, int iAudio, string sStartTime)
        {
            return _PlayBackByTime(iCH, iAudio, sStartTime, 0);
        }

        private int _PlayBackByTime(int iCH, int iAudio, string sStartTime, int is_utc)
        {

            int rtnVal = 0;
            try
            {
                Device_Logout();
                channel = iCH;

                apv = new VideoMediaElementCore.MediaElementCore(iCH + 1, 1);
                wpfmain.Children.Add(apv);
                wpfmain.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(wpfmain_MouseLeftButtonUp);
                apv.set_size(elementHost1.Width, elementHost1.Height);
                //apv.set_aes_key("1234567890123456");
                if (is_utc == 0) apv.set_source_l(1, IP, PORT, iCH, (byte)iAudio, ConvertToTimestamp(DateTime.Parse(sStartTime)));
                else apv.set_source_l(1, IP, PORT, iCH, (byte)iAudio, ConvertToTimestamp(DateTime.Parse(sStartTime)));
                //apv.set_playback_cmd(1, 1); //2014/07/21_CTS
                apv.set_print_msg(1);

                apv.play();
                System.Threading.Thread.Sleep(1000);
                apv.set_playback_cmd(1, 1);

                rtnVal = 1;
            }
            catch (Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            return rtnVal;
        }


        /// <summary>
        /// 回放開始時間設定
        /// </summary>
        /// <param name="sStartTime"></param>
        /// <returns></returns>
        public int Device_SeekByTime(string sStartTime)
        {
            int rtnVal = 0;
            try
            {
                DateTime d = DateTime.Parse(sStartTime);
                apv.seek_stream(channel, d.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            return rtnVal;
        }

        /// <summary>
        /// 停止回放
        /// </summary>
        public void Device_StopPlayBack()
        {
            if (apv != null)
            {
                apv.stop();
                apv = null;
            }
        }

        /// <summary>
        /// 回放控制
        /// </summary>
        /// <param name="iState">控制類別
        /// 0: pause stream 
        /// 1: play stream
        /// </param>
        /// <param name="iSpeed">撥放速度
        /// 1 : play speed at 1X 
        /// 2,4,8,16 : play fast forward (2,4,8,16)
        /// -1,-2,-4,-8,-16 : play backward (-1,-2,-4,-8,-16)
        /// </param>
        /// <returns></returns>
        public int Device_PlayBackControl(int iState, int iSpeed)
        {
            int rtnVal = 0;
            try
            {
                apv.set_playback_cmd(iState, iSpeed);
            }
            catch (Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            return rtnVal;
        }

        /// <summary>
        /// 擷取鏡頭圖片
        /// </summary>
        /// <param name="sPath">儲存路徑</param>
        /// <param name="sFileName">主檔名(不含副檔名)</param>
        /// <param name="iExtType">檔案格式。0: Tiff, 1: Jpeg, 2: Bmp, 3: Gif</param>
        /// <returns></returns>
        public int Device_PlayBackCaptureFile(string sPath, string sFileName, int iExtType)
        {
            int rtnVal = 0;
            try
            {
                snap_PATH = sPath;
                snap_FILENAME = sFileName;
                snap_EXT = iExtType;

                apv.set_snap_shot_cb(snap_shot_callback);
                apv.set_snap_shot();

                rtnVal = 1;
            }
            catch (Exception ex)
            {
                _ErrDesc = ex.Message;
            }

            return rtnVal;
        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
