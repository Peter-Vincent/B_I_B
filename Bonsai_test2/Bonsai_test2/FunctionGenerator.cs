using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.WebDsp
{
    [Description("Generates signal waveforms following any of a set of common periodic functions.")]
    public class WebFunctionGenerator : Source<float[]>
    {
        const double TwoPI = 2 * Math.PI;

        public WebFunctionGenerator()
        {
            SampleRate = 44100;
            BufferLength = 441;
            Frequency = 100;
            Amplitude = 1;
        }

        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The frequency of the signal waveform, in Hz.")]
        public double Frequency { get; set; }

        [Description("The periodic waveform used to sample the signal.")]
        public FunctionWaveform Waveform { get; set; }

        [Description("The sampling rate of the generated signal waveform, in Hz.")]
        public int SampleRate { get; set; }

        [Browsable(false)]
        public int? PlaybackRate
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    SampleRate = BufferLength * value.Value;
                    Frequency *= value.Value;
                }
            }
        }

        [Browsable(false)]
        public bool PlaybackRateSpecified
        {
            get { return PlaybackRate.HasValue; }
        }

        [Description("The amplitude of the signal waveform.")]
        public double Amplitude { get; set; }

        [Description("The optional DC-offset of the signal waveform.")]
        public double Offset { get; set; }

        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The optional phase offset, in radians, of the signal waveform.")]
        public double Phase { get; set; }

        static double NormalizedPhase(double phase)
        {
            return phase + Math.Ceiling(-phase / TwoPI) * TwoPI;
        }

        static void FrequencyPhaseShift(
            long sampleOffset,
            double timeStep,
            double newFrequency,
            ref double frequency,
            ref double phase)
        {
            newFrequency = Math.Max(0, newFrequency);
            if (frequency != newFrequency)
            {
                phase = NormalizedPhase(sampleOffset * timeStep * TwoPI * (frequency - newFrequency) + phase);
                frequency = newFrequency;
            }
        }

        float[] CreateBuffer(int bufferLength, long sampleOffset, double frequency, double phase)
        {
            var buffer = new float[bufferLength];
            if (frequency > 0)
            {
                var period = 1.0 / frequency;
                var waveform = Waveform;
                switch (waveform)
                {
                    default:
                    case FunctionWaveform.Sine:
                        frequency = frequency * TwoPI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = (float)Math.Sin(frequency * (i + sampleOffset) + phase);
                        }
                        break;
                    case FunctionWaveform.Triangular:
                        phase = NormalizedPhase(phase) / TwoPI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var t = frequency * (i + sampleOffset + period / 4) + phase;
                            buffer[i] = (float)(1 - (4 * Math.Abs((t % 1) - 0.5) - 1)) - 1;
                        }
                        break;
                    case FunctionWaveform.Square:
                    case FunctionWaveform.Sawtooth:
                        phase = NormalizedPhase(phase) / TwoPI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var t = frequency * (i + sampleOffset + period / 2) + phase;
                            buffer[i] = (float)(2 * (t % 1) - 1);
                            if (waveform == FunctionWaveform.Square)
                            {
                                buffer[i] = Math.Sign(buffer[i]);
                            }
                        }
                        break;
                }
            }

            var amplitude = Amplitude;
            var offset = Offset;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (float)(buffer[i] * amplitude + offset);
            }

            return buffer;
        }

        public override IObservable<float[]> Generate()
        {
            return Observable.Create<float[]>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var bufferLength = BufferLength;
                    if (bufferLength <= 0)
                    {
                        throw new InvalidOperationException("Buffer length must be a positive integer.");
                    }

                    var sampleRate = SampleRate;
                    if (sampleRate <= 0)
                    {
                        throw new InvalidOperationException("Sample rate must be a positive integer.");
                    }

                    var i = 0L;
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var frequency = 0.0;
                        var phaseShift = 0.0;
                        var timeStep = 1.0 / sampleRate;
                        var playbackRate = sampleRate / (double)bufferLength;
                        var playbackInterval = 1000.0 / playbackRate;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var sampleOffset = i++ * bufferLength;
                            FrequencyPhaseShift(sampleOffset, timeStep, Frequency, ref frequency, ref phaseShift);
                            var buffer = CreateBuffer(bufferLength, sampleOffset, frequency * timeStep, Phase + phaseShift);
                            observer.OnNext(buffer);

                            var sampleInterval = (int)(playbackInterval * i - stopwatch.ElapsedMilliseconds);
                            if (sampleInterval > 0)
                            {
                                sampleSignal.WaitOne(sampleInterval);
                            }
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        public IObservable<float[]> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var bufferLength = BufferLength;
                if (bufferLength <= 0)
                {
                    throw new InvalidOperationException("Buffer length must be a positive integer.");
                }

                var sampleRate = SampleRate;
                if (sampleRate <= 0)
                {
                    throw new InvalidOperationException("Sample rate must be a positive integer.");
                }

                var i = 0L;
                var frequency = 0.0;
                var phaseShift = 0.0;
                var timeStep = 1.0 / sampleRate;
                return source.Select(x =>
                {
                    var sampleOffset = i++ * bufferLength;
                    FrequencyPhaseShift(sampleOffset, timeStep, Frequency, ref frequency, ref phaseShift);
                    return CreateBuffer(bufferLength, sampleOffset, frequency * timeStep, Phase + phaseShift);
                });
            });
        }
    }

    public enum FunctionWaveform
    {
        Sine,
        Square,
        Triangular,
        Sawtooth
    }
}
