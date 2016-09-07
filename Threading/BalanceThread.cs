using com.locomain.thread.Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace com.locomain.thread
{
    public class BalanceThread
    {
        private Thread _thread;
        private ThreadResult _threadResult;
        private System.Timers.Timer _timer;

        private volatile List<Action> _actions;

        private int _threadLifeTime = YingYang.THREAD_SLEEP_TIME;//TODO
        private volatile bool _running = false; //TODO
        private volatile bool _shouldRun;
        private volatile bool _shouldRunInBackground = true;


        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="tr"></param>
        public BalanceThread(ThreadResult tr)
        {
            init(tr);
        }
        public BalanceThread(ThreadResult tr, Action initAction)
        {
            init(tr);
            addAction(initAction);
        }
        public BalanceThread(ThreadResult tr, Action initAction,bool autorun)
        {
            init(tr);       
            if(autorun)run();
        }

        /// <summary>
        /// Just in case finalizer
        /// </summary>
        ~BalanceThread()
        {
            try
            {
                stop();
            } catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private Thread getInstance()
        {
            return _thread == null ? new Thread(threadEngine) : _thread;
        }

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="tr"></param>
        private void init(ThreadResult tr)
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timerDone;
            _timer.Interval = _threadLifeTime==0?1:_threadLifeTime;

            _threadResult = tr;
            _thread = getInstance();
            _thread.IsBackground = _shouldRunInBackground;
            _actions = new List<Action>();
        }


        /// <summary>
        /// Main thread logic
        /// </summary>
        private void threadEngine()
        {
            Utils.log("engine started");   
            while (_shouldRun) //TODO  
            {
               // Utils.log("engine running");

                if (_actions.Count == 0)
                {
                    _shouldRun =_threadResult.onEndResult(this);//TODO recall to yingyan
                    if (_threadLifeTime <= 0)
                    {
                        stop();
                    } else
                    {
                        _timer.Enabled = _shouldRun;
                    }     
                    continue; 
                }
                if (_timer.Enabled) _timer.Enabled=false;
                _actions[_actions.Count - 1]?.Invoke();
                _actions.RemoveAt(_actions.Count - 1);
                Utils.log("action invoked");

                //TODO callback/lazy
            }
            Utils.log("engine stopped");
        }

        [Obsolete]
        public void setThread(Thread t)
        {
            _thread = t;
        }

        /// <summary>
        /// Timer initializer
        /// </summary>
        private void initTimer()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timerDone;
            _timer.Interval = _threadLifeTime == 0 ? 1 : _threadLifeTime;
        }

        /// <summary>
        /// destroy thread reference
        /// </summary>
        private void destroy()
        {
            if (_thread == null) return;
            _thread = null;
            Utils.log("deconstructing thread");
        }


        /// <summary>
        /// Stops the thread
        /// </summary>
        public void stop()
        {
            _timer.Enabled = false;           
            _shouldRun = false;
            _actions.Clear();
            _threadResult.release(this);

            destroy();  
        }

        /// <summary>
        /// Starts the the thread and execution of tasks only if the thread is not already running.
        /// </summary>
        public BalanceThread run()
        {
            if (_running) return this;

            _running = !_running;
            _shouldRun = true;
            _thread.Start();

            return this;
        }

        /// <summary>
        /// Adds action/task to queue/pool
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public BalanceThread addAction(Action a)
        {
            _actions.Add(a);
            return this;
        }

        /// <summary>
        /// Lets the thread run with the called thread or in the background
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BalanceThread runInBackground(bool value)
        {             
            _shouldRunInBackground = value;
            if (_thread == null) return this;
            _thread.IsBackground = value;
            return this;
        }

        /// <summary>
        /// Sets Lifetime after all actions are done
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BalanceThread setSleepTime(int value)
        {
            _threadLifeTime = value;
            initTimer();//re-init timer
            return this;
        }

        /// <summary>
        /// When _timer runs out this event stops thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerDone(object sender, ElapsedEventArgs e)
        {
            Utils.log("lifetime is over");
            stop();
        }

        /// <summary>
        /// Returns true if thread is busy
        /// </summary>
        /// <returns></returns>
        public bool isBusy()
        {
            return _actions.Count > 0;
        }
        /// <summary>
        /// Returns amount of actions in the execution queue
        /// </summary>
        /// <returns></returns>
        public int getActionQueue()
        {
            return _actions.Count;
        }
    }
}
