using EDennis.NetCoreTestingUtilities.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// This class provides a way to test a method whose
    /// input parameters and expected output are retrieved
    /// from a JSON file
    /// </summary>
    public class JsonAssert {


        /// <summary>
        /// Executes a test whose method parameters and expected
        /// return value are defined in a JSON file.  NOTE: this
        /// method assumes an instance (non-static) method.
        /// NOTE: for this version of the ExecuteTest method, the
        /// JSON file must reside in a subfolder having the same
        /// name as the method under test.  If the method subfolder
        /// resides in a top-level folder having the class name under
        /// test, then the filePath parameter can consist of just the
        /// file name.  The method name will be determined by 
        /// reflection.
        /// </summary>
        /// <typeparam name="T">Type of the object under test</typeparam>
        /// <param name="obj">The object under test</param>
        /// <param name="filePath">The path to the JSON file.  The file
        /// path can be the full (relative) file path or, if the file
        /// is nested in a folder named for the class name under test and a 
        /// subfolder named for the method name under test, the simple 
        /// file name (with or without the .json extension)
        /// </param>
        /// <seealso cref="ExecuteTest{T}(T, string, string)"/>
        public static void ExecuteTest<T>(T obj, string filePath){
            Test<T> test;
            if (Regex.IsMatch(filePath, @"(?:[^\\]*\\)?([^\\]*)\\[^\\]*")) {
                var methodName = Regex.Replace(filePath, @"(?:[^\\]*\\)?([^\\]*)\\[^\\]*", "$1");
                test = new Test<T>(obj, methodName, filePath);
            } else {
                test = new Test<T>(obj, filePath);
            }
            ExecuteTest(obj, test);
        }


        /// <summary>
        /// Executes a test whose method parameters and expected
        /// return value are defined in a JSON file.  NOTE: this
        /// method assumes an instance (non-static) method.
        /// </summary>
        /// <typeparam name="T">The type of the object under test</typeparam>
        /// <param name="obj">The object under test</param>
        /// <param name="methodName">The name of the method under test</param>
        /// <param name="filePath">File path to the JSON test file</param>
        /// <seealso cref="ExecuteTest{T}(T, string)"/>
        public static void ExecuteTest<T>(T obj, string methodName, string filePath) {
            var test = new Test<T>(obj, methodName, filePath);
            ExecuteTest(obj, test);
        }


        /// <summary>
        /// Executes a test whose method parameters and expected
        /// return value are defined in a JSON file.  NOTE: this
        /// method assumes an instance (non-static) method.
        /// </summary>
        /// <typeparam name="T">Type of the object under test</typeparam>
        /// <param name="obj">The object under test</param>
        /// <param name="test">A test object, which holds the class name,
        /// method name, and test case name for the test.  The class name
        /// and method name are used to execute the method via reflection</param>
        private static void ExecuteTest<T>(T obj, Test<T> test) {

            //use reflection to get method- and parameter-related objects
            MethodInfo methodInfo = obj.GetType().GetMethod(test.MethodName);
            ParameterInfo[] parameters = methodInfo.GetParameters();

            //initialize actualOutput
            object actualOutput = null;

            if (parameters.Length == 0) {
                //invoke method without params
                actualOutput = methodInfo.Invoke(obj, null);
            } else {
                //build a list of parameters, obtaining parameter values
                //(arguments) from the JSON test file
                List<object> args = new List<object>();
                for (int i = 0; i < parameters.Length; i++) {
                    ParameterInfo info = parameters[i];
                    object arg = null;
                    try {
                        arg = test.JToken[info.Name].ToObject(info.ParameterType);
                        args.Add(arg);
                    } catch (Exception) {
                        throw new FormatException($"Method argument \"{info.Name}\" is missing in the JSON test file.  \"{info.Name}\" must be a top-level object property in that file.");
                    }
                }
                //invoke method with params
                actualOutput = methodInfo.Invoke(obj, args.ToArray());
            }

            //get expectedOutput from JSON file
            object expectedOutput = null;
            try {
                expectedOutput = test.JToken["returns"].ToObject(methodInfo.ReturnType);
            } catch (Exception e) {
                throw new FormatException($"\"returns\" is missing in the JSON test file.  \"returns\" must be a top-level object property in that file. ..." + e.Message);
            }

            //test expected versus actual output
            TestOutput(expectedOutput, actualOutput);
        }


        /// <summary>
        /// Tests the expected vs. actual output.  When a test fails, it
        /// throws a JsonAssertException
        /// </summary>
        /// <param name="expectedOutput">The expected output</param>
        /// <param name="actualOutput">The actual output</param>
        protected static void TestOutput(object expectedOutput, object actualOutput) {

            //if the expectedJson doesn't equal the actualJson, throw an exception
            if (expectedOutput.ToJsonString() != actualOutput.ToJsonString()) {
                throw new JsonAssertException(expectedOutput, actualOutput);
            }
        }

        /// <summary>
        /// This class holds class name, method name, and test case name for the
        /// specific Test Case.  The class also holds the JSON.NET JToken object
        /// representation of the test case.
        /// </summary>
        /// <typeparam name="T">The type of the object under test</typeparam>
        private class Test<T> {

            /// <summary>
            /// The name of the class being tested
            /// </summary>
            public string ClassName { get; set; }

            /// <summary>
            /// The name of the method being tested
            /// </summary>
            public string MethodName { get; set; }

            /// <summary>
            /// The name of the JSON test file (excluding .json extension)
            /// </summary>
            public string TestCase { get; set; }

            /// <summary>
            /// The JSON representation of the test case
            /// </summary>
            public JToken JToken { get; set; }

            /// <summary>
            /// Instantiates a new Test object using the
            /// provided object and test case params
            /// </summary>
            /// <param name="obj">The object under test</param>
            /// <param name="testCase">The name of the JSON file (excluding its extension)</param>
            public Test(T obj, string testCase) {
                ClassName = obj.GetType().Name;
                TestCase = Regex.Replace(testCase,"\\.json$","");
                GetMethodName(ClassName);
                BuildJToken();
            }

            /// <summary>
            /// Instantiates a new Test object based upon
            /// the provided object, method name, and test
            /// case file path.
            /// </summary>
            /// <param name="obj">The object under test</param>
            /// <param name="methodName">The method under test</param>
            /// <param name="filePath">The full path to the JSON test file</param>
            public Test(T obj, string methodName, string filePath) {
                ClassName = obj.GetType().Name;
                TestCase = Path.GetFileNameWithoutExtension(filePath);
                MethodName = methodName;
                BuildJToken(filePath);
            }

            /// <summary>
            /// Builds the JSON representation of the test case, 
            /// assuming that the path conforms to the following:
            /// PROJECT ROOT > ClassName > MethodName > TestCase.json
            /// </summary>
            private void BuildJToken() {
                string filePath = $"{ClassName}\\{MethodName}\\{TestCase}.json";
                BuildJToken(filePath);
            }

            /// <summary>
            /// Builds the JSON representation of the test case 
            /// </summary>
            /// <param name="filePath">Full path to the JSON test case file</param>
            private void BuildJToken(string filePath) {
                string json = System.IO.File.ReadAllText(filePath);
                JToken = JToken.Parse(json);
            }

            /// <summary>
            /// Uses reflection to determine the name of the method being tested,
            /// when the method name is not explicitly specified.  The current
            /// StackTrace is traversed until the target class name is found.  (The
            /// className may have "Tests" or "Test" appended or "Test" prepended;
            /// otherwise, it must exactly match the class under test.)  Once the 
            /// class name is found, the method name is extracted.  NOTE: do not
            /// call a helper method in the test class; otherwise, this method will
            /// not work.
            /// </summary>
            /// <param name="targetClassName"></param>
            private void GetMethodName(String targetClassName) {
                StackTrace stackTrace = new StackTrace();
                for (int i = 0; i < stackTrace.FrameCount; i++) {
                    StackFrame frame = stackTrace.GetFrame(i);
                    string className = frame.GetMethod().ReflectedType.Name;
                    if (className == targetClassName 
                        || className == targetClassName + "Test"
                        || className == targetClassName + "Tests"
                        || className == "Test" + targetClassName) {
                        MethodName = frame.GetMethod().Name;
                        break;
                    }
                }
            }

        }



    }

}

