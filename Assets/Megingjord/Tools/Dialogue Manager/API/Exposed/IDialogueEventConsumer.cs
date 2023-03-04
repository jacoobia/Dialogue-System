namespace Megingjord.Tools.Dialogue_Manager.API.Exposed {
    public interface IDialogueEventConsumer {

        /// <summary>
        /// Triggered whenever the DialogueManager hits an event node
        /// and broadcasts the dialogue event
        /// Every dialogue event will hit this method, so the
        /// eventTag is for the user to filter to the events required
        /// </summary>
        /// <param name="eventTag"></param>
        void ConsumeDialogueEvent(string eventTag);

    }
}