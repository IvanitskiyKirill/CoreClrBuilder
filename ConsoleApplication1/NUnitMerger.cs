using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace CoreClrBuilder
{
    public static class NUnitMerger
    {
        public static bool MergeFiles(IEnumerable<string> files, string output)
        {
            XElement environment = null;
            XElement culture = null;
            var suites = new List<XElement>();

            bool finalSuccess = true;
            string finalResult = "";
            double totalTime = 0;
            int total = 0, errors = 0, failures = 0, notrun = 0, inconclusive = 0, ignored = 0, skipped = 0, invalid = 0;
            foreach (var file in files)
            {
                if (!File.Exists(file))
                    continue;
                var doc = XDocument.Load(file);
                var tr = doc.Element("test-results");

                if (environment == null)
                    environment = tr.Element("environment");
                if (culture == null)
                    culture = tr.Element("culture-info");

                if (tr.Attribute("total") != null)
                    total += Convert.ToInt32(tr.Attribute("total").Value);
                if (tr.Attribute("errors") != null)
                    errors += Convert.ToInt32(tr.Attribute("errors").Value);
                if (tr.Attribute("failures") != null)
                    failures += Convert.ToInt32(tr.Attribute("failures").Value);
                if (tr.Attribute("not-run") != null)
                    notrun += Convert.ToInt32(tr.Attribute("not-run").Value);
                if (tr.Attribute("inconclusive") != null)
                    inconclusive += Convert.ToInt32(tr.Attribute("inconclusive").Value);
                if (tr.Attribute("ignored") != null)
                    ignored += Convert.ToInt32(tr.Attribute("ignored").Value);
                if (tr.Attribute("skipped") != null)
                    skipped += Convert.ToInt32(tr.Attribute("skipped").Value);
                if (tr.Attribute("invalid") != null)
                    invalid += Convert.ToInt32(tr.Attribute("invalid").Value);

                var ts = tr.Element("test-suite");
                //string result = ts.Attribute("result").Value;

                if (!Convert.ToBoolean(ts.Attribute("success").Value))
                    finalSuccess = false;

                totalTime += Convert.ToDouble(ts.Attribute("time").Value);

                //if (finalResult != "Failure" && (String.IsNullOrEmpty(finalResult) || result == "Failure" || finalResult == "Success"))
                //    finalResult = result;

                suites.Add(ts);
            }

            if (String.IsNullOrEmpty(finalResult))
            {
                finalSuccess = false;
                finalResult = "Inconclusive";
            }

            var project = XElement.Parse(String.Format("<test-suite type=\"Test Project\" name=\"\" executed=\"True\" result=\"{0}\" success=\"{1}\" time=\"{2}\" asserts=\"0\" />", finalResult, finalSuccess ? "True" : "False", totalTime));
            var results = XElement.Parse("<results/>");
            results.Add(suites.ToArray());
            project.Add(results);

            var now = DateTime.Now;
            var trfinal = XElement.Parse(String.Format("<test-results name=\"Merged results\" total=\"{0}\" errors=\"{1}\" failures=\"{2}\" not-run=\"{3}\" inconclusive=\"{4}\" ignored=\"{5}\" skipped=\"{6}\" invalid=\"{7}\" date=\"{8}\" time=\"{9}\" />", total, errors, failures, notrun, inconclusive, ignored, skipped, invalid, now.ToString("yyyy-MM-dd"), now.ToString("HH:mm:ss")));
            trfinal.Add(new[] { environment, culture, project });
            trfinal.Save(output);

            return finalSuccess;
        }
    }
}
