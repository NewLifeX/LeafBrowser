using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using NewLife.Log;

namespace WindowsFormsApp1
{
    [ComVisible(true)]
    public partial class Form1 : Form
    {
        public Uri Home { get; set; }

        private ChromiumWebBrowser Browser { get; set; }

        public Form1()
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

            bw.FrameLoadStart += Browser_FrameLoadStart;
            bw.FrameLoadEnd += Web_FrameLoadEnd;
            bw.StatusMessage += Bw_StatusMessage;
            bw.TitleChanged += Bw_TitleChanged;

            Browser = bw;
        }

        private void Bw_TitleChanged(Object sender, TitleChangedEventArgs e)
        {
            this.Invoke(() => Text = e.Title);
        }

        private void Bw_StatusMessage(Object sender, StatusMessageEventArgs e)
        {
            this.Invoke(() => lbStatus.Text = e.Value);
        }

        private void Form1_Load(Object sender, EventArgs e)
        {
            //var url = "http://www.chinacar.com.cn/Home/GonggaoSearch/GonggaoSearch/search_json?_dc=" + DateTime.Now.Ticks;

            //var dic = new Dictionary<String, String>();
            //dic["s7"] = "YCS04200-68";

            //var client = new HttpClient();
            //var rs = client.PostAsync(url, new FormUrlEncodedContent(dic)).Result;
            //var html = rs.Content.ReadAsStringAsync().Result;

            var url = "http://www.chinacar.com.cn/search.html";
            Home = new Uri(url);

            //Browser.Load(url);
        }

        private void Browser_FrameLoadStart(Object sender, FrameLoadStartEventArgs e)
        {
            XTrace.WriteLine("FrameLoadStart {0}", e.Url);
        }

        private void BtnGo_Click(Object sender, EventArgs e)
        {
            Browser.Load(txtUrl.Text);
        }

        private async void Web_FrameLoadEnd(Object sender, FrameLoadEndEventArgs e)
        {
            XTrace.WriteLine("FrameLoadEnd {0}", e.Url);

            //一个网页会调用多次,需要手动自己处理逻辑
            var url = e.Url;
            var result = await Browser.GetSourceAsync();
            var html = result;

            ////调用js
            //browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("alert('这是c#调用的js,给文本框赋值！')");
            ////txtAccount
            //browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('kw').value='在C#里面给页面文本框进行赋值'");
        }

        private void WebBrowser1_DocumentCompleted(Object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.Host != Home.Host) return;

            Text = e.Url + "";

            XTrace.WriteLine(e.Url + "");
        }

        private void WebBrowser1_NewWindow(Object sender, CancelEventArgs e)
        {
            //var wb = (WebBrowser)sender;
            //var newUrl = wb.Document.ActiveElement.GetAttribute("href");
            //if (newUrl.IsNullOrEmpty()) return;

            //wb.Url = new Uri(newUrl);

            //e.Cancel = true;
        }

        private void WebBrowser1_Navigating(Object sender, WebBrowserNavigatingEventArgs e)
        {
            //XTrace.WriteLine("Navigating {0}", e.Url);
        }

        //private void Wb_BeforeScriptExecute(Object pDispWindow)
        //{
        //    var text = webBrowser1.DocumentText;
        //    var vDocument = (IHTMLDocument2)webBrowser1.Document.DomDocument;

        //    //var script = "alert('hello');";
        //    //vDocument.parentWindow.execScript(script, "javascript");
        //}

        private void WebBrowser_BeforeNavigate2(Object pDisp, ref Object URL, ref Object Flags, ref Object TargetFrameName, ref Object postData, ref Object Headers, ref Boolean Cancel)
        {
            var uri = new Uri(URL + "");
            if (uri.Host.EndsWithIgnoreCase("baidu.com"))
            {
                Cancel = true;
                return;
            }

            XTrace.WriteLine("Navigate [{0}] {1}", TargetFrameName, URL);

            if (postData != null)
            {
                var postDataText = System.Text.Encoding.ASCII.GetString(postData as Byte[]);
                XTrace.WriteLine("POST {0}", postDataText);
            }
        }

        //private void Wb_NewWindow3(ref Object ppDisp, ref Boolean Cancel, UInt32 dwFlags, String bstrUrlContext, String bstrUrl)
        //{
        //    XTrace.WriteLine("NewWindow3 [{0}] {1}", bstrUrlContext, bstrUrl);

        //    Cancel = true;
        //    webBrowser1.Navigate(bstrUrl);
        //}
    }
}
