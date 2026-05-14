public interface IChainWorkLifeCycle {
    public void BeforeStart();
    public void AfterSkip();
    //NOTE: ユーザー定義のclass Result型の返却もありかもしれない
    public bool Update();
}