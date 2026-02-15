using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Sparkling.SceneFinder
{
    public class LazyLoader
    {
        private readonly Queue<ILazyLoadable> m_loadQueue = new Queue<ILazyLoadable>();
        private double m_maxMillisPerFrame = 5.0;
        private int m_initialTotal = 0;
        private bool m_isSubscribed = false;

        public float Progress => m_initialTotal == 0 ? 1f : 1f - ((float)m_loadQueue.Count / m_initialTotal);
        public bool IsProcessing => m_loadQueue.Count > 0;

        public void Initialize(IEnumerable<ILazyLoadable> items, double maxMillisPerFrame = 5.0)
        {
            m_maxMillisPerFrame = maxMillisPerFrame;

            if (m_isSubscribed)
            {
                Stop();
            }

            m_loadQueue.Clear();
            foreach (var item in items)
            {
                if (item != null)
                {
                    m_loadQueue.Enqueue(item);
                }
            }

            m_initialTotal = m_loadQueue.Count;
            if (m_loadQueue.Count > 0)
            {
                Start();
            }
        }

        private void Start()
        {
            if (!m_isSubscribed)
            {
                EditorApplication.update += ProcessQueue;
                m_isSubscribed = true;
            }
        }

        private void Stop()
        {
            if (m_isSubscribed)
            {
                EditorApplication.update -= ProcessQueue;
                m_isSubscribed = false;
            }
        }

        public void Cancel()
        {
            m_loadQueue.Clear();
            m_initialTotal = 0;
            Stop();
        }

        private void ProcessQueue()
        {
            if (m_loadQueue.Count == 0)
            {
                Stop();
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            while (m_loadQueue.Count > 0)
            {
                if (sw.Elapsed.TotalMilliseconds > m_maxMillisPerFrame)
                {
                    break;
                }

                ILazyLoadable current = m_loadQueue.Dequeue();
                current?.Load();
            }

            sw.Stop();
        }
    }
}