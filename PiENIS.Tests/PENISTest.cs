using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiENIS.Tests
{
    [TestClass]
    public class PENISTest
    {
        private class TestData
        {
            public int[] Ints;
            public string Hey;
            public TestData Recursion;

            public override bool Equals(object obj) =>
                obj is TestData t
                && t.Ints.SequenceEqual(Ints)
                && t.Hey == Hey
                && (t.Recursion?.Equals(this.Recursion) ?? this.Recursion == null);
        }

        private const string ExampleFile = @"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey: what's up #not much, hbu
#it's ending
";

        private IFile File => new MemoryFile(ExampleFile);

        [TestMethod]
        public void DeserializeToObject()
        {
            var penis = new PENIS(File);

            Assert.AreEqual(penis.ToObject<TestData>(), new TestData
            {
                Ints = new[] { 1, 2, 3 },
                Hey = "what's up",
                Recursion = new TestData
                {
                    Ints = new[] { 4, 5, 6 },
                    Hey = "no"
                }
            });
        }

        [TestMethod]
        public void RemoveListItemWithComments()
        {
            var penis = new PENIS(File);

            penis.Remove("Ints");

            Assert.AreEqual(penis.Serialize(), @"#comment
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey: what's up #not much, hbu
#it's ending
");
        }

        [TestMethod]
        public void RemoveItemWithComments()
        {
            var penis = new PENIS(File);

            
penis.Remove("Hey");

            Assert.AreEqual(@"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
#it's ending
", penis.Serialize());
        }

        [TestMethod]
        public void AddItem()
        {
            var penis = new PENIS(File);

            penis.Set("Foo", "Bar");

            Assert.AreEqual(@"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey: what's up #not much, hbu
#it's ending

Foo: Bar", penis.Serialize());
        }

        [TestMethod]
        public void AddList()
        {
            var penis = new PENIS(File);

            penis.Set("Foo", new[] { 1, 2, 3 });

            Assert.AreEqual(@"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey: what's up #not much, hbu
#it's ending

Foo:
    - 1
    - 2
    - 3", penis.Serialize());
        }

        [TestMethod]
        public void SetItemToList()
        {
            var penis = new PENIS(File);

            penis.Set("Hey", new[] { 1, 2, 3 });

            Assert.AreEqual(@"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey:
    - 1
    - 2
    - 3
#it's ending
", penis.Serialize());
        }

        [TestMethod]
        public void SetListToItem()
        {
            var penis = new PENIS(File);

            penis.Set("Hey", new[] { 1, 2, 3 });
            penis.Set("Hey", 42);

            Assert.AreEqual(@"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey: 42
#it's ending
", penis.Serialize());
        }

        [TestMethod]
        public void SetItemToItem()
        {
            var penis = new PENIS(File);

            penis.Set("Hey", 42);

            Assert.AreEqual(@"#comment
Ints:
    - 1 #comment
    - 2
    - 3
#another comment

Recursion:
    Ints:
        - 4
        - 5
        - 6
    Hey: no

#before
Hey: 42 #not much, hbu
#it's ending
", penis.Serialize());
        }
    }
}
