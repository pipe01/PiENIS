using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiENIS.Tests
{
    [TestClass]
    public class ConvertTest
    {
        private class TestClass
        {
            public enum TestEnum
            {
                FirstValue,
                SecondValue,
                ThirdValue
            }

#pragma warning disable CS0649
            public int Integer;
            public string String { get; set; }
            public float Float;
            public TestEnum Enum { get; set; }
            public int[] IntArray;
            public TestEnum[] EnumArray { get; set; }
            public int[][][] DeepIntArray;
            public TestEnum[][][] DeepEnumArray { get; set; }
            public TestClass Nested;
#pragma warning restore CS0649
        }

        [TestMethod]
        public void Integer()
        {
            Assert.AreEqual(42, PenisConvert.DeserializeObject<TestClass>("Integer: 42").Integer);
        }

        [TestMethod]
        public void String()
        {
            Assert.AreEqual("foobar", PenisConvert.DeserializeObject<TestClass>("String: foobar").String);
        }

        [TestMethod]
        public void Float()
        {
            Assert.AreEqual(420.5, PenisConvert.DeserializeObject<TestClass>("Float: 420.5").Float);
        }

        [TestMethod]
        public void Enum()
        {
            Assert.AreEqual(TestClass.TestEnum.SecondValue,
                PenisConvert.DeserializeObject<TestClass>("Enum: SecondValue").Enum);
        }

        [TestMethod]
        public void IntArray()
        {
            Assert.IsTrue(PenisConvert.DeserializeObject<TestClass>(@"IntArray:
    - 1
    - 2
    - 3
    - 4").IntArray.SequenceEqual(new[] { 1, 2, 3, 4 }));
        }

        [TestMethod]
        public void EnumArray()
        {
            Assert.IsTrue(PenisConvert.DeserializeObject<TestClass>(@"EnumArray:
    - FirstValue
    - SecondValue
    - ThirdValue").EnumArray.SequenceEqual(
                new[] { TestClass.TestEnum.FirstValue, TestClass.TestEnum.SecondValue, TestClass.TestEnum.ThirdValue }));
        }

        [TestMethod]
        public void DeepIntArray()
        {
            var deep = PenisConvert.DeserializeObject<TestClass>(@"DeepIntArray:
    -
        -
            - 1
            - 2
        -
            - 3
            - 4
    -
        -
            - 5
            - 6
        -
            - 7
            - 8").DeepIntArray;

            Assert.IsTrue(deep[0][0].SequenceEqual(new[] { 1, 2 }));
            Assert.IsTrue(deep[0][1].SequenceEqual(new[] { 3, 4 }));
            Assert.IsTrue(deep[1][0].SequenceEqual(new[] { 5, 6 }));
            Assert.IsTrue(deep[1][1].SequenceEqual(new[] { 7, 8 }));
        }

        [TestMethod]
        public void DeepEnumArray()
        {
            var deep = PenisConvert.DeserializeObject<TestClass>(@"DeepEnumArray:
    -
        -
            - FirstValue
            - SecondValue
        -
            - ThirdValue
            - FirstValue
    -
        -
            - SecondValue
            - ThirdValue
        -
            - FirstValue
            - SecondValue").DeepEnumArray;

            Assert.IsTrue(deep[0][0].SequenceEqual(new[] { TestClass.TestEnum.FirstValue, TestClass.TestEnum.SecondValue }));
            Assert.IsTrue(deep[0][1].SequenceEqual(new[] { TestClass.TestEnum.ThirdValue, TestClass.TestEnum.FirstValue }));
            Assert.IsTrue(deep[1][0].SequenceEqual(new[] { TestClass.TestEnum.SecondValue, TestClass.TestEnum.ThirdValue }));
            Assert.IsTrue(deep[1][1].SequenceEqual(new[] { TestClass.TestEnum.FirstValue, TestClass.TestEnum.SecondValue }));
        }

        [TestMethod]
        public void NestedObject()
        {
            Assert.AreEqual(42, PenisConvert.DeserializeObject<TestClass>(@"Nested:
    Nested:
        Nested:
            Integer: 42").Nested.Nested.Nested.Integer);
        }
    }
}
