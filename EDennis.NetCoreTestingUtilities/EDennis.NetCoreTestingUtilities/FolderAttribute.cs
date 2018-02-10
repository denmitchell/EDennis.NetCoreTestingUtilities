using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;

using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// This attribute is used with a Theory attribute to
    /// use all file names in a folder as input params to
    /// an xunit test
    /// </summary>
    public class FolderAttribute : DataAttribute {

        private string _path;
        private string _extension;

        /// <summary>
        /// Constructs a new Folder attribute with the
        /// path explicitly defined
        /// </summary>
        /// <param name="path">relative or absolute path to folder</param>
        public FolderAttribute(string path) {
            _path = path;
        }

        /// <summary>
        /// Constructs a new Folder attribute with the
        /// path explicitly defined
        /// </summary>
        /// <param name="path">relative or absolute path to folder</param>
        /// <param name="extension">The file extension sought</param>
        public FolderAttribute(string path, string extension) {
            _path = path;
            _extension = extension;
        }

        /// <summary>
        /// Constructs a new Folder attribute with the path
        /// implicitly defined, where the top level folder is
        /// the name of the class (minus any Test or Tests suffix)
        /// and the second level folder is the name of the test method
        /// minus any Test or Tests suffix or prefix.
        /// </summary>
        public FolderAttribute() { }


        /// <summary>
        /// Returns the paths of all files associated with the 
        /// FolderAttribute's path variable.
        /// </summary>
        /// <param name="testMethod">The unit test method</param>
        /// <returns>IEnumerable of string representing file paths</returns>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod) {

            if (testMethod == null) {
                throw new ArgumentNullException(nameof(testMethod));
            }

            if (_path == null) {
                var topLevelFolder = GetTopLevelFolder(testMethod);
                var secondLevelFolder = GetSecondLevelFolder(testMethod, topLevelFolder);
                _path = $"{topLevelFolder}\\{secondLevelFolder}";
            }

            var path = Path.IsPathRooted(_path)
            ? _path
            : Path.GetRelativePath(Directory.GetCurrentDirectory(), _path);

            if (!Directory.Exists(_path)) {
                throw new ArgumentException($"Could not find path: {path}");
            }

            IEnumerable<string> files = Directory.GetFiles(_path).ToList();
            if (_extension != null)
                files = files.Where(f => f.EndsWith(_extension));


            foreach (string file in files)
                yield return new object[] { file };

        }


        /// <summary>
        /// Tries to find the top-level folder by looking for
        /// a folder having the class name of the test method
        /// optionally prefixed by "Test" or suffixed by 
        /// "Test" or "Tests"
        /// </summary>
        /// <param name="testMethod">The unit test method</param>
        /// <returns>the name of the top-level folder</returns>
        private string GetTopLevelFolder(MethodInfo testMethod) {
            var className = testMethod.DeclaringType.Name;
            if (Directory.Exists(className)) {
                return className;
            } else {
                var test = Regex.Replace(className, "Tests?$", "");
                if (Directory.Exists(test))
                    return test;
                var test2 = Regex.Replace(className, "^Test", "");
                if (Directory.Exists(test2))
                    return test2;
                else
                    throw new ArgumentException($"Could not find folder for {className}.");
            }
        }

        /// <summary>
        /// Tries to find the second-level folder by looking for
        /// a folder having the name of the test method
        /// optionally prefixed by "Test" or suffixed by 
        /// "Test" or "Tests"
        /// </summary>
        /// <param name="testMethod">The unit test method</param>
        /// <returns>the name of the top-level folder</returns>
        private string GetSecondLevelFolder(MethodInfo testMethod, string topLevelFolder) {
            var methodName = testMethod.Name;
            if (Directory.Exists(topLevelFolder + "\\" + methodName)) {
                return methodName;
            } else {
                var test = Regex.Replace(methodName, "Tests?$", "");
                if (Directory.Exists(topLevelFolder + "\\" + test))
                    return test;
                var test2 = Regex.Replace(methodName, "^Test", "");
                if (Directory.Exists(topLevelFolder + "\\" + test2))
                    return test2;
                else
                    throw new ArgumentException($"Could not find folder for {topLevelFolder + "\\" + methodName}.");
            }
        }


    }
}


