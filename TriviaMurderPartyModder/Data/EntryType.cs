namespace TriviaMurderPartyModder.Data {
    public enum EntryType {
        TDQuestion,
        TDFinalRound,
        TDWorstDrawing
    }

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
        JokeAudio
    }
}