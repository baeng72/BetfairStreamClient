using System.Text;
using System.Threading.Channels;

namespace BetfairStreamClient
{
    public class Logger
    {
        
        private readonly Channel<string> _logChannel;
        private Task _consumerTask;
        private CancellationToken _cancellationToken;
        private string? _currentFilePath;
        private string? _activeWorkerFilePath;

        public Logger()
        {
            var options = new BoundedChannelOptions(10000) { SingleWriter = false, SingleReader = true, FullMode = BoundedChannelFullMode.DropWrite };
            _logChannel = Channel.CreateBounded<string>(options);
            
        }
        public void Init(string filePath, CancellationToken cancellationToken)
        {
            _cancellationToken= cancellationToken;
            _currentFilePath = filePath;
            _consumerTask = Task.Run(ProcessLogQueueAsync);// Task.Factory.StartNew(ProcessLogQueueAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }
        public void Log(string message) => _logChannel.Writer.TryWrite($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {message}");

        //every 3 hours, rotate logs
        public void UpdateFilePath(string newFilePath) => Volatile.Write(ref _currentFilePath, newFilePath);

        private async Task ProcessLogQueueAsync()
        {
            FileStream? stream = null;
            StreamWriter? writer = null;
            try
            {
                while (await _logChannel.Reader.WaitToReadAsync(_cancellationToken))
                {
                    string? targetPath = Volatile.Read(ref _currentFilePath);
                    if (targetPath != _activeWorkerFilePath && targetPath != null)
                    {
                        if (writer != null)
                        {
                            await writer.FlushAsync();
                            await writer.DisposeAsync();
                        }
                        if (stream != null)
                        {
                            await stream.DisposeAsync();
                        }
                        _activeWorkerFilePath = targetPath;
                        stream = new FileStream(_activeWorkerFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
                        writer = new StreamWriter(stream, Encoding.UTF8);

                        if (stream.Length == 0)
                        {
                            await writer.WriteLineAsync("Timestamp,Message/Metrics".AsMemory(), _cancellationToken);
                        }

                    }
                    while (_logChannel.Reader.TryRead(out var logMessage))
                    {
                        if (writer != null)
                        {
                            await writer.WriteLineAsync(logMessage.AsMemory(), _cancellationToken);
                        }
                    }
                    if (writer != null)
                        await writer.FlushAsync(_cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                if (writer != null)
                {
                    while (_logChannel.Reader.TryRead(out var emergencyLog))
                        await writer.WriteLineAsync(emergencyLog.AsMemory());
                    await writer.FlushAsync();
                }

            }
            finally
            {
                if (writer != null)
                    await writer.DisposeAsync();
                if (stream != null)
                    await stream.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _logChannel.Writer.Complete();
            if(_consumerTask != null)
            {
                try
                {
                    await _consumerTask;

                }
                catch { }
            }
            GC.SuppressFinalize(this);
        }
    }
}
