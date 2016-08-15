using System;
using System.Xml;

namespace NUnit.Engine.Addins {
    public class NUnit2ResultSummary {
        private double duration;
        private DateTime endTime;
        private int errorCount;
        private int failureCount;
        private int ignoreCount;
        private int inconclusiveCount;
        private string name;
        private int notRunnable;
        private int resultCount;
        private int skipCount;
        private DateTime startTime;
        private int successCount;
        private int testsRun;

        public NUnit2ResultSummary() {
            this.startTime = DateTime.MinValue;
            this.endTime = DateTime.MaxValue;
        }
        private double GetAttribute(XmlNode node, string name, double defaultValue) {
            XmlNode value = node.Attributes.GetNamedItem(name);
            return value == null ? defaultValue : Convert.ToDouble(value.Value);
        }
        private string GetAttribute(XmlNode node, string name, string defaultValue = "") {
            XmlNode value = node.Attributes.GetNamedItem(name);
            return value == null ? defaultValue : value.Value;
        }
        private DateTime GetAttribute(XmlNode node, string name, DateTime defaultValue) {
            XmlNode value = node.Attributes.GetNamedItem(name);
            return value == null ? defaultValue : Convert.ToDateTime(value.Value);
        }
        public NUnit2ResultSummary(XmlNode result) {
            this.startTime = DateTime.MinValue;
            this.endTime = DateTime.MaxValue;
            if(result.Name != "test-run") {
                throw new InvalidOperationException("Expected <test-run> as top-level element but was <" + result.Name + ">");
            }
            this.name = GetAttribute(result, "name");
            this.duration = GetAttribute(result, "duration", (double)0.0);
            this.startTime = GetAttribute(result, "start-time", DateTime.MinValue);
            this.endTime = GetAttribute(result, "end-time", DateTime.MaxValue);
            this.Summarize(result);
        }

        private void Summarize(XmlNode node) {
            if(node.Name != "test-case") {
                foreach(XmlNode node2 in node.ChildNodes) {
                    this.Summarize(node2);
                }
                return;
            }
            this.resultCount++;
            string attribute = GetAttribute(node, "result");
            string str3 = GetAttribute(node, "label");
            if(str3 != null) {
                attribute = str3;
            }
            switch(attribute) {
                case "Skipped":
                    this.skipCount++;
                    return;
                case "Failed":
                    this.failureCount++;
                    this.testsRun++;
                    return;
                case "Ignored":
                    this.ignoreCount++;
                    return;
                case "Passed":
                    this.successCount++;
                    this.testsRun++;
                    return;
                case "Inconclusive":
                    this.inconclusiveCount++;
                    this.testsRun++;
                    return;
                case "Cancelled":
                    break;
                case "Error":
                    this.failureCount++;
                    this.testsRun++;
                    break;
                case "NotRunnable":
                case "Invalid":
                    this.notRunnable++;
                    return;
                default:
                    this.skipCount++;
                    return;
            }
            this.errorCount++;
            this.testsRun++;
        }

        //public double Duration { get { return this.duration; } }

        //public DateTime EndTime =>
        //    this.endTime;

        //public int Errors =>
        //    this.errorCount;

        //public int ErrorsAndFailures =>
        //    (this.errorCount + this.failureCount);

        public int Failures
        {
            get
            {
                return this.failureCount;
            }
        }
        //public int Ignored =>
        //    this.ignoreCount;

        //public int Inconclusive =>
        //    this.inconclusiveCount;

        //public string Name =>
        //    this.name;

        //public int NotRunnable =>
        //    this.notRunnable;

        //public int Passed =>
        //    this.successCount;

        public int ResultCount
        {
            get
            {
                return this.resultCount;
            }
        }
        //public int Skipped =>
        //    this.skipCount;

        //public DateTime StartTime =>
        //    this.startTime;

        //public bool Success =>
        //    (this.failureCount == 0);

        public int TestsNotRun
        {
            get
            {
                return ((this.skipCount + this.ignoreCount) + this.notRunnable);
            }
        }
        //public int TestsRun =>
        //    this.testsRun;
    }
}