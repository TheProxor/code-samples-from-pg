using System.Collections.Generic;


namespace Drawmasters.Announcer
{
    public class AnnouncerHandler : IDeinitializable, IInitializable
    {
        #region Fields

        private readonly List<Announcer> announcers;

        #endregion


        #region Ctor

        public AnnouncerHandler()
        {
            announcers = new List<Announcer>();
        }

        #endregion



        #region Methods

        public void AddAnnouncer(Announcer announcer)
        {
            announcers.Add(announcer);
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            announcers.ForEach(a => a.OnReady += Announcer_OnReady);
            announcers.ForEach(a => a.Hide(true));
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            announcers.ForEach(a => a.OnReady -= Announcer_OnReady);
            announcers.ForEach(a => a.Deinitialize());
        }

        #endregion



        #region Events handlers

        private void Announcer_OnReady(Announcer readyAnnouncer)
        {
            foreach (var announcer in announcers)
            {
                bool needShow = announcer == readyAnnouncer;
                if (needShow)
                {
                    announcer.Show();
                }
                else
                {
                    announcer.Hide(true);
                }
            }
        }

        #endregion
    }
}