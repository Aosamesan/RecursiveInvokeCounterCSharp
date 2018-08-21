using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecursiveCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== SORT TEST ====");
            IList<int> randomList = GetRandomIntegerList(16).ToList();
            IList<int> list1, list2;
            MergeSortTest(list1 = new List<int>(randomList));
            Console.WriteLine($"original list {(IsEqualList(randomList, list1) ? "==" : "!=")} merge sort's result");
            QuickSortTest(list2 = new List<int>(randomList));
            Console.WriteLine($"original list {(IsEqualList(randomList, list2) ? "==" : "!=")} quick sort's result");
            Console.WriteLine($"merge sort's result {(IsEqualList(list1, list2) ? "==" : "!=")} quick sort's result");
        }

        private static bool IsEqualList(IList<int> list1, IList<int> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<int> GetRandomIntegerList(int size)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            while(size-- > 0)
            {
                yield return r.Next(1000);
            }
        }

        private static bool IsSorted(IList<int> list)
        {
            int previous = list.First();

            foreach(int n in list)
            {
                if (previous > n)
                {
                    return false;
                }
                previous = n;
            }

            return true;
        }

        private static void MergeSortTest(IList<int> randomList)
        {
            AbstractInvokeCounter<Action<object[]>> mergeSourtCounter = InvokeCounter.CreateInvokeCounter(MergeSort);
            mergeSourtCounter.Invoke(randomList, 0, randomList.Count);
            Console.WriteLine("<Merge Sort> IsSorted? {0}, Invoke Count : {1}", IsSorted(randomList), mergeSourtCounter.Count);
        }

        private static void QuickSortTest(IList<int> randomList)
        {
            AbstractInvokeCounter<Action<object[]>> mergeSourtCounter = InvokeCounter.CreateInvokeCounter(QuickSort);
            mergeSourtCounter.Invoke(randomList, 0, randomList.Count);
            Console.WriteLine("<Quick Sort> IsSorted? {0}, Invoke Count : {1}", IsSorted(randomList), mergeSourtCounter.Count);
        }

        private static void MergeSort(object[] param, InvokeCounter invokeCounter)
        {
            if (param.Length != 3)
            {
                throw new ArgumentException();
            }

            IList<int> list = param[0] as IList<int>;
            int start = (int)param[1];
            int end = (int)param[2];
            int length = end - start; // exclude end

            if (length < 2)
            {
                return;
            }

            int mid = (start + end) / 2;

            // divide
            invokeCounter.Invoke(list, start, mid);
            invokeCounter.Invoke(list, mid, end);

            // conquer
            int left = start;
            int right = mid;
            Queue<int> queue = new Queue<int>();

            while (left < mid && right < end)
            {
                queue.Enqueue(list[left] < list[right] ? list[left++] : list[right++]);
            }

            while (left < mid)
            {
                queue.Enqueue(list[left++]);
            }

            while (right < end)
            {
                queue.Enqueue(list[right++]);
            }

            int index = start;
            while (queue.Count > 0)
            {
                list[index++] = queue.Dequeue();
            }
        }

        public static void QuickSort(object[] param, InvokeCounter invokeCounter)
        {
            if (param.Length != 3)
            {
                throw new ArgumentException();
            }

            IList<int> list = param[0] as IList<int>;
            int start = (int)param[1];
            int end = (int)param[2] - 1; // include end

            if (start < end)
            {
                int p = Partition(list, start, end);
                invokeCounter.Invoke(list, start, p);
                invokeCounter.Invoke(list, p + 1, end + 1);
            }

            int Partition(IList<int> innerList, int x, int y)
            {
                int pivot = innerList[y];
                int left = x - 1;
                int right = x;

                for (;right <= y - 1; right++)
                {
                    if (list[right] <= pivot)
                    {
                        left++;
                        Swap(list, left, right);
                    }
                }
                Swap(list, left + 1, y);
                return left + 1;
            }

            void Swap(IList<int> innerList, int x, int y)
            {
                var temp = innerList[x];
                innerList[x] = innerList[y];
                innerList[y] = temp;
            }
        }
    }

    public abstract class AbstractInvokeCounter<InvokerType>
    {
        protected InvokerType invoker;
        public int Count { get; protected set; }
        protected AbstractInvokeCounter()
        {
            Count = 0;
        }

        protected void SetInvoker(InvokerType invoker)
        {
            this.invoker = invoker;
        }

        public void Invoke(params object[] param)
        {
            Count++;
            (invoker as Delegate).DynamicInvoke(param as object);
        }

        public R Invoke<R>(params object[] param)
        {
            Count++;
            return (R)(invoker as Delegate).DynamicInvoke(param as object);
        }

        public void ResetCount()
        {
            Count = 0;
        }
    }

    class InvokeCounter : AbstractInvokeCounter<Action<object[]>>
    {
        private InvokeCounter()
            : base()
        {
        }

        public static InvokeCounter CreateInvokeCounter(Action<object[], InvokeCounter> action)
        {
            InvokeCounter invokeCounter = new InvokeCounter();
            invokeCounter.SetInvoker((param) => action(param, invokeCounter));
            return invokeCounter;
        }
    }

    class InvokeCounter<R> : AbstractInvokeCounter<Func<object[], R>>
    {
        private InvokeCounter()
            : base()
        {
        }
       
        public static InvokeCounter<T> CreateInvokeCounter<T>(Func<object[], InvokeCounter<T>, T> func)
        {
            InvokeCounter<T> invokeCounter = new InvokeCounter<T>();
            invokeCounter.SetInvoker((param) => func(param, invokeCounter));
            return invokeCounter;
        }
    }
}
