namespace Repo.Event
{
    public enum EventName
    {
        /// <summary>
        /// update item in baga
        /// </summary>
        EvtUpdateItem,

        /// <summary>
        /// call before scene load
        /// </summary>
        EvtBeforeUnloadScene,

        /// <summary>
        /// call after scene load
        /// </summary>
        EvtAfterLoadScene,

        /// <summary>
        /// click item
        /// </summary>
        EvtItemClick,

        /// <summary>
        /// use item
        /// </summary>
        EvtItemUse,

        /// <summary>
        /// output dialog
        /// </summary>
        EvtDialogPop,

        /// <summary>
        /// update game state
        /// </summary>
        EvtUpdateGameState,

        /// <summary>
        /// finish mini game
        /// </summary>
        EvtFinishMiniGame,

        /// <summary>
        /// finish game
        /// </summary>
        EvtPassGameEvent,

        /// <summary>
        /// start game week
        /// </summary>
        EvtStartGameEvent,
    }
}