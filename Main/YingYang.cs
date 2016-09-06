using com.locomain.thread.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.locomain.thread
{
#pragma warning restore
    public class YingYang : ThreadResult
    {
        public const int THREAD_SLEEP_TIME = 0; //10 seconds
    
        public delegate void Result(); //TODO?

        private volatile List<Action> _actions;
        private List<BalanceThread> _pool; //TODO weakreference?

        private int _threadLifeTime = THREAD_SLEEP_TIME;
        private int _preferedPoolSize = 1;
        private int _maxThreadCache = 1;
        private bool _preferedAsBackgroundThread = true;

        private static YingYang _yingYang;

        /// <summary>
        /// Multithreaded singleton YingYang get
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static YingYang getInstance()
        {
            return _yingYang==null? new YingYang() : _yingYang;
        } 

        /// <summary>
        /// Default constructor
        /// </summary>
        private YingYang()
        {
            _actions = new List<Action>();
            _pool = new List<BalanceThread>();
        }

        /// <summary>
        /// YingYang uses application context lifecycle.
        /// Stopping the app automaticly calles this method and stops all the threads
        /// </summary>
        /// 
        [Obsolete("Experimental")]
        ~YingYang()
        {
            Utils.log("YINGYANG destructor is called.");
            try
            {
                foreach(BalanceThread thread in _pool)
                {
                    thread.stop();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            } 
        }

        /// <summary>
        /// Routes actions to the threads
        /// </summary>
        /// <param name="action"></param>
        private void route(Action action)
        {
            //check if there is a active thread without running actions if so add action
            BalanceThread nonBusyThread = searchForNonBusyThread();
            if (nonBusyThread != null)
            {
                nonBusyThread.addAction(action);
                return;
            }

            //We didnt found a busy thread so we are going to make a new one
            if (getThreadPoolCount() < _preferedPoolSize) 
            {
                Utils.log("creating new BalanceThread in pool");
                _pool.Add(new BalanceThread(this, action)
                    .runInBackground(_preferedAsBackgroundThread)
                    .setSleepTime(_threadLifeTime)
                    .run());
                return;
            } else 
            {   //thread limit is crossed add to queue
                int[] actionValues = new int[_pool.Count];
                
                for (int i = 0; i <_pool.Count; i++)
                {
                    //re-check if threads are busy and create a list of actions to determine the best Thread
                    BalanceThread thread = _pool[i];
                    if (thread.isBusy())
                    {
                        actionValues[i] = thread.getActionQueue(); 
                    } else
                    {
                        thread.addAction(action);
                        Utils.log("adding new action to living non busy thread");
                    }
                }
                //check thread with lowest amount of action
                int minimalCount  = actionValues.Min();
                for(int t = 0; t < actionValues.Length; t++)
                {
                    if (minimalCount == actionValues[t])
                    {
                        _pool[t].addAction(action);
                    }
                }
            }
        }
        
        /// <summary>
        /// Returns Threads that are not busy if any
        /// </summary>
        /// <returns></returns>
        private BalanceThread searchForNonBusyThread()
        {
            if (_pool.Count == 0) return null;
            foreach(BalanceThread thread in _pool)
            {
                if (!thread.isBusy()) return thread;
            }
            return null;
        }

        /// <summary>
        /// Method to register action to the queue
        /// </summary>
        /// <param name="a"></param>
        public YingYang addAction(Action a)
        {
            _actions.Add(a);
            return this;
        }

        /// <summary>
        /// Method to start the registered action
        /// </summary>
        public void start()
        {
            foreach(Action action in _actions)
            {
                route(action);
            }
        }

        /// <summary>
        /// Method to instantly run a method
        /// </summary>
        /// <param name="a"></param>
        public YingYang run(Action a, bool autoStart = true)
        {
            route(a);
            if(autoStart)start();
            return this;
        }

        /// <summary>
        /// Sets the poolcount limit
        /// </summary>
        /// <param name="count"></param>
        public YingYang setPreferedThreadPoolCount(int count)
        {
            _preferedPoolSize = count;
            return this;
        }

        /// <summary>
        /// Will stop the threads when the calling thread stops ( defualt = true )
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public YingYang runWithMainThread(bool value = true)
        {
            _preferedAsBackgroundThread = value;
            return this;
        }

        /// <summary>
        /// Sets max thread cache aka threads alive after all actions are executed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public YingYang setMaxThreadCache(int value)
        {
            _maxThreadCache = value;
            return this;
        }

        /// <summary>
        /// Set life time in seconds after actions are done
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public YingYang setThreadSleepTime(int value)
        {
            _threadLifeTime = value * 1000;
            return this;
        }

        /// <summary>
        /// Returns the amount of threads used
        /// </summary>
        /// <returns></returns>
        public int getThreadPoolCount()
        {
            return _pool.Count;
        }

        /// <summary>
        /// Returns the active threads
        /// </summary>
        /// <returns></returns>
        public List<BalanceThread> getThreads()
        {
            return _pool;
        }
        /// <summary>
        /// Returns the actions in the queue
        /// </summary>
        /// <returns></returns>
        public int getActionsInQueue()
        {
            return _actions.Count;
        }

        /// <summary>
        /// Called by a child BalanceThread when all the actions are done.
        /// Returns if the thread should be alive or not
        /// </summary>
        /// <returns></returns>
        public bool onEndResult(BalanceThread thread)
        {
            return !(_pool.Count > _maxThreadCache);       
        }

        /// <summary>
        /// Releases thread from pool
        /// </summary>
        /// <param name="thread"></param>
        public void release(BalanceThread thread)
        {
            Utils.log("thread released from pool");
            _pool.Remove(thread);
        }
    }
}
