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
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using MTConnect.DataElements.Samples;
using Xunit;

namespace MTConnect.utests.DataElements.Samples
{
    public class TimeSeriesTests
    {
        [Fact]
        public void NotAvaialbeOnCreation()
        {
            TimeSeries uut = new TimeSeries("eventName");
            uut.Available.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(FormattingData))]
        public void ToStringFormatting(TimeSeries uut, string unavailableOutput, IList<double> valueToSet, string availableOutput)
        {
            uut.ToString().Should().Be(unavailableOutput);
            uut.Set(valueToSet);
            uut.ToString().Should().Be(availableOutput);
            uut.Available.Should().BeTrue();
        }

        public class FormattingData: IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new TimeSeries("testTimeSeries1"),
                    "testTimeSeries1|||UNAVAILABLE",
                    new List<double> { 1.0, 2.1333885, 3 },
                    "testTimeSeries1|3||1.0 2.133389 3.0",
                };
                yield return new object[] {
                    new TimeSeries("testTimeSeries1", 4),
                    "testTimeSeries1|||UNAVAILABLE",
                    new List<double> { 1.0, 2.1333885, 3 },
                    "testTimeSeries1|3|4|1.0 2.133389 3.0",
                };
                yield return new object[] {
                    new TimeSeries("testDevice2", "testTimeSeries2", 4),
                    "testDevice2:testTimeSeries2|||UNAVAILABLE",
                    new List<double> { 0.0, 2.1, 3.2 },
                    "testDevice2:testTimeSeries2|3|4|0.0 2.1 3.2"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        
        [Fact]
        public void SetToNullMakesUnavailable()
        {
            TimeSeries uut = new TimeSeries("testElement");
            uut.Set(new List<double> {1.22});
            uut.Available.Should().BeTrue();
            uut.Set(null);
            uut.Available.Should().BeFalse();
            uut.Value.Should().BeNull();
        }

        [Fact]
        public void SetUnavailable()
        {
            TimeSeries uut = new TimeSeries("testElement");
            uut.Set(new List<double> {1.22});
            uut.Available.Should().BeTrue();
            uut.SetUnavailable();
            uut.Available.Should().BeFalse();
            uut.Value.Should().BeNull();
        }
    }
}
