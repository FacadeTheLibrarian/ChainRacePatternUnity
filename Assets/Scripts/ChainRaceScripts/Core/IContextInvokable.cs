namespace ChainPattern {
    public interface IContextInvokable {
        /// <summary>
        /// Invokes the complete callback if no action has been performed yet, and locks the state by a guard clause.
        /// </summary>
        /// <param name="chain">The chain that has completed.</param>
        public void Complete(BaseChain chain);

        /// <summary>
        /// Invokes the skip callback if no action has been performed yet, and locks the state by a guard clause.
        /// </summary>
        /// <param name="chain">The chain that has been skipped.</param>
        public void Skip(BaseChain chain);
    }
}