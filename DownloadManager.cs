using System;
using System.IO;
using System.Net;

namespace Avantgarde.Lib
{
    
    public class DownloadManager
    {
        static int counter;
        // Overriding webclient class to set a timeout.
        public class WebClientWithTimeout : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest wr = base.GetWebRequest(address);
                wr.Timeout = 5000;
                return wr;
            }
        }

        public void DownloadRemoteFile(string url, string destination)
        {
            try
            {                
                using (WebClient wc = new WebClientWithTimeout())
                {
                    wc.DownloadFileCompleted += (s, e) =>
                    {
                        Console.Write("100%");
                        Console.WriteLine();
                        Utils.Log($"Downloaded {url} to {destination}");
                        counter = 0;
                    };
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                    wc.OpenRead(url);
                    // Size manipulations
                    string size;
                    long bytes = Convert.ToInt64(wc.ResponseHeaders["Content-Length"]);
                    double mb = Math.Round((bytes / 1024f) / 1024f, 2);
                    if (mb <= 0.00f)
                    {
                        size = Math.Round((bytes / 1024f), 2).ToString() + "KB";
                    }
                    else
                    {
                        size = mb.ToString() + "MB";
                    }                    
                    Utils.Log($"Downloading {Path.GetFileName(destination)}: {size}...");
                    wc.DownloadFileTaskAsync(new Uri(url), destination).Wait();
                }
                
            }
            catch (WebException we)
            {
                Utils.Log(we.Message, Utils.MsgType.error);
            }
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            counter++;
            if (counter % 500 == 0) // https://stackoverflow.com/questions/27261797/webclient-downloadprogresschanged-console-writeline-is-blocking-ui-thread
            {
                Console.Write(e.ProgressPercentage.ToString() + "%");
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }       
    }
}
