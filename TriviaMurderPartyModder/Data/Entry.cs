namespace TriviaMurderPartyModder.Data {
    public abstract class Entry {
        public int ID {
            get => id;
            set {
                id = value;
                jet = null; // Will be loaded from a different folder
            }
        }
        int id;

        /// <summary>
        /// Audio file existence and path container.
        /// </summary>
        protected DataJet jet;
    }
}