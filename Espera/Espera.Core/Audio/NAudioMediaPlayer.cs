﻿using NAudio.Wave;
using ReactiveMarrow;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Espera.Core.Audio
{
    public class NAudioMediaPlayer : IMediaPlayerCallback, IDisposable
    {
        private readonly WaveOutEvent outputDevice;
        private AudioFileReader currentReader;

        public NAudioMediaPlayer()
        {
            this.outputDevice = new WaveOutEvent();
        }

        public TimeSpan CurrentTime
        {
            get
            {
                return this.currentReader == null ? TimeSpan.Zero : this.currentReader.CurrentTime;
            }

            set
            {
                this.currentReader.CurrentTime = value;
            }
        }

        public IObservable<Unit> Finished
        {
            get
            {
                return Observable.FromEventPattern<StoppedEventArgs>(
                        h => this.outputDevice.PlaybackStopped += h,
                        h => this.outputDevice.PlaybackStopped -= h)
                    .ToUnit();
            }
        }

        public void Dispose()
        {
            try
            {
                this.outputDevice.Dispose();
            }

            // NAudio does strange things in the Dispose method and can throw a NullReferenceException
            catch (NullReferenceException)
            { }

            if (this.currentReader != null)
            {
                this.currentReader.Dispose();
            }
        }

        public Task LoadAsync(Uri uri)
        {
            return Task.Run(() =>
            {
                var newReader = new AudioFileReader(uri.OriginalString);

                AudioFileReader oldReader = Interlocked.Exchange(ref this.currentReader, newReader);

                if (oldReader != null)
                {
                    oldReader.Dispose();
                }

                this.outputDevice.Init(this.currentReader);
            });
        }

        public Task PauseAsync()
        {
            return Task.Run(() => this.outputDevice.Pause());
        }

        public Task PlayAsync()
        {
            return Task.Run(() => this.outputDevice.Play());
        }

        public void SetVolume(float volume)
        {
            this.outputDevice.Volume = volume;
        }

        public Task StopAsync()
        {
            return Task.Run(() => this.outputDevice.Stop());
        }
    }
}