namespace AsyncEvent.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using AlexBins.Utils.AsyncEvent;
    using Moq;

    public class AsyncEventTest
    {
        private AsyncEvent<object> _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new AsyncEvent<object>();
        }

        [Test]
        public async Task EventRaised_CallsAllHandlers()
        {
            var handlers = Enumerable.Range(0, 10)
                .Select(i => new Mock<AsyncEvent<object>.Handler>())
                .ToArray();

            foreach (var handler in handlers)
            {
                _sut += handler.Object;
            }

            var sender = new object();
            var arg = new object();

            await _sut.InvokeAsync(sender, arg);

            Assert.Multiple(() =>
            {
                foreach (var handler in handlers)
                {
                    handler.Verify(x => x(sender, arg));
                }
            });
        }

        [Test]
        public async Task HandlerRemoved_NotCalled()
        {
            var handlerMock = new Mock<AsyncEvent<object>.Handler>();
            _sut += handlerMock.Object;
            _sut -= handlerMock.Object;

            await _sut.InvokeAsync(default, default);

            handlerMock.Verify(x => x(It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void HandlerDelayed_EventCompletionDelayed()
        {
            var tcs = new TaskCompletionSource<bool>();
            Task Handler(object sender, object arg)
            {
                return tcs.Task;
            }

            _sut += Handler;

            var eventTask = _sut.InvokeAsync(default, default);

            Assert.IsFalse(eventTask.AsTask().Wait(250));
            tcs.SetResult(true);
            Assert.IsTrue(eventTask.AsTask().Wait(250));
        }

        [Test]
        public void EventDefaultState_DoesNotCrash()
        {
            Assert.DoesNotThrowAsync(async () => await _sut.InvokeAsync(default, default));
        }

        [Test]
        public void HandlerNotRegistered_RemoveDoesNotBreak()
        {
            Task Handler(object sender, object arg) {return Task.CompletedTask;}
            Assert.DoesNotThrow(() => _sut -= Handler);
        }
    }
}