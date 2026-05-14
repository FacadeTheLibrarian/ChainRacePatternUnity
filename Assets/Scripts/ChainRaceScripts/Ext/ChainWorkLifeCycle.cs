namespace Sample {
    public class ChainWorkLifeCycle : IChainWorkLifeCycle {
        private System.Action start = delegate { };
        private System.Action skip = delegate { };
        private System.Func<bool> update = delegate { return false; };

        public ChainWorkLifeCycle(
            System.Action onStart = null,
            System.Action onSkip = null,
            System.Func<bool> onUpdate = null
        ) {
            start = onStart;
            skip = onSkip;
            update = onUpdate;
        }

        public void BeforeStart() {
            start?.Invoke();
        }
        public void AfterSkip() {
            skip?.Invoke();
        }
        public bool Update() {
            if (update == null) {
                return false;
            }
            return update.Invoke();
        }
    }
}