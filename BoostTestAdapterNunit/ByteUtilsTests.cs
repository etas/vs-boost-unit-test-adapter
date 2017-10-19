// https://github.com/csoltenborn/GoogleTestAdapter

using System.Text;

using BoostTestAdapter.Utility;

using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    [TestFixture]
    public class ByteUtilsTests
    {
        [Test]
        public void IndexOf_FooEmptyPattern_ReturnsFound()
        {
            var bytes = Encoding.ASCII.GetBytes("foo");
            var pattern = Encoding.ASCII.GetBytes("");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(0));
        }

        [Test]
        public void IndexOf_EmptyBytesFoo_ReturnsNotFound()
        {
            var bytes = Encoding.ASCII.GetBytes("");
            var pattern = Encoding.ASCII.GetBytes("foo");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_EmptyBytesEmptyPattern_ReturnsFound()
        {
            var bytes = Encoding.ASCII.GetBytes("");
            var pattern = Encoding.ASCII.GetBytes("");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(0));
        }

        [Test]
        public void IndexOf_FooBar_ReturnsNotFound()
        {
            var bytes = Encoding.ASCII.GetBytes("foofoofoo");
            var pattern = Encoding.ASCII.GetBytes("bar");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_FooAtBeginning_ReturnsFound()
        {
            var bytes = Encoding.ASCII.GetBytes("fooxxx");
            var pattern = Encoding.ASCII.GetBytes("foo");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(0));
        }

        [Test]
        public void IndexOf_FooAtEnd_ReturnsFound()
        {
            var bytes = Encoding.ASCII.GetBytes("xxxfoo");
            var pattern = Encoding.ASCII.GetBytes("foo");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(3));
        }

        [Test]
        public void IndexOf_FooInMiddle_ReturnsFound()
        {
            var bytes = Encoding.ASCII.GetBytes("xxxfooxxx");
            var pattern = Encoding.ASCII.GetBytes("foo");
            var index = bytes.IndexOf(pattern);

            Assert.That(index, Is.EqualTo(3));
        }

    }
}