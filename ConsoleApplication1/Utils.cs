using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder
{
    class PlatformPathsCorrector {
        public const char UnixSeparator = '/';
        public const char WindowsSeparator = '\\';

        private static PlatformPathsCorrector inst;
        public static PlatformPathsCorrector Inst {
            get {
                if (inst == null)
                    inst = new PlatformPathsCorrector();
                return inst;
            }
        }
        private PlatformPathsCorrector() {

        }

        public char PlatformSeparator { get { return Platform == Platform.Windows ? WindowsSeparator : UnixSeparator; } }
        public Platform Platform { get; set; }

        public string Correct(string path, Platform pathPlatform) {
            if (pathPlatform == Platform.Windows && Platform == Platform.Unix)
                return path.Replace(WindowsSeparator, UnixSeparator);
            return path;
        }
    }

    static class TestbuildPathHelper {
        public static string Combine(string path, string runtime, string framework) {
            return PlatformPathsCorrector.Inst.Correct(Path.Combine(path, EnvironmentSettings.Platform.ToString(), string.Join("-", runtime, framework)), Platform.Windows);
        }
    }
}
