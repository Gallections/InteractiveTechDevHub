### Base64 Wav Loader
I happen to come across the issue of converting WAV audio files from bytes to audioclips in unity. After trials and errors and some aid with AI, I managed to create this script that solves the issue. 

To use this script, simply paste it in your unity project and call the `FromWavData(bytes, name)` function from the static class. 

Usage Example:
<code>Base64WavLoader.FromWavData(audioBytes, "myClip")</code>
