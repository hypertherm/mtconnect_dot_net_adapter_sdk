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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AdapterLab
{
    using MTConnect;
    using MTConnect.Adapter;

    public partial class CuttingToolForm : Form
    {
        MTCAdapter mAdapter;

        public CuttingToolForm(MTCAdapter adapter)
        {
            InitializeComponent();
            mAdapter = adapter;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CuttingTool tool = new CuttingTool(assetId.Text, toolId.Text, serialNumber.Text);
            tool.Description = description.Text;
            tool.Manufacturers = manufacturers.Text;

            List<string> status = new List<string>();
            if (statusUsed.Checked)
                status.Add("USED");
            if (statusNew.Checked)
                status.Add("NEW");
            if (statusAllocated.Checked)
                status.Add("ALLOCATED");
            if (statusMeasured.Checked)
                status.Add("MEASURED");
            if (statusBroken.Checked)
                status.Add("BROKEN");
            tool.AddStatus(status.ToArray());

            CuttingTool.LifeType type = CuttingTool.LifeType.MINUTES;
            if (lifeType.Text == "PART_COUNT")
                type = CuttingTool.LifeType.PART_COUNT;
            else if (lifeType.Text == "WEAR")
                type = CuttingTool.LifeType.WEAR;

            CuttingTool.Direction dir = CuttingTool.Direction.UP;
            if (lifeDirection.Text == "DOWN")
                dir = CuttingTool.Direction.DOWN;

            tool.AddLife(type, dir, lifeValue.Text, lifeInitial.Text, lifeLimit.Text);

            tool.AddProperty("ProcessSpindleSpeed", new string[] 
                 { "nominal", speedNominal.Text,
                   "minimum", speedMin.Text,
                   "maximum", speedMax.Text}, speed.Text);

            tool.AddMeasurement("FunctionalLength", "LF", Double.Parse(lengthVal.Text), Double.Parse(lengthNom.Text),
                Double.Parse(lengthMin.Text), Double.Parse(lengthMax.Text));
            tool.AddMeasurement("CuttingDiameterMax", "DC", Double.Parse(diaVal.Text), Double.Parse(diaNom.Text),
                Double.Parse(diaMin.Text), Double.Parse(diaMax.Text));

            mAdapter.AddAsset(tool);

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
