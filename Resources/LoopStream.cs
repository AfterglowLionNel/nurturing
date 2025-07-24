using System;
using NAudio.Wave;

namespace nurturing
{
    /// <summary>
    /// NAudio の WaveStream をループ再生させるラッパー
    /// </summary>
    public class LoopStream : WaveStream
    {
        // 再生が終わると自動で先頭に戻るクラス
        private readonly WaveStream _source;

        public LoopStream(WaveStream source)
        {
            // 再生対象の WaveStream を受け取る
            _source = source ?? throw new ArgumentNullException(nameof(source));
            EnableLooping = true;
        }

        public bool EnableLooping { get; set; }

        public override WaveFormat WaveFormat => _source.WaveFormat;

        public override long Length => _source.Length;

        public override long Position
        {
            get => _source.Position;
            set => _source.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // 読み切ったら自動で先頭に戻りループ再生
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = _source.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_source.Position == 0 || !EnableLooping)
                        break;

                    _source.Position = 0; // ループ
                    continue;
                }
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _source.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
