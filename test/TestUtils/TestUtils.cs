#nullable enable
using System.Threading.Tasks;

namespace Playdux.test.TestUtils
{
    public static class TestUtils
    {
        /// Yields and blocks until control is regained.
        public static void BlockingWait() => Task.Run(Task.Yield).Wait();

        /// Blocks for a given number of milliseconds.
        public static void BlockingWait(int ms) => Task.Run(() => Task.Delay(ms)).Wait();
    }
}