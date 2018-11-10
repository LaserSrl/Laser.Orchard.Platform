using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace KrakeAdmin.Filters {

    public sealed class StringWriterWithEncoding : StringWriter {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding, IFormatProvider formatProvider)
            : base(formatProvider) {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}