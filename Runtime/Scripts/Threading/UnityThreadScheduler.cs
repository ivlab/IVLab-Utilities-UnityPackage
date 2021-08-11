using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IVLab.Utilities
{

    /// <summary>
    /// Unity doesn't play nicely with threads, so the UnityThreadScheduler
    /// allows a programmer to better deal with asynchronous and threaded code.
    /// UnityThreadScheduler needs to either be attached to a GameObject, or
    /// `UnityThreadScheduler.GetInstance()` needs to be called somewhere in the
    /// code in order to use it.
    /// </summary>
    /// <remarks>
    /// Be extremely careful about exception handling with threads in Unity!
    /// They do not handle exceptions automatically and things will fail silently!
    /// </remarks>
    /// <example><code>
    /// UnityThreadScheduler.GetInstance();
    /// Task.Run(async () =>
    /// {
    ///     // Edit the transform in the main thread, since many MonoBehaviours
    ///     // need to be run from the main thread.
    ///     await UnityThreadScheduler.RunMainThreadWork(() =>
    ///     {
    ///         transform.scale = Vector3.one * 2.0f
    ///     });
    /// })
    /// </code></example>
    public class UnityThreadScheduler : Singleton<UnityThreadScheduler>
    {

        public enum UnityMethod
        {
            FixedUpdate,
            Update,
            LateUpdate
        }
        class UnityThreadJob
        {
            public bool done = false;
            public System.Diagnostics.Stopwatch stopwatch;
            public virtual void Execute() { }
            public System.Exception exception = null;
        }

        // Variation of Job for returning a value
        class UnityThreadJobFunc<T> : UnityThreadJob
        {
            public System.Func<T> work;
            public T result;

            public override void Execute()
            {
                result = work.Invoke();
            }

        }

        // Variation of Job that returns no value
        class UnityhreadJobAction : UnityThreadJob
        {
            public System.Action work;

            public override void Execute()
            {
                work.Invoke();
            }

        }


        // The queue that is flushed each update loop. 

        Dictionary<UnityMethod, Queue<UnityThreadJob>> queuedJobsPerMethod = new System.Collections.Generic.Dictionary<UnityMethod, Queue<UnityThreadJob>>();

        void UnityThread_ProcessJob(UnityThreadJob job)
        {
            try
            {
                job.Execute();
            }
            catch (System.Exception e)
            {
                job.exception = e;
            }
            job.done = true;
        }

        void UnityThread_ProcessAllQueuedJobs(UnityMethod method)
        {
            while (queuedJobsPerMethod[method].Count > 0)
            {

                UnityThreadJob job;
                lock (queuedJobsPerMethod[method])
                {
                    job = queuedJobsPerMethod[method].Dequeue();
                    // Debug.Log(job.GetHashCode() + " Dequed @ " + job.stopwatch.ElapsedMilliseconds + " ms");

                }
                UnityThread_ProcessJob(job);
            }
        }


        // Helper for queueing and waiting on jobs
        async Task RunJob(UnityThreadJob job, UnityMethod method)
        {
            job.stopwatch = System.Diagnostics.Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            lock (queuedJobsPerMethod[method])
            {
                queuedJobsPerMethod[method].Enqueue(job);
            }
            // Debug.Log(job.GetHashCode() +  " Queued @ " + job.stopwatch.ElapsedMilliseconds + " ms");
            while (job.done == false) await Task.Delay(5);
            // Debug.Log(job.GetHashCode() + " Done @ " + job.stopwatch.ElapsedMilliseconds + " ms");

            if (job.exception != null)
                throw job.exception;
        }

        void KickoffJob(UnityThreadJob job, UnityMethod method)
        {
            job.stopwatch = System.Diagnostics.Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            lock (queuedJobsPerMethod[method])
            {
                queuedJobsPerMethod[method].Enqueue(job);
            }
            // Debug.Log(job.GetHashCode() +  " Queued @ " + job.stopwatch.ElapsedMilliseconds + " ms");
            // Debug.Log(job.GetHashCode() + " Done @ " + job.stopwatch.ElapsedMilliseconds + " ms");

            if (job.exception != null)
                throw job.exception;
        }

        public async Task<T> RunMainThreadWork<T>(UnityMethod method, System.Func<T> work)
        {
            var funcJob = new UnityThreadJobFunc<T>();
            funcJob.work = work;

            // First, await the beginning/invocation of the work
            await RunJob(funcJob, method);

            // HACK: If the job returned a Task, assume we want to wait for that task before we move on...
            Debug.Log(funcJob.result.GetType());
            if (funcJob.result.GetType().BaseType == typeof(Task))
            {
                Task taskResult = funcJob.result as Task;
                await taskResult;
            }
            return funcJob.result;

        }
        public async Task<T> RunMainThreadWork<T>(System.Func<T> work)
        {
            return await RunMainThreadWork(UnityMethod.LateUpdate, work);
        }



        public async Task RunMainThreadWork(UnityMethod method, System.Action work)
        {
            var actionJob = new UnityhreadJobAction();
            actionJob.work = work;
            await RunJob(actionJob, method);
            return;
        }

        public async Task RunMainThreadWork(System.Action work)
        {
            await RunMainThreadWork(UnityMethod.LateUpdate, work);
        }

        public void KickoffMainThreadWork(UnityMethod method, System.Action work)
        {
            var actionJob = new UnityhreadJobAction();
            actionJob.work = work;
            KickoffJob(actionJob, method);
            return;
        }

        public void KickoffMainThreadWork(System.Action work)
        {
            KickoffMainThreadWork(UnityMethod.LateUpdate, work);
        }


        // Start is called before the first frame update
        void OnEnable()
        {
            queuedJobsPerMethod[UnityMethod.FixedUpdate] = new Queue<UnityThreadJob>();
            queuedJobsPerMethod[UnityMethod.Update] = new Queue<UnityThreadJob>();
            queuedJobsPerMethod[UnityMethod.LateUpdate] = new Queue<UnityThreadJob>();
        }

        void LateUpdate()
        {
            UnityThread_ProcessAllQueuedJobs(UnityMethod.LateUpdate);
        }

        void Update()
        {
            UnityThread_ProcessAllQueuedJobs(UnityMethod.Update);
        }

        void FixedUpdate()
        {
            UnityThread_ProcessAllQueuedJobs(UnityMethod.FixedUpdate);
        }
    }

}
