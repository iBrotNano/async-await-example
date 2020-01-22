using RetryHelper;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncAwaitUI
{
    public partial class Form1 : Form
    {
        #region Fields

        private static CancellationTokenSource _cancellationTokenSource;

        private static CancellationToken _cancellationToken;

        private static Progress<bool> _progress;

        #endregion Fields

        #region Constructors

        public Form1()
        {
            InitializeComponent();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _progress = new Progress<bool>();
            _progress.ProgressChanged += OnProgressChanged;
        }

        #endregion Constructors

        #region Methods

        private void WriteToTextbox(string text)
        {
            textBox1.Text += text
                + Environment.NewLine;
        }

        private void OnProgressChanged(object sender, bool e)
        {
            WriteToTextbox($"Progress: {e.ToString()}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();

            if (_cancellationToken.IsCancellationRequested)
            {
                WriteToTextbox("Task canceled!");
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 1");

            // Case 1: Asynchrone Methode ohne await. Die Methode wird
            // ausgeführt, es wird aber nicht darauf gewartet, dass die Methode
            // beendet wird. Die UI bleibt responsiv.
            Retry.RetryActionAsync(() => WriteToTextbox($"Executed and nothing is returned.")
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 2");

            // Case 2: Asynchrone Methode ohne await. Die Methode wird
            // ausgeführt, es wird aber nicht darauf gewartet, dass die Methode
            // beendet wird. Die Methode unterscheidet sich von der Methode oben
            // nur dadurch, dass das Resultat der Operation ins Nichts gesendet
            // wird. _ = ist die Syntax dafür. Der Compiler weißt, dass mich das
            // Ergebnis der gestarteten Methode nicht interessiert und die grüne
            // Wellenlinie aus dem ersten Beispiel ist verschwunden.
            _ = Retry.RetryActionAsync(() => WriteToTextbox($"Executed and nothing is returned.")
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 3");

            try
            {
                // Case 3: Asynchrone Methode mit await. Die Methode wird
                // ausgeführt und es wird auf deren Beendigung gewartet. Die
                // Methode läuft erst weiter, wenn das Polling beendet wurde.
                // Die UI bleibt aber responsiv. Das ist der Unterschied zur
                // synchronen Ausführung.
                await Retry.RetryActionAsync(() => WriteToTextbox($"Executed and nothing is returned.")
                    , TimeSpan.FromSeconds(1)
                    , _cancellationToken
                    , _progress);
            }
            catch { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Sync");

            // Sync. Die UI friert ein.
            Retry.RetryAction(() => Console.WriteLine($"Executed and nothing is returned.")
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 4");

            // Case 4: Code wird synchron ausgeführt auch.
            Retry.RetryActionAsync(() => Console.WriteLine($"Executed and nothing is returned.")
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress)
                .GetAwaiter()
                .GetResult();
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 5");

            try
            {
                // Case 5: Eine Action wird mit einem Parameter aufgerufen. Die
                // UI fiert nicht ein, weil die asynchrone Methode verwendet
                // wurde. Es wird auf das Resultat gewartet. Die Methode wird an
                // der Stelle des await fortgesetzt.
                await Retry.RetryActionAsync(RunWithParameter
                    , 1
                    , TimeSpan.FromSeconds(1)
                    , _cancellationToken
                    , _progress);
            }
            catch (TaskCanceledException) { }
        }

        private void RunWithParameter<T>(T param)
        {
            WriteToTextbox(param.ToString());
        }

        private void button8_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 6");

            // Case 6: Func<Task> Eine Methode, die einen Task zurück gibt. Auf
            // das Ende des Tasks wird innerhalb der Poll-Methode gewartet. In
            // diesem Fall ist der Task nicht gestartet. Das heißt er hat noch
            // nicht mit seiner Arbeit begonnen. Der Task wird innerhalb von
            // PollFuncAsync gestartet.
            _ = Retry.RetryFuncAsync(CreateNotRunningTask
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);
        }

        private Task CreateNotRunningTask()
        {
            return new Task(() => new Random().Next());
        }

        private void button9_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 7");

            // Case 7: Func<Task> Eine Methode, die einen Task zurück gibt. Auf
            // das Ende des Tasks wird innerhalb der Poll-Methode gewartet. In
            // diesem Fall ist der Task bereits gestartet.
            _ = Retry.RetryFuncAsync(CreateRunningTask
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);
        }

        private Task CreateRunningTask()
        {
            return Task.Run(() => new Random().Next());
        }

        private async void button10_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 8");

            try
            {
                // Case 8: Func<T, Task> Eine Methode, die einen Task zurück
                // gibt und einen Parameter übergeben bekommt wird asynchron
                // ausgeführt. Die UI friert nicht ein. Es wird als Parameter
                // ein Tuple übergeben. Auf diese Weise kann man die
                // Pollfunktionen nutzen und mehrere Parameter übergeben.
                await Retry.RetryFuncAsync(CreateParameterizedTask
                    , new Tuple<int, int>(1, 100)
                    , TimeSpan.FromSeconds(1)
                    , _cancellationToken
                    , _progress);
            }
            catch (TaskCanceledException) { }
        }

        private Task CreateParameterizedTask(Tuple<int, int> parameter)
        {
            return new Task(() => new Random().Next(parameter.Item1, parameter.Item2));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 9");

            // Case 9: Func<Task<T>> Die Pollfunktion führt eine asynchrone
            // Methode synchron aus und gibt deren Resultat zurück.
            int result = Retry.RetryFunc(CreateTaskWithReturnValue
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);

            WriteToTextbox(result.ToString());
        }

        private Task<int> CreateTaskWithReturnValue()
        {
            return new Task<int>(() => GetSpecialNumber());
        }

        private int GetSpecialNumber(Random random = null)
        {
            if (random is null)
                random = new Random();

            int number = random.Next();

            if (number % 13 == 0)
                return number;
            else
                return GetSpecialNumber(random);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 10");

            // Case 10: Func<T, Task<TResult>> Die Pollfunktion führt eine
            // asynchrone Methode mit Parameter synchron aus und gibt deren
            // Resultat zurück.
            int result = Retry.RetryFunc(CreateTaskWithReturnValueAndParameterInFunc
                , new Random()
                , TimeSpan.FromSeconds(1)
                , _cancellationToken
                , _progress);

            WriteToTextbox(result.ToString());
        }

        private Task<int> CreateTaskWithReturnValueAndParameterInFunc(Random random)
        {
            return new Task<int>(() => GetSpecialNumber(random));
        }

        private async void button13_Click(object sender, EventArgs e)
        {
            WriteToTextbox("Case 11");

            // Case 11: Func<T, Task<TResult>> Die Pollfunktion führt eine
            // asynchrone Methode aus, die Exceptions wirft und erst nach einer
            // Weile erfolgreich ist.

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                int result = await Retry.RetryFuncAsync(CreateDelayedSuccessfulTask
                    , stopWatch
                    , TimeSpan.FromSeconds(1)
                    , _cancellationToken
                    , _progress);

                WriteToTextbox(result.ToString());
            }
            catch (TaskCanceledException) { }
        }

        private Task<int> CreateDelayedSuccessfulTask(Stopwatch stopwatch)
        {
            if (stopwatch.ElapsedMilliseconds < 30000)
                return CreateExceptionThrowingTask();
            else
                return CreateTaskWithReturnValue();
        }

        private Task<int> CreateExceptionThrowingTask()
        {
            return new Task<int>(() =>
            {
                throw new Exception("Test");
            });
        }

        #endregion Methods
    }
}