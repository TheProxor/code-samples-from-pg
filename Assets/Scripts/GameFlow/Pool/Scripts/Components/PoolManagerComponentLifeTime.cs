namespace Drawmasters.Pool
{
    public class PoolManagerComponentLifeTime: PoolManagerComponentTemplate, IUpdatable
    {
        #region Fields

        private float initialTime = 0;
        private float watchedTime = 0;

        #endregion



        #region Ctor

        public PoolManagerComponentLifeTime(float seconds)
        {
            initialTime = seconds;
        }

        ~PoolManagerComponentLifeTime()
        {
            foreach (var item in poolManager.componentPool)
            {
                item.OnSomething -= Refresh;
            }
        }

        #endregion



        #region Override methods

        public override void Initialize(PoolManager _poolManager)
        {
            base.Initialize(_poolManager);

            foreach (var item in poolManager.componentPool)
            {
                item.OnSomething += Refresh;
            }
        }

        #endregion



        #region Methods

        internal void Refresh()
        {
            watchedTime = 0f;
        }

        #endregion



        #region IUpdatable

        public void CustomUpdate(float deltaTime)
        {

            if (watchedTime < initialTime)
            {
                watchedTime += deltaTime;
            }
            else
            {
                Refresh();

                foreach (var item in poolManager.componentPool)
                {
                    item.RemoveFirst();
                }
            }
        }

        #endregion
    }
}
