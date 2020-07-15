/*
 * Copyright Copyright 2012, System Insights, Inc.
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

using System.Xml;

namespace MTConnect.Assets
{
    /// <summary>
    /// Interface for an MTConnect Asset.
    /// </summary>
    public interface IAsset
    {
        /// <value></value>
        string AssetId { get; }
        /// <value></value>
        string AssetType { get; }

        /// <summary>
        /// Write the Asset as an XML
        /// </summary>
        /// <param name="writer">An <see cref="System.Xml.XmlWriter"/> that is used to output the asset structure.</param>
        /// <returns>An <see cref="System.Xml.XmlWriter"/> which is wrapped by pre-existing tags.</returns>
        XmlWriter ToXml(XmlWriter writer);
    }
}