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
using MTConnect.DataElements.Events;
using Xunit;

namespace MTConnect.utests
{
    public class MessageTests
    {
        
        [Fact]
        public void NotAvaialbeOnCreation()
        {
            Message uut = new Message("eventName");
            uut.Available.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(FormattingData))]
        public void ToStringFormatting(Message uut, string unavailableOutput, MessageValue valueToSet, string availableOutput)
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
                    new Message("testEvent1"),
                    "testEvent1||UNAVAILABLE",
                    new MessageValue{ NativeCode = "test native code", Message = "This is a message"},
                    "testEvent1|test native code|This is a message"
                };
                yield return new object[] {
                    new Message("testDevice2", "testEvent2"),
                    "testDevice2:testEvent2||UNAVAILABLE",
                    new MessageValue{ NativeCode = "test native code", Message = "This is a message"},
                    "testDevice2:testEvent2|test native code|This is a message"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        
        [Fact]
        public void SetToUnavailableMakesUnavailable()
        {
            Message uut = new Message("testElement");
            uut.Set(new MessageValue { NativeCode = "native", Message = "associated message"});
            uut.Available.Should().BeTrue();
            uut.Set(MessageValue.Unavailable);
            uut.Available.Should().BeFalse();
            uut.Value.Should().Be(MessageValue.Unavailable);
        }

        [Fact]
        public void SetUnavailable()
        {
            Message uut = new Message("testElement");
            uut.Set(new MessageValue { NativeCode = "native", Message = "associated message"});
            uut.Available.Should().BeTrue();
            uut.SetUnavailable();
            uut.Available.Should().BeFalse();
            uut.Value.Should().Be(MessageValue.Unavailable);
        }
    }
}
