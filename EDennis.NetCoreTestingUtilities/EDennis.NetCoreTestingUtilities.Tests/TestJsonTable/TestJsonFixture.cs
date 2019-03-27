using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.TestJsonTable {
    public class TestJsonFixture : IDisposable {

        public TestJsonFixture() {
            using (var context = new TestJsonContext()) {

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var range = context.TestJsons.Where(r => r.ProjectName == "SomeProject");
                context.TestJsons.RemoveRange(range);
                context.SaveChanges();

                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "A", TestFile = "Input", Json = "[1]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "A", TestFile = "Expected", Json = "[2]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "B", TestFile = "Input", Json = "[3]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "B", TestFile = "Expected", Json = "[4]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "A", TestFile = "Input", Json = "[5]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "A", TestFile = "Expected", Json = "[6]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "B", TestFile = "Input", Json = "[7]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "B", TestFile = "Expected", Json = "[8]"
                });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioA",
                        TestCase = "TestCaseA",
                        TestFile = "Input",
                        Json = "123"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioA",
                        TestCase = "TestCaseA",
                        TestFile = "Expected",
                        Json = "[\"A\",\"B\",\"C\"]"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioA",
                        TestCase = "TestCaseB",
                        TestFile = "Input",
                        Json = "2018-01-01"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioA",
                        TestCase = "TestCaseB",
                        TestFile = "Expected",
                        Json = "[\"D\",\"E\",\"F\"]"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioB",
                        TestCase = "TestCaseA",
                        TestFile = "Input",
                        Json = "789"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioB",
                        TestCase = "TestCaseA",
                        TestFile = "Expected",
                        Json = "[\"G\",\"H\",\"I\"]"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioB",
                        TestCase = "TestCaseB",
                        TestFile = "Input",
                        Json = "abc"
                    });

                    context.TestJsons.Add(new TestJson {
                        ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                        ClassName = "ClassA",
                        MethodName = "MethodA",
                        TestScenario = "TestScenarioB",
                        TestCase = "TestCaseB",
                        TestFile = "Expected",
                        Json = "[\"J\",\"K\",\"L\"]"
                    });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "",
                    TestCase = "TestCaseA",
                    TestFile = "Input",
                    Json = "uvw"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "",
                    TestCase = "TestCaseA",
                    TestFile = "Expected",
                    Json = "[\"U\",\"V\",\"W\"]"
                });


                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "",
                    TestCase = "TestCaseB",
                    TestFile = "Input",
                    Json = "xyz"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "",
                    TestCase = "TestCaseB",
                    TestFile = "Expected",
                    Json = "[\"X\",\"Y\",\"Z\"]"
                });

                context.SaveChanges();

            }
        }

            public void Dispose() {
            // ... clean up test data from the database ...
        }

    }
}
