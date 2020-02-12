using System.Collections;

namespace Assets.Scripts._3rdparty
{
    public class ThreadedJob
    {
        private bool mIsDone = false;
        private object mHandle = new object();
        private System.Threading.Thread mThread = null;
        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (mHandle)
                {
                    tmp = mIsDone;
                }
                return tmp;
            }
            set
            {
                lock (mHandle)
                {
                    mIsDone = value;
                }
            }
        }

        public virtual void Start()
        {
            mThread = new System.Threading.Thread(Run);
            mThread.Start();
        }
        public virtual void Abort()
        {
            mThread.Abort();
        }

        protected virtual void ThreadFunction() { }

        protected virtual void OnFinished() { }

        public virtual bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }
        public IEnumerator WaitFor()
        {
            while (!Update())
            {
                yield return null;
            }
        }
        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }
    }
}