using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace NUnit.Engine.Addins {
    public interface IResultWriter {
        // Methods
        void CheckWritability(string outputPath);
        void WriteResultFile(XmlNode resultNode, TextWriter writer);
        void WriteResultFile(XmlNode resultNode, string outputPath);
    }
    public class MyNUnit2XmlResultWriter : IResultWriter {
        private XmlWriter xmlWriter;

        private static string CharacterSafeString(string encodedString) {
            char[] chars = Encoding.Default.GetChars(Encoding.Default.GetBytes(encodedString));
            ArrayList list = new ArrayList();
            for(int i = 0; i < chars.Length; i++) {
                char ch = chars[i];
                if(((ch < ' ') && (ch != '\t')) && ((ch != '\n') && (ch != '\r'))) {
                    list.Add(i);
                }
            }
            foreach(int num2 in list) {
                chars[num2] = '?';
            }
            return Encoding.Default.GetString(Encoding.Default.GetBytes(chars));
        }

        public void CheckWritability(string outputPath) {
            using(new StreamWriter(outputPath, false, Encoding.UTF8)) {
            }
        }

        private void InitializeXmlFile(XmlNode result) {
            NUnit2ResultSummary summary = new NUnit2ResultSummary(result);
            this.xmlWriter.WriteStartDocument(false);
            //this.xmlWriter.WriteComment("This file represents the results of running a test suite");
            this.xmlWriter.WriteStartElement("test-results");
            this.xmlWriter.WriteAttributeString("name", "testSet"); // this.xmlWriter.WriteAttributeString("name", GetAttribute(result, "fullname"));
            this.xmlWriter.WriteAttributeString("total", summary.ResultCount.ToString());
            //this.xmlWriter.WriteAttributeString("errors", summary.Errors.ToString());
            this.xmlWriter.WriteAttributeString("failures", summary.Failures.ToString());
            this.xmlWriter.WriteAttributeString("not-run", summary.TestsNotRun.ToString());
            //this.xmlWriter.WriteAttributeString("inconclusive", summary.Inconclusive.ToString());
            //this.xmlWriter.WriteAttributeString("ignored", summary.Ignored.ToString());
            //this.xmlWriter.WriteAttributeString("skipped", summary.Skipped.ToString());
            //this.xmlWriter.WriteAttributeString("invalid", summary.NotRunnable.ToString());
            DateTime time = GetAttribute(result, "start-time", DateTime.UtcNow);
            this.xmlWriter.WriteAttributeString("date", time.ToString("yyyy-MM-dd"));
            this.xmlWriter.WriteAttributeString("time", time.ToString("HH:mm:ss"));
            //this.WriteEnvironment();
            //this.WriteCultureInfo();
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
        private void StartTestElement(XmlNode result, string nameResult) {
            bool isTestCase = nameResult == "test-case";
            if(isTestCase) {
                this.xmlWriter.WriteStartElement("test-case");
                this.xmlWriter.WriteAttributeString("name", GetAttribute(result, "fullname"));
            } else {
                var isRoot = (nameResult == "root");
                string str6 = isRoot ? "root" : GetAttribute(result, "type");
                this.xmlWriter.WriteStartElement("test-suite");
                //this.xmlWriter.WriteAttributeString("type", str6);
                this.xmlWriter.WriteAttributeString("name", isRoot ? "root" : GetAttribute(result, "name"));
            }
            //XmlNode node = result.SelectSingleNode("properties/property[@name='Description']");
            //if(node != null) {
            //    string str8 = GetAttribute(node, "value");
            //    if(str8 != null) {
            //        this.xmlWriter.WriteAttributeString("description", str8);
            //    }
            //}
            string attribute = GetAttribute(result, "result");
            string label = GetAttribute(result, "label");
            string str3 = (attribute == "Skipped") ? "False" : "True";
            string str4 = (attribute == "Passed") ? "True" : "False";
            double num = GetAttribute(result, "duration", (double)0.0);
            string str5 = GetAttribute(result, "asserts");
            this.xmlWriter.WriteAttributeString("executed", str3);
            //this.xmlWriter.WriteAttributeString("result", this.TranslateResult(attribute, label));
            if(!isTestCase) {
                if(str3 == "False") { // Skipped
                    str4 = (GetAttribute(result, "failed") == "0") ? "True" : "False";
                }
                this.xmlWriter.WriteAttributeString("success", str4);
                this.xmlWriter.WriteAttributeString("time", num.ToString("#####0.000", NumberFormatInfo.InvariantInfo));
                this.xmlWriter.WriteAttributeString("asserts", str5);
            } else {
                if(str3 == "True") {  // only for test
                    this.xmlWriter.WriteAttributeString("success", str4);
                    this.xmlWriter.WriteAttributeString("time", num.ToString("#####0.000", NumberFormatInfo.InvariantInfo));
                    this.xmlWriter.WriteAttributeString("asserts", str5);
                }
            }
        }

        private void TerminateXmlFile() {
            this.xmlWriter.WriteEndElement();
            this.xmlWriter.WriteEndDocument();
            this.xmlWriter.Flush();
            this.xmlWriter.Close();
        }

        private string TranslateResult(string resultState, string label) {
            if(resultState != "Passed") {
                if(resultState == "Inconclusive") {
                    return "Inconclusive";
                }
                if(resultState == "Failed") {
                    if((label != "Error") && (label != "Cancelled")) {
                        return "Failure";
                    }
                    return label;
                }
                if(resultState == "Skipped") {
                    if(label != "Ignored") {
                        if(label == "Invalid") {
                            return "NotRunnable";
                        }
                        return "Skipped";
                    }
                    return "Ignored";
                }
            }
            return "Success";
        }

        private void WriteCategoriesElement(XmlNode properties) {
            XmlNodeList list = properties.SelectNodes("property[@name='Category']");
            if(list.Count != 0) {
                this.xmlWriter.WriteStartElement("categories");
                foreach(XmlNode node in list) {
                    this.xmlWriter.WriteStartElement("category");
                    this.xmlWriter.WriteAttributeString("name", GetAttribute(node, "value"));
                    this.xmlWriter.WriteEndElement();
                }
                this.xmlWriter.WriteEndElement();
            }
        }

        private void WriteCData(string text) {
            int startIndex = 0;
            do {
                int index = text.IndexOf("]]>", startIndex);
                if(index < 0) {
                    if(startIndex > 0) {
                        this.xmlWriter.WriteCData(text.Substring(startIndex));
                    } else {
                        this.xmlWriter.WriteCData(text);
                    }
                    return;
                }
                this.xmlWriter.WriteCData(text.Substring(startIndex, (index - startIndex) + 2));
                startIndex = index + 2;
            }
            while(startIndex < text.Length);
        }

        private void WriteChildResults(XmlNode result) {
            this.xmlWriter.WriteStartElement("results");
            foreach(XmlNode node in result.ChildNodes) {
                if(node.Name.StartsWith("test-")) {
                    this.WriteResultElement(node, false);
                }
            }
            this.xmlWriter.WriteEndElement();
        }

        private void WriteCultureInfo() {
            this.xmlWriter.WriteStartElement("culture-info");
            this.xmlWriter.WriteAttributeString("current-culture", CultureInfo.CurrentCulture.ToString());
            this.xmlWriter.WriteAttributeString("current-uiculture", CultureInfo.CurrentUICulture.ToString());
            this.xmlWriter.WriteEndElement();
        }

        private void WriteEnvironment() {
            this.xmlWriter.WriteStartElement("environment");
            this.xmlWriter.WriteAttributeString("nunit-version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            this.xmlWriter.WriteAttributeString("clr-version", Environment.Version.ToString());
            this.xmlWriter.WriteAttributeString("os-version", Environment.OSVersion.ToString());
            this.xmlWriter.WriteAttributeString("platform", Environment.OSVersion.Platform.ToString());
            this.xmlWriter.WriteAttributeString("cwd", Environment.CurrentDirectory);
            this.xmlWriter.WriteAttributeString("machine-name", Environment.MachineName);
            this.xmlWriter.WriteAttributeString("user", Environment.UserName);
            this.xmlWriter.WriteAttributeString("user-domain", Environment.UserDomainName);
            this.xmlWriter.WriteEndElement();
        }

        private void WriteFailureElement(string message, string stackTrace) {
            this.xmlWriter.WriteStartElement("failure");
            this.xmlWriter.WriteStartElement("message");
            this.WriteCData(message);
            this.xmlWriter.WriteEndElement();
            this.xmlWriter.WriteStartElement("stack-trace");
            if(stackTrace != null) {
                this.WriteCData(stackTrace);
            }
            this.xmlWriter.WriteEndElement();
            this.xmlWriter.WriteEndElement();
        }

        private void WritePropertiesElement(XmlNode properties) {
            XmlNodeList list = properties.SelectNodes("property");
            XmlNodeList list2 = properties.SelectNodes("property[@name='Category']");
            if(list.Count != list2.Count) {
                this.xmlWriter.WriteStartElement("properties");
                foreach(XmlNode node in list) {
                    if(GetAttribute(node, "name") != "Category") {
                        this.xmlWriter.WriteStartElement("property");
                        this.xmlWriter.WriteAttributeString("name", GetAttribute(node, "name"));
                        this.xmlWriter.WriteAttributeString("value", GetAttribute(node, "value"));
                        this.xmlWriter.WriteEndElement();
                    }
                }
                this.xmlWriter.WriteEndElement();
            }
        }

        private void WriteReasonElement(string message) {
            this.xmlWriter.WriteStartElement("reason");
            this.xmlWriter.WriteStartElement("message");
            this.WriteCData(message);
            this.xmlWriter.WriteEndElement();
            this.xmlWriter.WriteEndElement();
        }

        private void WriteResultElement(XmlNode result, bool isRoot) {
            if(isRoot) {
                //string oldName = result.Name;
                this.StartTestElement(result, "root");
                this.xmlWriter.WriteStartElement("results");
            }
            this.StartTestElement(result, result.Name);
            //XmlNode properties = result.SelectSingleNode("properties");
            //if(properties != null) {
            //    this.WriteCategoriesElement(properties);
            //    this.WritePropertiesElement(properties);
            //}
            XmlNode node2 = result.SelectSingleNode("reason/message");
            if(node2 != null) {
                this.WriteReasonElement(node2.InnerText);
            }
            node2 = result.SelectSingleNode("failure/message");
            XmlNode node3 = result.SelectSingleNode("failure/stack-trace");
            if(node2 != null) {

                this.WriteFailureElement(node2.InnerText, node3 == null ? null : node3.InnerText);
            }
            if(result.Name != "test-case") {
                this.WriteChildResults(result);
            }
            this.xmlWriter.WriteEndElement();
            if(isRoot) {
                this.xmlWriter.WriteEndElement();
            }
        }
        public void WriteResultFile(XmlNode result, TextWriter writer) {
            using(XmlTextWriter writer2 = new XmlTextWriter(writer)) {
                writer2.Formatting = Formatting.Indented;
                this.WriteXmlOutput(result, writer2);
            }
        }

        public void WriteResultFile(XmlNode result, string outputPath) {
            using(StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8)) {
                this.WriteResultFile(result, writer);
            }
        }

        private void WriteXmlOutput(XmlNode result, XmlWriter xmlWriter) {
            this.xmlWriter = xmlWriter;
            this.InitializeXmlFile(result);
            bool needRootNode = false; // true;
            foreach(XmlNode node in result.ChildNodes) {
                if(node.Name.StartsWith("test-")) {
                    this.WriteResultElement(node, needRootNode);
                    needRootNode = false;
                }
            }

            this.TerminateXmlFile();
        }
    }
}
