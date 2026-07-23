using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests
{
    public class DuplexTestStream : System.IO.Stream
    {
        private readonly System.IO.Stream _inputStream;   // Client reads data from here
        private readonly System.IO.Stream _outputStream;  // Client writes data here

        // Change the first parameter to accept a PipeReader's Stream wrapper
        public DuplexTestStream(System.IO.Stream inputStream, System.IO.Stream outputStream)
        {
            _inputStream = inputStream;
            _outputStream = outputStream;
        }

        // Route client Read operations to our input pipe stream wrapper
        public override int Read(byte[] buffer, int offset, int count) => _inputStream.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
            _inputStream.ReadAsync(buffer, offset, count, token);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) =>
            _inputStream.ReadAsync(buffer, token);

        // Route client Write operations to our output memory stream
        public override void Write(byte[] buffer, int offset, int count) => _outputStream.Write(buffer, offset, count);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token) => _outputStream.WriteAsync(buffer, offset, count, token);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default) => _outputStream.WriteAsync(buffer, token);

        // Required overrides for Stream
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => _outputStream.Flush();
        public override Task FlushAsync(CancellationToken token) => _outputStream.FlushAsync(token);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

}
