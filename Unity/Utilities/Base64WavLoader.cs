using System;
using System.IO;
using UnityEngine;

public static class Base64WavLoader
{
    public static AudioClip FromBase64(string base64, string clipName = "base64_wav")
    {
        byte[] wavData = Convert.FromBase64String(base64);
        return FromWavData(wavData, clipName);
    }

    public static AudioClip FromWavData(byte[] wavData, string clipName = "wav_clip")
    {
        using (MemoryStream ms = new MemoryStream(wavData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // WAV RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF")
                throw new Exception("Invalid WAV file: missing RIFF header");

            int fileSize = reader.ReadInt32();
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE")
                throw new Exception("Invalid WAV file: missing WAVE header");

            // Find "fmt " chunk
            string fmt = new string(reader.ReadChars(4));
            while (fmt != "fmt ")
            {
                int chunkSize = reader.ReadInt32();
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                fmt = new string(reader.ReadChars(4));
            }

            int fmtChunkSize = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();  // 1 = PCM
            int numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            int bitsPerSample = reader.ReadInt16();

            if (fmtChunkSize > 16)
                reader.ReadBytes(fmtChunkSize - 16);

            // Find "data" chunk
            string dataID = new string(reader.ReadChars(4));
            while (dataID != "data")
            {
                int chunkSize = reader.ReadInt32();
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                dataID = new string(reader.ReadChars(4));
            }

            int dataSize = reader.ReadInt32();

            //some WAVs have -1 (0xFFFFFFFF) or bogus values for dataSize
            if (dataSize < 0 || dataSize > (wavData.Length - reader.BaseStream.Position))
            {
                dataSize = (int)(wavData.Length - reader.BaseStream.Position);
            }

            byte[] data = reader.ReadBytes(dataSize);

            // Convert PCM to float samples
            float[] samples;

            if (bitsPerSample == 16)
            {
                int sampleCount = data.Length / 2;
                samples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = BitConverter.ToInt16(data, i * 2);
                    samples[i] = sample / 32768f;
                }
            }
            else if (bitsPerSample == 8)
            {
                int sampleCount = data.Length;
                samples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    samples[i] = (data[i] - 128) / 128f;
                }
            }
            else
            {
                throw new Exception($"Unsupported WAV bit depth: {bitsPerSample}");
            }

            // Create AudioClip
            AudioClip audioClip = AudioClip.Create(clipName, samples.Length / numChannels, numChannels, sampleRate, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }
    }
}

