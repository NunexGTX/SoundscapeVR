using System; 
using System.IO; 
using UnityEngine;

public class LoadAmbisonicWav : MonoBehaviour 
{    
    public string filePath; // full path to user folder file, set at runtime    
    public UnityEngine.AudioSource audioSource;

    void OnEnable()
    {
        if (string.IsNullOrEmpty(filePath) || audioSource == null)
        {
            Debug.LogError("File path or AudioSource not set.");
            return;
        }

        float[] samples;
        int channels, sampleRate, sampleCount;
        if (!LoadWavFile(Path.Combine(Application.streamingAssetsPath, filePath), out samples, out channels, out sampleRate, out sampleCount))
        {
            Debug.LogError("Failed to load WAV.");
            return;
        }

        // Create AudioClip: name, samplesPerChannel, channels, sampleRate, stream=false
        int samplesPerChannel = sampleCount / channels;
        AudioClip clip = AudioClip.Create(Path.GetFileName(filePath), samplesPerChannel, channels, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.clip = clip;
        //audioSource.spatialBlend = 1f; // 3D
        audioSource.Play();

        // Ensure Steam Audio Ambisonics component is present on same GameObject or parent
        // and configured to treat this source as Ambisonic (Ambisonics Source component in Steam Audio)
    }

    // Very small WAV loader for PCM16/PCM32 interleaved multichannel
    bool LoadWavFile(string path, out float[] outSamples, out int outChannels, out int outSampleRate, out int outSampleCount)
    {
        outSamples = null;
        outChannels = 0;
        outSampleRate = 0;
        outSampleCount = 0;

        if (!File.Exists(path)) return false;
        byte[] bytes = File.ReadAllBytes(path);
        using (var ms = new MemoryStream(bytes))
        using (var br = new BinaryReader(ms))
        {
            // Minimal RIFF header parse
            try
            {
                string riff = new string(br.ReadChars(4)); // "RIFF"
                br.ReadInt32(); // file size
                string wave = new string(br.ReadChars(4)); // "WAVE"
                // fmt chunk
                string fmt = new string(br.ReadChars(4));
                int fmtLen = br.ReadInt32();
                int audioFormat = br.ReadInt16();
                int numChannels = br.ReadInt16();
                int sampleRate = br.ReadInt32();
                br.ReadInt32(); // byte rate
                br.ReadInt16(); // block align
                int bitsPerSample = br.ReadInt16();
                // skip any extra fmt bytes
                if (fmtLen > 16) br.ReadBytes(fmtLen - 16);

                // find data chunk
                string chunkId;
                int chunkSize;
                while (true)
                {
                    chunkId = new string(br.ReadChars(4));
                    chunkSize = br.ReadInt32();
                    if (chunkId == "data") break;
                    br.ReadBytes(chunkSize);
                }

                byte[] data = br.ReadBytes(chunkSize);
                int bytesPerSample = bitsPerSample / 8;
                int totalSamples = data.Length / bytesPerSample;
                int samplesPerChannel = totalSamples / numChannels;

                float[] samples = new float[totalSamples];

                if (bitsPerSample == 16)
                {
                    for (int i = 0; i < totalSamples; i++)
                    {
                        short v = BitConverter.ToInt16(data, i * 2);
                        samples[i] = v / 32768f;
                    }
                }
                else if (bitsPerSample == 24)
                {
                    for (int i = 0; i < totalSamples; i++)
                    {
                        int offset = i * 3;
                        // Reconstruct a signed 24-bit integer into the upper 3 bytes of an int32
                        int v = (data[offset + 2] << 24) | (data[offset + 1] << 16) | (data[offset] << 8);
                        // Arithmetic right-shift by 8 preserves the sign, giving a range of [-2^23, 2^23-1]
                        samples[i] = (v >> 8) / 8388608f; // 8388608 = 2^23
                    }
                }
                else if (bitsPerSample == 32)
                {
                    for (int i = 0; i < totalSamples; i++)
                    {
                        int v = BitConverter.ToInt32(data, i * 4);
                        samples[i] = v / (float)Int32.MaxValue;
                    }
                }
                else
                {
                    Debug.LogError("Unsupported bits per sample: " + bitsPerSample);
                    return false;
                }

                outSamples = samples;
                outChannels = numChannels;
                outSampleRate = sampleRate;
                outSampleCount = totalSamples;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("WAV parse error: " + e.Message);
                return false;
            }
        }
    }
}