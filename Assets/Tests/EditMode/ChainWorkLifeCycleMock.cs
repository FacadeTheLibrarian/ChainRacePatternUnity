namespace ChainPattern {
    public class ChainWorkLifeCycleMock : IChainWorkLifeCycle {
        public bool startCalled;
        public bool skipCalled;
        public bool updateCalled;

        private System.Action start = delegate { };
        private System.Action skip = delegate { };
        private System.Func<bool> update = delegate { return false; };

        public ChainWorkLifeCycleMock(
            System.Action onStart = null,
            System.Action onSkip = null,
            System.Func<bool> onUpdate = null
        ) {
            start = onStart;
            skip = onSkip;
            update = onUpdate;
        }

        public void BeforeStart() {
            startCalled = true;
            start?.Invoke();
        }
        public void AfterSkip() {
            skipCalled = true;
            skip?.Invoke();
        }
        public bool Update() {
            updateCalled = true;
            if (update == null) {
                return false;
            }
            return update.Invoke();
        }
    }
}