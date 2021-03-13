using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Lavalink;

namespace Suruga.Extensions
{
    // Source: https://github.com/Yucked/Victoria/blob/v5/src/DefaultQueue.cs
    public class LavalinkExtensions<T> : IEnumerable<T> where T : LavalinkTrack
    {
        private readonly LinkedList<T> list = new();

        /// <summary>
        /// Equeues a <see cref="LavalinkTrack"/>.
        /// </summary>
        /// <param name="lavalinkTrack">A track.</param>
        public void Enqueue(T lavalinkTrack)
        {
            if (lavalinkTrack == null)
            {
                return;
            }

            lock (list)
            {
                list.AddLast(lavalinkTrack);
            }
        }

        /// <summary>
        /// Dequeues a <see cref="LavalinkTrack"/>.
        /// </summary>
        /// <param name="lavalinkTrack">A track.</param>
        /// <returns>[<see cref="bool"/>] A <see cref="true"/> value indicating that the dequeuing of a track was successful, otherwise retuns a <see cref="false"/> value.</returns>
        public bool Dequeue(out T lavalinkTrack)
        {
            lock (list)
            {
                if (list.Count < 1)
                {
                    lavalinkTrack = default;
                    return false;
                }

                T result = list.First.Value;
                if (list.First == null)
                {
                    lavalinkTrack = default;
                    return false;
                }

                list.RemoveFirst();
                lavalinkTrack = result;

                return true;
            }
        }

        /// <summary>
        /// Shuffles/Orders each position of a track randomly in a queue, if it exists.
        /// </summary>
        public void Shuffle()
        {
            lock (list)
            {
                if (list.Count < 2)
                {
                    return;
                }

                T[] shadow = new T[list.Count];
                int defaultNumber = 0;

                for (LinkedListNode<T> node = list.First; !(node is null); node = node.Next)
                {
                    Random random = new();

                    int nextRandomNumber = random.Next(defaultNumber + 1);
                    if (defaultNumber != nextRandomNumber)
                    {
                        shadow[defaultNumber] = shadow[nextRandomNumber];
                    }

                    shadow[nextRandomNumber] = node.Value;
                    defaultNumber++;
                }

                list.Clear();

                shadow.ToList().ForEach(x => list.AddLast(x));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (list)
            {
                for (LinkedListNode<T> node = list.First; node != null; node = node.Next)
                {
                    yield return node.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}