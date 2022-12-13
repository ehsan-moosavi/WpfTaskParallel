using System;
using System.Windows;
using System.Windows.Shapes;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;


namespace WpfTaskParallel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        public MainWindow()
        {
           // InitializeComponent();
        }

       // private void InitializeComponent()
       // {
        //    throw new NotImplementedException();
       // }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            cancelToken.Cancel();
            // This will be updated shortly
        }

        private void CmdProcess_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => ProcessFiles());
            
        }

        private void ProcessFiles()
            
        {
            ParallelOptions paropt = new ParallelOptions();
            paropt.CancellationToken = cancelToken.Token;
            paropt.MaxDegreeOfParallelism= System.Environment.ProcessorCount;
            // Load up all *.jpg files, and make a new folder for the modified data.
            string[] files = Directory.GetFiles(@".\TestPictures", "*.jpg", SearchOption.AllDirectories);
            string newDir = @".\ModifiedPictures";
            Directory.CreateDirectory(newDir);
            // Process the image data in a blocking manner.
            try
            {
                Parallel.ForEach(files, paropt, currentFile =>
                 {
                     string filename = System.IO.Path.GetFileName(currentFile);
                     using (Bitmap bitmap = new Bitmap(currentFile))
                     {
                         bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                         bitmap.Save(System.IO.Path.Combine(newDir, filename));
                    // Print out the ID of the thread processing the current image.
                    this.Dispatcher.Invoke((Action)delegate
                         {
                             this.Title = $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";
                         }
                     );
                     }
                 }
                );
                this.Dispatcher.Invoke((Action)delegate
                {
                    this.Title = "Done!";
                });
            }
            catch(OperationCanceledException ex)
            {
                this.Dispatcher.Invoke((Action)delegate{
                    this.Title = ex.Message;
                });
            }

            
               
               
        }

        
    }
}

