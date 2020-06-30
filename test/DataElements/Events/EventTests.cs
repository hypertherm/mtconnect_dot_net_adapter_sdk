/*
 * Copyright Copyright 2020, Hypertherm, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using MTConnect.DataElements.Events;
using Xunit;

namespace MTConnect.utests.DataElements.Events
{
    public class EventTests
    {
        [Fact]
        public void NotAvaialbeOnCreation()
        {
            Event<int> uut = new Event<int>("eventName");
            uut.Available.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(StringFormattingData))]
        public void StringTypeToStringFormatting(Event<string> uut, string unavailableOutput, string valueToSet, string availableOutput)
        {
            uut.ToString().Should().Be(unavailableOutput);
            uut.Set(valueToSet);
            uut.ToString().Should().Be(availableOutput);
            uut.Available.Should().BeTrue();
        }

        public class StringFormattingData: IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new Event<string> ("testEvent1"),
                    "testEvent1|UNAVAILABLE",
                    "set a new value",
                    "testEvent1|set a new value"
                };
                yield return new object[] {
                    new Event<string> ("testDevice2", "testEvent2"),
                    "testDevice2:testEvent2|UNAVAILABLE",
                    "set a new value",
                    "testDevice2:testEvent2|set a new value"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(IntFormattingData))]
        public void IntTypeToStringFormatting(Event<int> uut, string unavailableOutput, int valueToSet, string availableOutput)
        {
            uut.ToString().Should().Be(unavailableOutput);
            uut.Set(valueToSet);
            uut.ToString().Should().Be(availableOutput);
            uut.Available.Should().BeTrue();
        }

        public class IntFormattingData: IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new Event<int> ("testEvent1"),
                    "testEvent1|UNAVAILABLE",
                    3,
                    "testEvent1|3"
                };
                yield return new object[] {
                    new Event<int> ("testDevice2", "testEvent2"),
                    "testDevice2:testEvent2|UNAVAILABLE",
                    5,
                    "testDevice2:testEvent2|5"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(DoubleFormattingData))]
        public void DoubleTypeToStringFormatting(Event<double> uut, string unavailableOutput, double valueToSet, string availableOutput)
        {
            uut.ToString().Should().Be(unavailableOutput);
            uut.Set(valueToSet);
            uut.ToString().Should().Be(availableOutput);
            uut.Available.Should().BeTrue();
        }

        public class DoubleFormattingData: IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new Event<double> ("testEvent1"),
                    "testEvent1|UNAVAILABLE",
                    3.1,
                    "testEvent1|3.1"
                };
                yield return new object[] {
                    new Event<double> ("testDevice2", "testEvent2"),
                    "testDevice2:testEvent2|UNAVAILABLE",
                    0.511,
                    "testDevice2:testEvent2|0.511"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(DateTimeFormattingData))]
        public void DateTimeTypeToStringFormatting(Event<DateTime> uut, string unavailableOutput, DateTime valueToSet, string availableOutput)
        {
            uut.ToString().Should().Be(unavailableOutput);
            uut.Set(valueToSet);
            uut.ToString().Should().Be(availableOutput);
            uut.Available.Should().BeTrue();
        }

        public class DateTimeFormattingData: IEnumerable<object[]>
        {
            DateTime testTime = new DateTime(2020, 03, 22, 13, 34, 12, 18, DateTimeKind.Utc);
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new Event<DateTime> ("testEvent1"),
                    "testEvent1|UNAVAILABLE",
                    testTime,
                    "testEvent1|2020-03-22T13:34:12.0180000Z"
                };
                yield return new object[] {
                    new Event<DateTime> ("testDevice2", "testEvent2"),
                    "testDevice2:testEvent2|UNAVAILABLE",
                    testTime,
                    "testDevice2:testEvent2|2020-03-22T13:34:12.0180000Z"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        [Fact]
        public void SetToDefaultMakesUnavailable()
        {
            Event<int> uut = new Event<int>("testElement");
            uut.Set(3);
            uut.Available.Should().BeTrue();
            uut.Set(default(int));
            uut.Available.Should().BeFalse();
        }

        [Fact]
        public void SetUnavailable()
        {
            Event<int> uut = new Event<int>("testElement");
            uut.Set(3);
            uut.Value.Should().Be(3);
            uut.Available.Should().BeTrue();
            uut.SetUnavailable();
            uut.Available.Should().BeFalse();
            uut.Value.Should().Be(default(int));
        }
    }
}
