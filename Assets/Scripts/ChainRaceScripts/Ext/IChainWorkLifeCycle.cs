public interface IChainWorkLifeCycle {
    /// <summary>
    /// Called before starting the work
    /// Note that the work is not started yet, so you can prepare for the work here
    /// </summary>
    public void BeforeStart();
    /// <summary>
    /// Called after skipping the work
    /// Note that the work is already skipped
    /// </summary>
    public void AfterSkip();
    //NOTE: ユーザー定義のclass Result型の返却もありかもしれない
    //UPDATE: Workの中でコンストラクタにnullが渡ったらそもそもUpdateを呼ばないので継続判断のbool返却でOK
    /// <summary>
    /// Called when ChainWork Updates
    /// If no IChainWorkLifeCycle is provided to ChainWork,
    /// Update will not be called and just waits for End() to be called to complete the chain
    /// retruns TRUE when update ends, otherwise FALSE to continue updating
    /// </summary>
    /// <returns>should END updating?</returns>
    public bool Update();
}