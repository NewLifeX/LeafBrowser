using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using NewLife;
using NewLife.Collections;
using NewLife.IO;
using NewLife.Log;
using NewLife.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeafBrower
{
    [ComVisible(true)]
    public partial class FrmMain : Form
    {
        private ChromiumWebBrowser Browser { get; set; }

        public FrmMain()
        {
            InitializeComponent();

            InitBrowser();
        }

        public void InitBrowser()
        {
            var settines = new CefSettings()
            {
                Locale = "zh-CN",
                AcceptLanguageList = "zh-CN",
                MultiThreadedMessageLoop = true
            };

            Cef.Initialize(settines);

            var bw = new ChromiumWebBrowser("");
            panel1.Controls.Add(bw);
            bw.Dock = DockStyle.Fill;

            var req = new MyRequestHandler
            {
                OnComplete = OnComplete
            };

            bw.RequestHandler = req;

            bw.FrameLoadStart += Browser_FrameLoadStart;
            bw.FrameLoadEnd += Web_FrameLoadEnd;
            bw.StatusMessage += Bw_StatusMessage;
            bw.TitleChanged += Bw_TitleChanged;

            Application.ApplicationExit += (s, e) => Cef.Shutdown();

            Browser = bw;
        }

        private void Bw_TitleChanged(Object sender, TitleChangedEventArgs e)
        {
            this.Invoke(() => Text = e.Title);
        }

        private void Bw_StatusMessage(Object sender, StatusMessageEventArgs e)
        {
            if (!e.Value.IsNullOrEmpty()) this.Invoke(() => lbStatus.Text = e.Value);
        }

        private void Form1_Load(Object sender, EventArgs e)
        {
#if DEBUG
            //var result = File.ReadAllText("result.json");
            //DecodeResult(result);
#endif

            var set = Setting.Current;
            txtUrl.Text = set.Url;
        }

        private void Browser_FrameLoadStart(Object sender, FrameLoadStartEventArgs e)
        {
            XTrace.WriteLine("FrameLoadStart {0}", e.Url);
        }

        private void BtnGo_Click(Object sender, EventArgs e)
        {
            var url = txtUrl.Text;
            if (url.IsNullOrEmpty()) return;

            var set = Setting.Current;
            set.Url = url;
            set.SaveAsync();

            Browser.Load(url);
        }

        private void Web_FrameLoadEnd(Object sender, FrameLoadEndEventArgs e)
        {
            XTrace.WriteLine("FrameLoadEnd {0}", e.Url);

            // 如果是主框架，则改变地址栏
            if (e.Frame != null && e.Frame.IsMain) this.Invoke(() => txtUrl.Text = e.Url);

            //var url = e.Url;
            //var result = await Browser.GetSourceAsync();
            //var html = result;
        }

        private void OnComplete(IRequest request, IResponse response, String result)
        {
            var url = request.Url;

            // 解码Json
            ThreadPoolX.QueueUserWorkItem(() => DecodeResult(url, result));
        }

        private void DecodeResult(String url, String result)
        {
            try
            {
                var p = url.IndexOf('?');
                if (p > 0) url = url.Substring(0, p);

                p = url.LastIndexOf('/');
                if (p > 0) url = url.Substring(p + 1);

                var name = url.Replace('/', '_');
                var fname = $"{name}_{++_gid}.json";
                fname = Setting.Current.DataPath.CombinePath(fname).GetFullPath().EnsureDirectory(true);
                File.WriteAllText(fname, result);

                var js = JsonConvert.DeserializeObject(result);

                //var js = new JsonParser(result).Decode();
                Decode(name, js);
            }
            catch (JsonReaderException) { }
        }

        private void Decode(String name, Object js)
        {
            if (js is IDictionary<String, JToken> jts && jts.Count > 0)
            {
                foreach (var item in jts)
                {
                    Decode(name, item.Value);
                }

                return;
            }

            if (js is IList<JToken> tokens)
            {
                if (tokens.Count > 0) WriteData(name, tokens);

                return;
            }
        }

        private static Int32 _gid;
        private void WriteData(String name, IList<JToken> list)
        {
            // 头部
            var headers = new List<String>();
            foreach (var item in list)
            {
                if (item is IDictionary<String, JToken> dic)
                {
                    foreach (var elm in dic)
                    {
                        if (!headers.Contains(elm.Key)) headers.Add(elm.Key);
                    }
                }
            }

            //var fname = $"{DateTime.Now:yyyyMMddHHmmss}_{++_gid}.csv";
            var fname = $"{name}_{++_gid}.csv";
            fname = Setting.Current.DataPath.CombinePath(fname).GetFullPath().EnsureDirectory(true);
            using (var csv = new CsvFile(fname, true))
            {
                //csv.Encoding = new UTF8Encoding(true);

                // 第一行写头部
                csv.WriteLine(headers);

                // 单行和多行
                foreach (var item in list)
                {
                    if (item is IDictionary<String, JToken> dic)
                        csv.WriteLine(headers.Select(e => dic[e]));
                    else
                        csv.WriteLine(new[] { item });
                }
            }
        }
    }
}