// /*
//  * Copyright Copyright 2012, System Insights, Inc.
//  *
//  *    Licensed under the Apache License, Version 2.0 (the "License");
//  *    you may not use this file except in compliance with the License.
//  *    You may obtain a copy of the License at
//  *
//  *       http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *    Unless required by applicable law or agreed to in writing, software
//  *    distributed under the License is distributed on an "AS IS" BASIS,
//  *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *    See the License for the specific language governing permissions and
//  *    limitations under the License.
//  */
// using MTConnect.DataElements;
// using NUnit.Framework;
// using System.Collections.Generic;

using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using MTConnect.DataElements.Conditions;
using MTConnect.Utilities.Time;
using Xunit;

namespace MTConnect.utests.DataElements.Conditions
{
    public class SimpleConditionTests
    {
        private SimpleCondition uut;
        private Mock<ITimeProvider> _mockTimeProvider;

        public SimpleConditionTests()
        {
            _mockTimeProvider = new Mock<ITimeProvider>();
            uut = new SimpleCondition("device", "condition", _mockTimeProvider.Object);
        }

        [Fact]
        public void NotAvaialbeOnCreation()
        {
            uut.Available.Should().BeFalse();
        }
        
        [Fact]
        public void SetNormal()
        {
            _mockTimeProvider
                .Setup(tp => tp.Now)
                .Returns(new DateTime(1,2,3,4,5,6, DateTimeKind.Utc));
            uut.SetNormal();
            uut.Available.Should().BeTrue();
            uut.Value.Should().BeEquivalentTo(new HashSet<ConditionValue> { ConditionValue.NormalConditionValue(_mockTimeProvider.Object.Now, "device", "condition") });
        }

        [Fact]
        public void RemoveByNativeCode()
        {
            uut.AddCondition(
                new ConditionValue
                {
                    Timestamp = new DateTime(1, 2, 3, 4, 5, 6, DateTimeKind.Utc),
                    DeviceName = "device",
                    ConditionName = "condition",
                    NativeCode = "native code",
                    NativeSeverity = "native severity",
                    Level = ConditionLevel.Normal,
                    Qualifier = ConditionQualifier.None,
                    Message = "message"
                }
            );
            uut.HasChanged.Should().BeTrue();

            uut.AddToUpdate(new StringBuilder());

            uut.HasChanged.Should().BeFalse();

            uut
                .Value
                .Should()
                .BeEquivalentTo(
                    new HashSet<ConditionValue> 
                    {
                        new ConditionValue
                        {
                            Timestamp = new DateTime(1, 2, 3, 4, 5, 6, DateTimeKind.Utc),
                            DeviceName = "device",
                            ConditionName = "condition",
                            NativeCode = "native code",
                            NativeSeverity = "native severity",
                            Level = ConditionLevel.Normal,
                            Qualifier = ConditionQualifier.None,
                            Message = "message"
                        }
                    }
                );
            
            uut.RemoveCondition("native code");

            uut.HasChanged.Should().BeTrue();

            uut.Value.Should().BeEquivalentTo(new HashSet<ConditionValue> { ConditionValue.NormalConditionValue(_mockTimeProvider.Object.Now, "device", "condition") });
        }
        
        [Fact]
        public void RemoveByConditionValue()
        {
            ConditionValue value = new ConditionValue
                {
                    Timestamp = new DateTime(1, 2, 3, 4, 5, 6, DateTimeKind.Utc),
                    DeviceName = "device",
                    ConditionName = "condition",
                    NativeCode = "native code",
                    NativeSeverity = "native severity",
                    Level = ConditionLevel.Normal,
                    Qualifier = ConditionQualifier.None,
                    Message = "message"
                };

            uut.AddCondition(value);

            uut.HasChanged.Should().BeTrue();

            uut.AddToUpdate(new StringBuilder());

            uut.HasChanged.Should().BeFalse();
            
            
            uut.RemoveCondition(value);

            uut.HasChanged.Should().BeTrue();

            uut.Value.Should().BeEquivalentTo(new HashSet<ConditionValue> { ConditionValue.NormalConditionValue(_mockTimeProvider.Object.Now, "device", "condition") });
        }

        [Fact]
        public void AddToUpdate()
        {
            ConditionValue condition1 = new ConditionValue
            {
                Timestamp = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                DeviceName = "device",
                ConditionName = "condition",
                NativeCode = "native code 1",
                NativeSeverity = "native severity 1",
                Level = ConditionLevel.Fault,
                Qualifier = ConditionQualifier.None,
                Message = "message 1"
            };

            ConditionValue condition2 = new ConditionValue
            {
                Timestamp = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                DeviceName = "device",
                ConditionName = "condition",
                NativeCode = "native code 2",
                NativeSeverity = "native severity 2",
                Level = ConditionLevel.Warning,
                Qualifier = ConditionQualifier.None,
                Message = "message 2"
            };

            uut.AddCondition(condition1);

            uut.AddCondition(condition2);

            StringBuilder sb = new StringBuilder();
            uut.AddToUpdate(sb);

            string actual = sb.ToString();
            actual
                .Should()
                .Be("2021-01-02T03:04:05.000000Z|device:condition|FAULT|native code 1|native severity 1||message 1\r\n2021-06-07T08:09:10.000000Z|device:condition|WARNING|native code 2|native severity 2||message 2\r\n");
                //  Enum.GetName(mLevel.GetType(), mLevel) + "|" + mNativeCode + "|" + mNativeSeverity + "|" + mQualifier + "|" + mText
        }

        [Fact]
        public void AddToUpdateNoReEmit()
        {
            ConditionValue condition1 = new ConditionValue
            {
                Timestamp = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                DeviceName = "device",
                ConditionName = "condition",
                NativeCode = "native code 1",
                NativeSeverity = "native severity 1",
                Level = ConditionLevel.Fault,
                Qualifier = ConditionQualifier.None,
                Message = "message 1"
            };

            ConditionValue condition2 = new ConditionValue
            {
                Timestamp = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                DeviceName = "device",
                ConditionName = "condition",
                NativeCode = "native code 2",
                NativeSeverity = "native severity 2",
                Level = ConditionLevel.Warning,
                Qualifier = ConditionQualifier.None,
                Message = "message 2"
            };

            uut.AddCondition(condition1);

            uut.AddCondition(condition2);

            StringBuilder sb = new StringBuilder();
            uut.AddToUpdate(sb);

            string actual = sb.ToString();
            actual
                .Should()
                .Be("2021-01-02T03:04:05.000000Z|device:condition|FAULT|native code 1|native severity 1||message 1\r\n2021-06-07T08:09:10.000000Z|device:condition|WARNING|native code 2|native severity 2||message 2\r\n");
            
            // Should not have any value
            sb.Clear();
            uut.AddToUpdate(sb);
            actual = sb.ToString();
            actual.Should().BeEmpty();
        }
    }
}
// {

//     [TestFixture]
//     public class ConditionTests
//     {
//         Condition c;
//         Condition s;
        
//         [SetUp]
//         public void setup()
//         {
//             c = new Condition("c");
//             s = new Condition("s", true);
//         }

//         [Test]
//         public void should_require_a_newline()
//         {
//             Assert.AreEqual(true, c.NewLine);
//         }

//         [Test]
//         public void should_include_newly_added_activations()
//         {
//             c.Begin();
//             c.Add(Condition.Level.WARNING, "text", "code", "HIGH", "1123");
//             c.Prepare();
//             List<DataItem> list = c.ItemList();

//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("c|WARNING|code|1123|HIGH|text", list[0].ToString());
//         }

//         [Test]
//         public void should_remove_old_activations_after_cleanup()
//         {
//             c.Begin();
//             c.Add(Condition.Level.WARNING, "text", "code");
//             c.Prepare();
//             c.Cleanup();

//             c.Begin();
//             c.Prepare();
//             List<DataItem> list = c.ItemList();
//             c.Cleanup();

//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("c|NORMAL||||", list[0].ToString());

//             list = c.ItemList(true);
//             Assert.AreEqual(1, list.Count);
//         }

//         [Test]
//         public void should_remove_old_activations_and_add_new()
//         {
//             c.Begin();
//             c.Add(Condition.Level.WARNING, "text", "code");
//             c.Prepare();
//             c.Cleanup();

//             c.Begin();
//             c.Add(Condition.Level.WARNING, "text", "code2");
//             c.Prepare();
//             List<DataItem> list = c.ItemList();
//             c.Cleanup();

//             Assert.AreEqual(2, list.Count);
//             Assert.AreEqual("c|NORMAL|code|||", list[0].ToString());
//             Assert.AreEqual("c|WARNING|code2|||text", list[1].ToString());
//         }

//         [Test]
//         public void should_remove_only_old_activations()
//         {
//             c.Begin();
//             c.Add(Condition.Level.WARNING, "text", "code1");
//             c.Add(Condition.Level.FAULT, "text", "code2");
//             c.Prepare();
//             List<DataItem> list = c.ItemList();
//             c.Cleanup();

//             Assert.AreEqual(2, list.Count);

//             c.Begin();
//             c.Add(Condition.Level.FAULT, "text", "code2");
//             c.Prepare();
//             list = c.ItemList();
//             c.Cleanup();

//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("c|NORMAL|code1|||", list[0].ToString());
//             list = c.ItemList(true);
//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("c|FAULT|code2|||text", list[0].ToString());

//             c.Begin();
//             c.Add(Condition.Level.FAULT, "text", "code2");
//             c.Prepare();
//             list = c.ItemList();
//             c.Cleanup();

//             Assert.AreEqual(0, list.Count);

//             list = c.ItemList(true);
//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("c|FAULT|code2|||text", list[0].ToString());

//             c.Begin();
//             c.Prepare();
//             list = c.ItemList();
//             c.Cleanup();

//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("c|NORMAL||||", list[0].ToString());
//         }


//         [Test]
//         public void simple_conditions_should_not_mark_and_sweep()
//         {
//             s.Begin();
//             s.Add(Condition.Level.WARNING, "text", "code");
//             s.Prepare();
//             s.Cleanup();

//             s.Begin();
//             s.Prepare();
//             List<DataItem> list = s.ItemList();
//             s.Cleanup();

//             Assert.AreEqual(0, list.Count);

//             list = s.ItemList(true);
//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("s|WARNING|code|||text", list[0].ToString());
//         }

//         [Test]
//         public void simple_conditions_should_be_cleared_manually()
//         {
//             s.Begin();
//             s.Add(Condition.Level.WARNING, "text", "code");
//             s.Prepare();
//             List<DataItem> list = s.ItemList();
//             s.Cleanup();

//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("s|WARNING|code|||text", list[0].ToString());

//             s.Clear("code");
//             list = s.ItemList(true);
//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("s|NORMAL||||", list[0].ToString());
//         }

//         [Test]
//         public void simple_conditions_should_clear_one_code_when_multiple_are_present()
//         {
//             s.Begin();
//             s.Add(Condition.Level.WARNING, "text1", "code1");
//             s.Add(Condition.Level.FAULT, "text2", "code2");
//             s.Prepare();
//             List<DataItem> list = s.ItemList();
//             s.Cleanup();

//             Assert.AreEqual(2, list.Count);
//             Assert.AreEqual("s|WARNING|code1|||text1", list[0].ToString());
//             Assert.AreEqual("s|FAULT|code2|||text2", list[1].ToString());

//             s.Begin();
//             s.Clear("code1");
//             list = s.ItemList();
//             s.Prepare();
//             s.Cleanup();

//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("s|NORMAL|code1|||", list[0].ToString());

//             list = s.ItemList(true);
//             Assert.AreEqual(1, list.Count);
//             Assert.AreEqual("s|FAULT|code2|||text2", list[0].ToString());
//         }
//     }
// }
