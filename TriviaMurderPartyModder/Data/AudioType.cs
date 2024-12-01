namespace TriviaMurderPartyModder.Data {
    /// <summary>
    /// Possible dialog options in the game where an audio file can be added.
    /// </summary>
    public enum AudioType {
        /// <summary>
        /// Used for questions and final round.
        /// </summary>
        Q,
        /// <summary>
        /// Used for questions.
        /// </summary>
        Intro,
        /// <summary>
        /// Used for worst drawing.
        /// </summary>
        JokeAudio,
        /// <summary>
        /// Used for worst response.
        /// </summary>
        QuestionAudio
    }
}