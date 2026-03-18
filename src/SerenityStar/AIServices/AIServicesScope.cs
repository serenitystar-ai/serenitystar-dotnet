using System.Net.Http;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Provides access to AI services such as transcription, speech, and image generation.
    /// </summary>
    public sealed class AIServicesScope
    {
        /// <summary>
        /// Transcribe audio and video files into text.
        /// </summary>
        public TranscriptionScope Transcription { get; }

        internal AIServicesScope(HttpClient httpClient)
        {
            Transcription = new TranscriptionScope(httpClient);
        }
    }
}
