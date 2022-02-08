using FluentAssertions;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Floatingman.ByteOrderMarker
{
    public class EncodingTests
    {
        private readonly ITestOutputHelper output;

        public EncodingTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CanSeeBomOnUTF8()
        {
            // by default the BOM is not required on UTF 8
            var bom = new UTF8Encoding(true);

            var preamble = bom.GetPreamble();
            preamble[0].Should().Be(0xEF);
            preamble[1].Should().Be(0xBB);
            preamble[2].Should().Be(0xBF);
        }

        [Fact]
        public void CanSeeBomOnUTF16()
        {
            var bom = new UnicodeEncoding();

            var preamble = bom.GetPreamble();
            preamble[0].Should().Be(0xFF);
        }

        [Fact]
        public void BinaryReaderDetectEncoding()
        {
            var filename = "sampler.txt";
            using (var f = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] bom = new byte[4];
                f.Read(bom, 0, bom.Length);

                bom[0].Should().Be(0xEF);
                bom[1].Should().Be(0xBB);
                bom[2].Should().Be(0xBF);

            }
        }

        [Fact]
        public void StreamReaderDetect()
        {
            var filename = "sampler.txt";
            // stream reader by default skips the BOM
            using (var f = new StreamReader(filename, true))
            {
                var c = f.Read();
                f.CurrentEncoding.Should().Be(Encoding.UTF8);
                // c.Should().Be(0xFF);
            }
        }

        [Fact]
        public void StreamReaderDefault()
        {
            var filename = "sampler.txt";
            // stream reader by default skips the BOM
            using (var f = new StreamReader(filename))
            {
                var c = f.Read();
                f.CurrentEncoding.Should().Be(Encoding.UTF8);
                // c.Should().Be(0xFF);
            }
        }

        [Fact]
        public void GetEncoding()

        {
            var filename = "sampler.txt";
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // the default?
            Encoding enc = Encoding.UTF8;

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) enc = Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) enc = Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) enc = Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) enc = Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) enc = Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) enc = new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            enc.Should().Be(Encoding.UTF8);
        }

        [Fact]
        public void WriteBomToStream()
        {
            var filename = "sampler.txt";
            var text = "text";
            var encoding = new UTF8Encoding(true);
            var bytes = encoding.GetBytes(text);
            var preamble = encoding.GetPreamble();
            using (var f = new FileStream(filename, FileMode.Create))
            {
                f.Write(preamble, 0, preamble.Length);
                f.Write(bytes, 0, bytes.Length);
            }
            (new FileInfo(filename)).Length.Should().Be(7);
        }
    }
}