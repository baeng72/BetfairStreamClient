using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BetfairStreamClient.Stream
{
    public class RawStreamDumper : IAsyncDisposable
    {
        private readonly Channel<byte[]> _rawChannel;
        private Task? _writerTask;
        private CancellationToken _cancellationToken;
        private string? _currentFilePath;
        private string? _activeWorkerFilePath;

        public RawStreamDumper()
        {
            //Use an unbounded channel for raw historical data to ensure we never drop a single packet.
            //It uses byte[] arrays rented directly from your incoming stream network pipeline.
            _rawChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
            {
                SingleWriter = true,//only your socket reader thread writes to this
                SingleReader = true
            });
        }

        public void Init(string initialPath, CancellationToken cancellationToken)
        {
            _currentFilePath = initialPath;
            _cancellationToken = cancellationToken;
            _writerTask = Task.Run(ProcessRawQueueAsync);
        }

        public void UpdateFilePath(string newPath)
        {
            Volatile.Write(ref _currentFilePath, newPath);
        }

        public void EnqueueRawChunk(byte[] rawBytes, int length)
        {
            //Allocate a precie snapshot block for the file system
            //(Do not pass rented array directly or it will get corrupted by the parser thread)
            byte[] copy = new byte[length];
            Buffer.BlockCopy(rawBytes, 0, copy, 0, length);

            //Push to background file writer instantaneously
            _rawChannel.Writer.TryWrite(copy);
        }

        private async Task ProcessRawQueueAsync()
        {
            FileStream? stream = null;
            try
            {
                while (await _rawChannel.Reader.WaitToReadAsync(_cancellationToken))
                {
                    string? targetPath = Volatile.Read(ref _currentFilePath);
                    //Handle 3 hour rotation shifts safely
                    if (targetPath != _activeWorkerFilePath && targetPath != null)
                    {
                        if (stream != null) await stream.DisposeAsync();
                        _activeWorkerFilePath = targetPath;
                        stream = new FileStream(_activeWorkerFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 65536, useAsync: true);
                    }

                    while (_rawChannel.Reader.TryRead(out var chunk))
                    {
                        if (stream != null)
                        {
                            await stream.WriteAsync(chunk.AsMemory(), _cancellationToken);
                            //Append a trailing newline character to keep valid .json structural format
                            //await stream.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine), _cancellationToken);
                        }
                    }
                    if (stream != null)
                    {
                        await stream.FlushAsync(_cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                if(stream!=null) await stream.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _rawChannel.Writer.Complete();
            if(_writerTask != null)
            {
                try
                {
                    await _writerTask;

                }
                catch { }
            }
            GC.SuppressFinalize(this);
        }

    }
}
